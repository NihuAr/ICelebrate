using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Kursachzhoska.Models;

namespace Kursachzhoska.Services
{
    public class ApiService
    {
        private static ApiService _instance;
        public static ApiService Instance => _instance ??= new ApiService();

        private readonly HttpClient _http;
        private string _accessToken;
        private string _refreshToken;
        private string _deviceToken;
        private const string BaseUrl = "http://localhost:8000/api/";

        public User CurrentUser { get; set; }
        public bool IsOrganizerMode { get; set; }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private ApiService()
        {
            _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        private void SetAuth()
        {
            if (!string.IsNullOrEmpty(_accessToken))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        private StringContent JsonBody(object obj)
        {
            var json = JsonSerializer.Serialize(obj, JsonOpts);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private async Task<T> ReadAs<T>(HttpResponseMessage resp)
        {
            var body = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(body, JsonOpts);
        }

        public async Task<string> GetErrorMessageAsync(HttpResponseMessage resp)
        {
            try
            {
                var content = await resp.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(content))
                {
                    if (doc.RootElement.TryGetProperty("error", out var errProp))
                    {
                        return errProp.GetString() ?? "Неизвестная ошибка";
                    }
                    if (doc.RootElement.TryGetProperty("detail", out var detailProp))
                    {
                        return detailProp.GetString() ?? "Неизвестная ошибка";
                    }
                    if (doc.RootElement.TryGetProperty("message", out var msgProp))
                    {
                        return msgProp.GetString() ?? "Неизвестная ошибка";
                    }
                }
                return content;
            }
            catch
            {
                return $"Ошибка {(int)resp.StatusCode}: {resp.ReasonPhrase}";
            }
        }

        // ─── Auth ───────────────────────────────────────────────

        // Login step 1: returns requires_otp + otp_id/device_token if needed
        public async Task<LoginStep1Response> LoginStep1Async(string email, string password)
        {
            var resp = await _http.PostAsync("auth/login/", JsonBody(new { email, password, device_token = _deviceToken }));
            if (!resp.IsSuccessStatusCode)
            {
                return new LoginStep1Response { Success = false, Error = await GetErrorMessageAsync(resp) };
            }

            var data = await ReadAs<LoginStep1ApiResponse>(resp);
            if (data.RequiresOtp)
            {
                return new LoginStep1Response { Success = true, RequiresOtp = true, OtpId = data.OtpId };
            }
            _accessToken = data.Access;
            _refreshToken = data.Refresh;
            _deviceToken = data.DeviceToken;
            SetAuth();
            await LoadProfileAsync();
            return new LoginStep1Response { Success = true, RequiresOtp = false };
        }

        // Login step 2: verify OTP
        public async Task<bool> LoginStep2Async(string otpId, string code, bool rememberDevice)
        {
            var resp = await _http.PostAsync("auth/login/otp/", JsonBody(new { otp_id = otpId, code, remember_device = rememberDevice }));
            if (!resp.IsSuccessStatusCode) return false;
            var data = await ReadAs<LoginStep2ApiResponse>(resp);
            _accessToken = data.Access;
            _refreshToken = data.Refresh;
            if (!string.IsNullOrEmpty(data.DeviceToken)) _deviceToken = data.DeviceToken;
            SetAuth();
            await LoadProfileAsync();
            return true;
        }

        public async Task<bool> RegisterAsync(string name, string surname, string email,
            string password, string phone, DateTime dob, List<string> preferences)
        {
            var payload = new
            {
                username = email,
                email,
                password,
                first_name = name,
                last_name = surname,
                phone_number = phone,
                date_of_birth = dob.ToString("yyyy-MM-dd"),
                preferences = string.Join(",", preferences)
            };

            var resp = await _http.PostAsync("auth/register/", JsonBody(payload));
            return resp.IsSuccessStatusCode;
        }

        public async Task<string> RequestEmailVerificationAsync(string email)
        {
            var resp = await _http.PostAsync("auth/verify-email/request/", JsonBody(new { email }));
            if (!resp.IsSuccessStatusCode) return null;

            try
            {
                var content = await resp.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(content))
                {
                    if (doc.RootElement.TryGetProperty("otp_id", out var idProp))
                    {
                        if (idProp.ValueKind == JsonValueKind.Number)
                            return idProp.GetInt32().ToString();
                        return idProp.GetString();
                    }
                }
            }
            catch { }
            return null;
        }

        public async Task<bool> ConfirmEmailAsync(string otpId, string code)
        {
            var resp = await _http.PostAsync("auth/verify-email/confirm/", JsonBody(new { otp_id = otpId, code }));
            if (!resp.IsSuccessStatusCode) return false;
            await LoadProfileAsync();
            return true;
        }

        public async Task<string> RequestPasswordResetAsync(string email)
        {
            var resp = await _http.PostAsync("auth/password-reset/request/", JsonBody(new { email }));
            if (!resp.IsSuccessStatusCode) return null;

            try
            {
                var content = await resp.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(content))
                {
                    if (doc.RootElement.TryGetProperty("otp_id", out var idProp))
                    {
                        if (idProp.ValueKind == JsonValueKind.Number)
                            return idProp.GetInt32().ToString();
                        return idProp.GetString();
                    }
                }
            }
            catch { }
            return null;
        }

        public async Task<bool> ConfirmPasswordResetAsync(string otpId, string code, string newPassword)
        {
            var resp = await _http.PostAsync("auth/password-reset/confirm/", JsonBody(new { otp_id = otpId, code, new_password = newPassword }));
            return resp.IsSuccessStatusCode;
        }

        // Backward-compatible simple login (no OTP support). Returns false if OTP is required.
        public async Task<bool> LoginAsync(string username, string password)
        {
            var step1 = await LoginStep1Async(username, password);
            if (!step1.Success || step1.RequiresOtp)
                return false;
            return true;
        }

        public async Task<bool> Toggle2FAAsync(bool enable, string password = null)
        {
            SetAuth();
            object payload;
            if (enable)
                payload = new { action = "enable", password };
            else
                payload = new { action = "disable" };
            var resp = await _http.PostAsync("auth/2fa/toggle/", JsonBody(payload));
            if (!resp.IsSuccessStatusCode) return false;
            await LoadProfileAsync();
            return true;
        }

        public async Task<bool> LeaveOrganizerAsync()
        {
            SetAuth();
            var resp = await _http.PostAsync("auth/leave-organizer/", null);
            if (!resp.IsSuccessStatusCode) return false;
            await LoadProfileAsync();
            return true;
        }

        public async Task LoadProfileAsync()
        {
            SetAuth();
            var resp = await _http.GetAsync("auth/profile/");
            if (!resp.IsSuccessStatusCode) return;

            var profile = await ReadAs<ApiUser>(resp);
            CurrentUser = MapUser(profile);
        }

        public async Task UpdateProfileAsync()
        {
            if (CurrentUser == null) return;
            SetAuth();

            var payload = new
            {
                first_name = CurrentUser.Name,
                last_name = CurrentUser.Surname,
                phone_number = CurrentUser.PhoneNumber,
                date_of_birth = CurrentUser.DateOfBirth.ToString("yyyy-MM-dd"),
                preferences = string.Join(",", CurrentUser.Preferences)
            };

            await _http.PatchAsync("auth/profile/", JsonBody(payload));
        }

        public async Task BecomeOrganizerAsync(string companyName, List<string> categories)
        {
            SetAuth();
            var payload = new { company_name = companyName, organizer_categories = string.Join(",", categories) };
            var resp = await _http.PostAsync("auth/become-organizer/", JsonBody(payload));
            if (resp.IsSuccessStatusCode)
                await LoadProfileAsync();
        }

        public void Logout()
        {
            _accessToken = null;
            _refreshToken = null;
            CurrentUser = null;
            _http.DefaultRequestHeaders.Authorization = null;
        }

        // ─── Events ─────────────────────────────────────────────

        public async Task<List<Event>> GetAllEventsAsync()
        {
            SetAuth();
            var resp = await _http.GetAsync("events/?upcoming=true");
            if (!resp.IsSuccessStatusCode) return new List<Event>();

            var data = await ReadAs<PaginatedResponse<ApiEvent>>(resp);
            var list = data?.Results ?? new List<ApiEvent>();
            return list.Select(MapEvent).ToList();
        }

        public async Task<List<Event>> GetEventsByCategoryAsync(string category)
        {
            SetAuth();
            var url = category == "All" || category == "Все"
                ? "events/?upcoming=true"
                : $"events/?upcoming=true&category={Uri.EscapeDataString(category)}";

            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return new List<Event>();

            var data = await ReadAs<PaginatedResponse<ApiEvent>>(resp);
            return (data?.Results ?? new List<ApiEvent>()).Select(MapEvent).ToList();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            SetAuth();
            var resp = await _http.GetAsync($"events/{id}/");
            if (!resp.IsSuccessStatusCode) return null;
            var apiEvent = await ReadAs<ApiEvent>(resp);
            return MapEvent(apiEvent);
        }

        public async Task<List<Event>> GetEventsByOrganizerAsync(int organizerId)
        {
            SetAuth();
            var resp = await _http.GetAsync($"events/?organizer={organizerId}");
            if (!resp.IsSuccessStatusCode) return new List<Event>();
            var data = await ReadAs<PaginatedResponse<ApiEvent>>(resp);
            return (data?.Results ?? new List<ApiEvent>()).Select(MapEvent).ToList();
        }

        public async Task AddEventAsync(Event evt)
        {
            SetAuth();
            var payload = new
            {
                title = evt.Title,
                description = evt.Description,
                date_time = evt.DateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                duration_hours = 2.0,
                location = evt.Location,
                price = evt.Price,
                payment_method = evt.Price == 0 ? "free" : "both",
                color = evt.ImageUrl,
                visible_only_for_creator = evt.VisibleOnlyForCreator
            };
            await _http.PostAsync("events/", JsonBody(payload));
        }

        public async Task UpdateEventAsync(Event evt)
        {
            SetAuth();
            var payload = new
            {
                title = evt.Title,
                description = evt.Description,
                date_time = evt.DateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                duration_hours = 2.0,
                location = evt.Location,
                price = evt.Price,
                payment_method = evt.Price == 0 ? "free" : "both",
                color = evt.ImageUrl,
                visible_only_for_creator = evt.VisibleOnlyForCreator
            };
            await _http.PatchAsync($"events/{evt.EventId}/", JsonBody(payload));
        }

        public async Task DeleteEventAsync(int id)
        {
            SetAuth();
            await _http.DeleteAsync($"events/{id}/");
        }

        // ─── Favorites ──────────────────────────────────────────

        public async Task ToggleFavoriteAsync(int eventId)
        {
            SetAuth();
            await _http.PostAsync($"events/{eventId}/toggle_favorite/", null);
        }

        public async Task<List<Event>> GetFavoriteEventsAsync()
        {
            SetAuth();
            var resp = await _http.GetAsync("favorites/");
            if (!resp.IsSuccessStatusCode) return new List<Event>();

            var data = await ReadAs<PaginatedResponse<ApiFavorite>>(resp);
            return (data?.Results ?? new List<ApiFavorite>()).Select(f => MapEvent(f.Event)).ToList();
        }

        public async Task<bool> IsEventFavoriteAsync(int eventId)
        {
            var evt = await GetEventByIdAsync(eventId);
            return evt?.IsFavorite ?? false;
        }

        // ─── Applications ───────────────────────────────────────

        public async Task ApplyToEventAsync(int eventId)
        {
            SetAuth();
            await _http.PostAsync("applications/", JsonBody(new { @event = eventId }));
        }

        public async Task<List<ApiApplication>> GetMyApplicationsAsync()
        {
            SetAuth();
            var resp = await _http.GetAsync("applications/my/");
            if (!resp.IsSuccessStatusCode) return new List<ApiApplication>();
            var data = await ReadAs<PaginatedResponse<ApiApplication>>(resp);
            return data?.Results ?? new List<ApiApplication>();
        }

        public List<ApiApplication> GetMyApplications()
        {
            return Task.Run(GetMyApplicationsAsync).Result;
        }

        public async Task<List<ApiApplication>> GetEventApplicationsAsync(int eventId)
        {
            SetAuth();
            var resp = await _http.GetAsync($"events/{eventId}/applications/");
            if (!resp.IsSuccessStatusCode) return new List<ApiApplication>();
            var data = await ReadAs<PaginatedResponse<ApiApplication>>(resp);
            return data?.Results ?? new List<ApiApplication>();
        }

        public async Task ApplicationActionAsync(int appId, string action)
        {
            SetAuth();
            await _http.PatchAsync($"applications/{appId}/action/", JsonBody(new { action }));
        }

        // ─── Notifications ──────────────────────────────────────

        public async Task<List<Notification>> GetNotificationsAsync()
        {
            SetAuth();
            var resp = await _http.GetAsync("notifications/");
            if (!resp.IsSuccessStatusCode) return new List<Notification>();
            var data = await ReadAs<PaginatedResponse<ApiNotification>>(resp);
            return (data?.Results ?? new List<ApiNotification>()).Select(MapNotification).ToList();
        }

        public async Task MarkAllNotificationsReadAsync()
        {
            SetAuth();
            await _http.PostAsync("notifications/mark_all_read/", null);
        }

        public async Task ClearNotificationsAsync()
        {
            SetAuth();
            await _http.PostAsync("notifications/clear_all/", null);
        }

        // ─── Reviews ────────────────────────────────────────────

        public async Task<List<Review>> GetEventReviewsAsync(int eventId)
        {
            SetAuth();
            var resp = await _http.GetAsync($"reviews/?event={eventId}");
            if (!resp.IsSuccessStatusCode) return new List<Review>();
            var data = await ReadAs<PaginatedResponse<ApiReview>>(resp);
            return (data?.Results ?? new List<ApiReview>()).Select(MapReview).ToList();
        }

        public async Task<List<Review>> GetOrganizerReviewsAsync(int organizerId)
        {
            var events = await GetEventsByOrganizerAsync(organizerId);
            var all = new List<Review>();
            foreach (var e in events)
            {
                var revs = await GetEventReviewsAsync(e.EventId);
                all.AddRange(revs);
            }
            return all;
        }

        public async Task<List<Review>> GetUserReviewsAsync(int userId)
        {
            SetAuth();
            var resp = await _http.GetAsync("reviews/");
            if (!resp.IsSuccessStatusCode) return new List<Review>();

            var data = await ReadAs<PaginatedResponse<ApiReview>>(resp);
            var list = (data?.Results ?? new List<ApiReview>()).Select(MapReview).ToList();
            return list.Where(r => r.UserId == userId).ToList();
        }

        public async Task<bool> HasUserReviewedEventAsync(int userId, int eventId)
        {
            var reviews = await GetEventReviewsAsync(eventId);
            return reviews.Any(r => r.UserId == userId);
        }

        public async Task AddReviewAsync(int eventId, int rating, string comment)
        {
            SetAuth();
            await _http.PostAsync("reviews/", JsonBody(new { @event = eventId, rating, comment }));
        }

        public async Task AddReviewAsync(Review review)
        {
            await AddReviewAsync(review.EventId, review.Rating, review.Comment);
        }

        // ─── Categories ─────────────────────────────────────────

        public async Task<List<ApiCategory>> GetCategoriesAsync()
        {
            var resp = await _http.GetAsync("categories/");
            if (!resp.IsSuccessStatusCode) return new List<ApiCategory>();
            return await ReadAs<List<ApiCategory>>(resp);
        }

        // ─── Promos ─────────────────────────────────────────────

        public async Task<List<Promo>> GetPromosAsync()
        {
            SetAuth();
            var resp = await _http.GetAsync("promos/");
            if (!resp.IsSuccessStatusCode) return new List<Promo>();
            var data = await ReadAs<PaginatedResponse<ApiPromo>>(resp);
            return (data?.Results ?? new List<ApiPromo>()).Select(MapPromo).ToList();
        }

        public async Task<List<UserPromo>> GetMyPromosAsync()
        {
            SetAuth();
            var resp = await _http.GetAsync("my-promos/");
            if (!resp.IsSuccessStatusCode) return new List<UserPromo>();
            var data = await ReadAs<PaginatedResponse<ApiUserPromo>>(resp);
            return (data?.Results ?? new List<ApiUserPromo>()).Select(MapUserPromo).ToList();
        }

        public async Task<bool> PurchasePromoAsync(int promoId)
        {
            SetAuth();
            var resp = await _http.PostAsync($"promos/{promoId}/purchase/", null);
            if (resp.IsSuccessStatusCode)
            {
                await LoadProfileAsync();
                return true;
            }
            return false;
        }

        public async Task<UserPromo> GetPromoQRAsync(int userPromoId)
        {
            SetAuth();
            var resp = await _http.GetAsync($"my-promos/{userPromoId}/qr/");
            if (!resp.IsSuccessStatusCode) return null;
            var data = await ReadAs<ApiPromoQR>(resp);
            return new UserPromo
            {
                UniqueCode = data.UniqueCode,
                QRImageBase64 = data.QRImage
            };
        }

        public async Task<bool> CreatePromoAsync(Promo promo)
        {
            SetAuth();
            var payload = new
            {
                title = promo.Title,
                description = promo.Description,
                bonus_price = promo.BonusPrice,
                usage_limit = promo.UsageLimit,
                valid_until = promo.ValidUntil?.ToString("yyyy-MM-ddTHH:mm:ss")
            };
            var resp = await _http.PostAsync("promos/", JsonBody(payload));
            return resp.IsSuccessStatusCode;
        }

        // ─── Bonus ──────────────────────────────────────────────

        public async Task<List<BonusTransaction>> GetBonusHistoryAsync()
        {
            SetAuth();
            var resp = await _http.GetAsync("bonus/history/");
            if (!resp.IsSuccessStatusCode) return new List<BonusTransaction>();
            var data = await ReadAs<PaginatedResponse<ApiBonusTransaction>>(resp);
            return (data?.Results ?? new List<ApiBonusTransaction>()).Select(MapBonusTransaction).ToList();
        }

        // ─── Mapping helpers ────────────────────────────────────

        private User MapUser(ApiUser u)
        {
            return new User
            {
                UserId = u.Id,
                Name = u.FirstName ?? "",
                Surname = u.LastName ?? "",
                Email = u.Email ?? "",
                PhoneNumber = u.PhoneNumber ?? "",
                DateOfBirth = DateTime.TryParse(u.DateOfBirth, out var d) ? d : DateTime.MinValue,
                Preferences = (u.Preferences ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                IsOrganizer = u.IsOrganizer,
                CompanyName = u.CompanyName,
                OrganizerCategories = u.OrganizerCategories ?? new List<string>(),
                FavoriteEventIds = new List<int>(),
                BonusBalance = u.BonusBalance,
                Is2FAEnabled = u.Is2FAEnabled,
                IsEmailVerified = u.IsEmailVerified
            };
        }

        private Event MapEvent(ApiEvent e)
        {
            return new Event
            {
                EventId = e.Id,
                Title = e.Title ?? "",
                Description = e.Description ?? "",
                DateTime = DateTime.TryParse(e.DateTime, out var d) ? d : DateTime.MinValue,
                Location = e.Location ?? "",
                Price = decimal.TryParse(e.Price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p) ? p : 0,
                Category = e.Category?.Name ?? "",
                ImageUrl = e.Color ?? "#6366F1",
                OrganizerId = e.Organizer?.Id ?? 0,
                OrganizerName = ($"{e.Organizer?.FirstName} {e.Organizer?.LastName}").Trim(),
                OrganizerCompanyName = e.Organizer?.CompanyName ?? "",
                IsFavorite = e.IsFavorite,
                Rating = e.AverageRating ?? 0,
                Reviews = new List<Review>(),
                Participants = new List<EventParticipant>(),
                VisibleOnlyForCreator = e.VisibleOnlyForCreator
            };
        }

        private Notification MapNotification(ApiNotification n)
        {
            return new Notification
            {
                NotificationId = n.Id,
                UserId = CurrentUser?.UserId ?? 0,
                Title = n.Title ?? "",
                Message = n.Message ?? "",
                Date = DateTime.TryParse(n.CreatedAt, out var d) ? d : DateTime.Now,
                RequiresConfirmation = !n.IsRead,
                IsConfirmed = n.IsRead ? true : null
            };
        }

        private Review MapReview(ApiReview r)
        {
            return new Review
            {
                ReviewId = r.Id,
                EventId = r.Event,
                UserId = r.User?.Id ?? 0,
                UserName = $"{r.User?.FirstName} {r.User?.LastName}".Trim(),
                Rating = r.Rating,
                Comment = r.Comment ?? "",
                CreatedDate = DateTime.TryParse(r.CreatedAt, out var d) ? d : DateTime.Now
            };
        }

        private Promo MapPromo(ApiPromo p)
        {
            return new Promo
            {
                PromoId = p.Id,
                OrganizerId = p.Organizer?.Id ?? 0,
                OrganizerName = $"{p.Organizer?.FirstName} {p.Organizer?.LastName}".Trim(),
                OrganizerCompanyName = p.Organizer?.CompanyName ?? "",
                Title = p.Title ?? "",
                Description = p.Description ?? "",
                BonusPrice = p.BonusPrice,
                UsageLimit = p.UsageLimit,
                ValidUntil = DateTime.TryParse(p.ValidUntil, out var d) ? d : (DateTime?)null,
                IsActive = p.IsActive,
                IsAvailable = p.IsAvailable,
                PurchasesCount = p.PurchasesCount,
                CreatedAt = DateTime.TryParse(p.CreatedAt, out var c) ? c : DateTime.Now
            };
        }

        private UserPromo MapUserPromo(ApiUserPromo up)
        {
            return new UserPromo
            {
                UserPromoId = up.Id,
                UserId = CurrentUser?.UserId ?? 0,
                PromoId = up.Promo?.Id ?? 0,
                PromoTitle = up.Promo?.Title ?? "",
                UniqueCode = up.UniqueCode ?? "",
                Status = up.Status ?? "active",
                PurchasedAt = DateTime.TryParse(up.PurchasedAt, out var d) ? d : DateTime.Now
            };
        }

        private BonusTransaction MapBonusTransaction(ApiBonusTransaction bt)
        {
            return new BonusTransaction
            {
                TransactionId = bt.Id,
                UserId = CurrentUser?.UserId ?? 0,
                Amount = bt.Amount,
                Reason = bt.Reason ?? "",
                Description = bt.Description ?? "",
                EventId = bt.Event,
                CreatedAt = DateTime.TryParse(bt.CreatedAt, out var d) ? d : DateTime.Now
            };
        }

        // ─── Sync wrappers for backward compatibility ───────────

        public bool ValidateLogin(string login, string password)
        {
            return Task.Run(() => LoginAsync(login, password)).Result;
        }

        public void RegisterUser(User user)
        {
            Task.Run(() => RegisterAsync(
                user.Name, user.Surname, user.Email,
                user.Password, user.PhoneNumber, user.DateOfBirth,
                user.Preferences
            )).Wait();
        }

        public List<Event> GetAllEvents()
        {
            return Task.Run(GetAllEventsAsync).Result;
        }

        public List<Event> GetEventsByCategory(string category)
        {
            return Task.Run(() => GetEventsByCategoryAsync(category)).Result;
        }

        public Event GetEventById(int id)
        {
            return Task.Run(() => GetEventByIdAsync(id)).Result;
        }

        public List<Event> GetEventsByOrganizer(int organizerId)
        {
            return Task.Run(() => GetEventsByOrganizerAsync(organizerId)).Result;
        }

        public List<Event> GetOrganizerFutureEvents(int organizerId)
        {
            var events = GetEventsByOrganizer(organizerId);
            return events.Where(e => e.DateTime >= DateTime.Now).OrderBy(e => e.DateTime).ToList();
        }

        public List<Event> GetOrganizerPastEvents(int organizerId)
        {
            var events = GetEventsByOrganizer(organizerId);
            return events.Where(e => e.DateTime < DateTime.Now).OrderByDescending(e => e.DateTime).ToList();
        }

        public void AddEvent(Event evt)
        {
            Task.Run(() => AddEventAsync(evt)).Wait();
        }

        public void UpdateEvent(Event evt)
        {
            Task.Run(() => UpdateEventAsync(evt)).Wait();
        }

        public void DeleteEvent(int id)
        {
            Task.Run(() => DeleteEventAsync(id)).Wait();
        }

        public void UpdateCurrentUser()
        {
            Task.Run(UpdateProfileAsync).Wait();
        }

        public void ToggleFavoriteEvent(int eventId)
        {
            Task.Run(() => ToggleFavoriteAsync(eventId)).Wait();
        }

        public bool IsEventFavorite(int eventId)
        {
            return Task.Run(() => IsEventFavoriteAsync(eventId)).Result;
        }

        public List<Event> GetFavoriteEvents()
        {
            return Task.Run(GetFavoriteEventsAsync).Result;
        }

        public List<Notification> GetUserNotifications(int userId)
        {
            return Task.Run(GetNotificationsAsync).Result;
        }

        public void AddNotification(Notification n)
        {
            // Notifications created server-side, stub
        }

        public void UpdateNotification(Notification n)
        {
            // No individual update via API, stub
        }

        public void DeleteNotification(int id)
        {
            // Stub
        }

        public void ClearUserNotifications(int userId)
        {
            Task.Run(ClearNotificationsAsync).Wait();
        }

        public List<Review> GetEventReviews(int eventId)
        {
            return Task.Run(() => GetEventReviewsAsync(eventId)).Result;
        }

        public void AddReview(Review review)
        {
            Task.Run(() => AddReviewAsync(review.EventId, review.Rating, review.Comment)).Wait();
        }

        public List<Review> GetOrganizerReviews(int organizerId)
        {
            var events = GetEventsByOrganizer(organizerId);
            var all = new List<Review>();
            foreach (var e in events)
                all.AddRange(GetEventReviews(e.EventId));
            return all.OrderByDescending(r => r.CreatedDate).ToList();
        }

        public List<Review> GetUserReviews(int userId)
        {
            // Would need a server-side filter; stub returns empty
            return new List<Review>();
        }

        public bool HasUserReviewedEvent(int userId, int eventId)
        {
            var reviews = GetEventReviews(eventId);
            return reviews.Any(r => r.UserId == userId);
        }

        public double GetEventAverageRating(int eventId)
        {
            var reviews = GetEventReviews(eventId);
            return reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating);
        }

        public double GetOrganizerAverageRating(int organizerId)
        {
            var reviews = GetOrganizerReviews(organizerId);
            return reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating);
        }

        public int GetOrganizerReviewsCount(int organizerId)
        {
            return GetOrganizerReviews(organizerId).Count;
        }

        public void DeleteUserReviews(int userId) { }
        public void DeleteUserReviewsByEmail(string email) { }
        public List<User> GetAllUsers() => new List<User>();
        public void DeleteUser(int userId) { }

        // ─── Sync wrappers for Promos and Bonus ───────────────────

        public List<Promo> GetPromos()
        {
            return Task.Run(GetPromosAsync).Result;
        }

        public List<UserPromo> GetMyPromos()
        {
            return Task.Run(GetMyPromosAsync).Result;
        }

        public bool PurchasePromo(int promoId)
        {
            return Task.Run(() => PurchasePromoAsync(promoId)).Result;
        }

        public UserPromo GetPromoQR(int userPromoId)
        {
            return Task.Run(() => GetPromoQRAsync(userPromoId)).Result;
        }

        public bool CreatePromo(Promo promo)
        {
            return Task.Run(() => CreatePromoAsync(promo)).Result;
        }

        public List<BonusTransaction> GetBonusHistory()
        {
            return Task.Run(GetBonusHistoryAsync).Result;
        }

        // ─── Applications ────────────────────────────────────

        public List<UserApplication> GetUserApplications()
        {
            return Task.Run(GetUserApplicationsAsync).Result;
        }

        public async Task<List<UserApplication>> GetUserApplicationsAsync()
        {
            SetAuth();
            var resp = await _http.GetAsync("applications/my/");
            if (!resp.IsSuccessStatusCode) return new List<UserApplication>();
            var data = await ReadAs<PaginatedResponse<ApiApplication>>(resp);
            return (data?.Results ?? new List<ApiApplication>()).Select(MapApplication).ToList();
        }

        public bool CancelApplication(int applicationId)
        {
            return Task.Run(() => CancelApplicationAsync(applicationId)).Result;
        }

        public async Task<bool> CancelApplicationAsync(int applicationId)
        {
            SetAuth();
            await ApplicationActionAsync(applicationId, "cancel");
            return true;
        }

        private UserApplication MapApplication(ApiApplication a)
        {
            return new UserApplication
            {
                ApplicationId = a.Id,
                UserId = CurrentUser?.UserId ?? 0,
                EventId = a.Event,
                EventTitle = a.EventTitle ?? string.Empty,
                Status = a.Status ?? "pending",
                Attendance = a.Attendance ?? "unknown",
                AppliedDate = DateTime.TryParse(a.CreatedAt, out var d) ? d : DateTime.Now
            };
        }
    }

    // ─── API DTOs ────────────────────────────────────────────

    public class TokenResponse
    {
        [JsonPropertyName("access")]
        public string Access { get; set; }
        [JsonPropertyName("refresh")]
        public string Refresh { get; set; }
    }

    public class LoginStep1ApiResponse
    {
        [JsonPropertyName("requires_otp")]
        public bool RequiresOtp { get; set; }
        [JsonPropertyName("otp_id")]
        public string OtpId { get; set; }
        [JsonPropertyName("access")]
        public string Access { get; set; }
        [JsonPropertyName("refresh")]
        public string Refresh { get; set; }
        [JsonPropertyName("device_token")]
        public string DeviceToken { get; set; }
    }

    public class LoginStep2ApiResponse
    {
        [JsonPropertyName("access")]
        public string Access { get; set; }
        [JsonPropertyName("refresh")]
        public string Refresh { get; set; }
        [JsonPropertyName("device_token")]
        public string DeviceToken { get; set; }
    }

    public class LoginStep1Response
    {
        public bool Success { get; set; }
        public bool RequiresOtp { get; set; }
        public string OtpId { get; set; }
        public string Error { get; set; }
    }

    public class PaginatedResponse<T>
    {
        [JsonPropertyName("results")]
        public List<T> Results { get; set; }
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class ApiUser
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public string LastName { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }
        [JsonPropertyName("date_of_birth")]
        public string DateOfBirth { get; set; }
        [JsonPropertyName("preferences")]
        public string Preferences { get; set; }
        [JsonPropertyName("is_organizer")]
        public bool IsOrganizer { get; set; }
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }
        [JsonPropertyName("organizer_categories")]
        public List<string> OrganizerCategories { get; set; }
        [JsonPropertyName("bonus_balance")]
        public int BonusBalance { get; set; }
        [JsonPropertyName("profile_image")]
        public string ProfileImage { get; set; }
        [JsonPropertyName("is_2fa_enabled")]
        public bool Is2FAEnabled { get; set; }
        [JsonPropertyName("is_email_verified")]
        public bool IsEmailVerified { get; set; }
    }

    public class ApiCategory
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class ApiEvent
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("date_time")]
        public string DateTime { get; set; }
        [JsonPropertyName("duration_hours")]
        public string DurationHours { get; set; }
        [JsonPropertyName("location")]
        public string Location { get; set; }
        [JsonPropertyName("price")]
        public string Price { get; set; }
        [JsonPropertyName("color")]
        public string Color { get; set; }
        [JsonPropertyName("image")]
        public string Image { get; set; }
        [JsonPropertyName("category")]
        public ApiCategory Category { get; set; }
        [JsonPropertyName("organizer")]
        public ApiUserPublic Organizer { get; set; }
        [JsonPropertyName("is_favorite")]
        public bool IsFavorite { get; set; }
        [JsonPropertyName("bonus_points")]
        public int BonusPoints { get; set; }
        [JsonPropertyName("average_rating")]
        public double? AverageRating { get; set; }
        [JsonPropertyName("visible_only_for_creator")]
        public bool VisibleOnlyForCreator { get; set; }
    }

    public class ApiUserPublic
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public string LastName { get; set; }
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }
    }

    public class ApiFavorite
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("event")]
        public ApiEvent Event { get; set; }
    }

    public class ApiApplication
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("event")]
        public int Event { get; set; }
        [JsonPropertyName("event_title")]
        public string EventTitle { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("attendance")]
        public string Attendance { get; set; }
        [JsonPropertyName("user")]
        public ApiUserPublic User { get; set; }
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }

    public class ApiNotification
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("is_read")]
        public bool IsRead { get; set; }
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }

    public class ApiReview
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("event")]
        public int Event { get; set; }
        [JsonPropertyName("user")]
        public ApiUserPublic User { get; set; }
        [JsonPropertyName("rating")]
        public int Rating { get; set; }
        [JsonPropertyName("comment")]
        public string Comment { get; set; }
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }

    public class ApiPromo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("organizer")]
        public ApiUserPublic Organizer { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("bonus_price")]
        public int BonusPrice { get; set; }
        [JsonPropertyName("usage_limit")]
        public int UsageLimit { get; set; }
        [JsonPropertyName("valid_until")]
        public string ValidUntil { get; set; }
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; }
        [JsonPropertyName("purchases_count")]
        public int PurchasesCount { get; set; }
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }

    public class ApiUserPromo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("promo")]
        public ApiPromo Promo { get; set; }
        [JsonPropertyName("unique_code")]
        public string UniqueCode { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("purchased_at")]
        public string PurchasedAt { get; set; }
    }

    public class ApiPromoQR
    {
        [JsonPropertyName("unique_code")]
        public string UniqueCode { get; set; }
        [JsonPropertyName("qr_image")]
        public string QRImage { get; set; }
    }

    public class ApiBonusTransaction
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
        [JsonPropertyName("reason")]
        public string Reason { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("event")]
        public int? Event { get; set; }
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }
}
