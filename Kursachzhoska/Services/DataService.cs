using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Kursachzhoska.Models;

namespace Kursachzhoska.Services
{
    public class DataService
    {
        private static DataService _instance;
        public static DataService Instance => _instance ??= new DataService();

        public User CurrentUser { get; set; }
        public bool IsOrganizerMode { get; set; }

        private List<Event> _events;
        private List<User> _users;
        private List<Notification> _notifications;
        private List<Review> _reviews;
        
        private readonly string _usersFilePath;
        private readonly string _eventsFilePath;
        private readonly string _notificationsFilePath;
        private readonly string _reviewsFilePath;

        private DataService()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "When2Meet"
            );
            
            _usersFilePath = Path.Combine(appDataPath, "users.json");
            _eventsFilePath = Path.Combine(appDataPath, "events.json");
            _notificationsFilePath = Path.Combine(appDataPath, "notifications.json");
            _reviewsFilePath = Path.Combine(appDataPath, "reviews.json");
            
            LoadDataFromFile();
            InitializeMockData();
        }

        private void InitializeMockData()
        {
            // Add test user only if list is empty
            if (_users == null || _users.Count == 0)
            {
                _users = new List<User>
                {
                    new User
                    {
                        UserId = 1,
                        Name = "Katerina",
                        Surname = "Voronovskaya",
                        Email = "katieyka@gmail.com",
                        PhoneNumber = "+375 29 965 22 08",
                        DateOfBirth = new DateTime(2006, 10, 3),
                        Preferences = new List<string> { "Sport", "IT" },
                        Password = "password123"
                    },
                    new User
                    {
                        UserId = 2,
                        Name = "Artem",
                        Surname = "Mekena",
                        Email = "artemmekena@gmail.com",
                        PhoneNumber = "+375 29 123 45 67",
                        DateOfBirth = new DateTime(1995, 5, 15),
                        Preferences = new List<string> { "IT", "Art", "Sport" },
                        Password = "25389658"
                    }
                };
                SaveUsersToFile();
            }

            // CurrentUser will be set after login or registration

            // Add test events only if list is empty
            if (_events == null || _events.Count == 0)
            {
                _events = new List<Event>
            {
                // Upcoming events
                new Event
                {
                    EventId = 1,
                    Title = "Hatha yoga",
                    Description = "Join us for a relaxing yoga session",
                    DateTime = new DateTime(2025, 12, 23, 19, 0, 0),
                    Location = "Masherova st., 10",
                    Category = "Sport",
                    ImageUrl = "#8FBC8F",
                    Price = 0
                },
                new Event
                {
                    EventId = 2,
                    Title = "Readers club",
                    Description = "Discuss your favorite books",
                    DateTime = new DateTime(2025, 12, 23, 19, 0, 0),
                    Location = "Masherova st., 10",
                    Category = "Books",
                    ImageUrl = "#F4A460",
                    Price = 0
                },
                new Event
                {
                    EventId = 3,
                    Title = "Exhibition «Art in lines»",
                    Description = "Contemporary art exhibition",
                    DateTime = new DateTime(2025, 12, 23, 19, 0, 0),
                    Location = "Masherova st., 10",
                    Category = "Art",
                    ImageUrl = "#DDA0DD",
                    Price = 500
                },
                new Event
                {
                    EventId = 4,
                    Title = "English speaking club",
                    Description = "Practice your English skills",
                    DateTime = new DateTime(2025, 12, 23, 19, 0, 0),
                    Location = "Masherova st., 10",
                    Category = "Languages",
                    ImageUrl = "#F0E68C",
                    Price = 0
                },
                new Event
                {
                    EventId = 5,
                    Title = "Stretching",
                    Description = "Full body stretching session",
                    DateTime = new DateTime(2025, 12, 23, 19, 0, 0),
                    Location = "Masherova st., 10",
                    Category = "Sport",
                    ImageUrl = "#D2B48C",
                    Price = 300
                },
                new Event
                {
                    EventId = 6,
                    Title = "Street music concert",
                    Description = "Live street music performance",
                    DateTime = new DateTime(2026, 2, 5, 18, 0, 0),
                    Location = "Masherova st., 10",
                    Category = "Music",
                    ImageUrl = "#B0C4DE",
                    Price = 0
                },
                new Event
                {
                    EventId = 7,
                    Title = "Music concert",
                    Description = "Classical music evening",
                    DateTime = new DateTime(2026, 2, 10, 19, 0, 0),
                    Location = "Masherova st., 10",
                    Category = "Music",
                    ImageUrl = "#BC8F8F",
                    Price = 800
                },
                new Event
                {
                    EventId = 8,
                    Title = "Orchestral music concert",
                    Description = "Symphony orchestra performance",
                    DateTime = new DateTime(2026, 2, 15, 19, 0, 0),
                    Location = "Masherova st., 10",
                    Category = "Music",
                    ImageUrl = "#8B7355",
                    Price = 1200
                },
                new Event
                {
                    EventId = 9,
                    Title = "Guitar masterclass",
                    Description = "Learn from professional guitarists",
                    DateTime = new DateTime(2026, 2, 20, 19, 0, 0),
                    Location = "Masherova st., 10",
                    Category = "Music",
                    ImageUrl = "#DEB887",
                    Price = 600
                },
                
                // Past events for review demonstration (for account artemmekena@gmail.com - userId will be 2)
                new Event
                {
                    EventId = 10,
                    Title = "IT Conference 2025",
                    Description = "Annual IT conference with latest technology trends",
                    DateTime = new DateTime(2025, 9, 15, 10, 0, 0),
                    Location = "Conference Hall, Minsk",
                    Category = "IT",
                    ImageUrl = "#6366F1",
                    Price = 0,
                    Participants = new List<EventParticipant>
                    {
                        new EventParticipant { UserId = 2, Name = "Artem", Email = "artemmekena@gmail.com", RegistrationDate = new DateTime(2025, 9, 1) }
                    }
                },
                new Event
                {
                    EventId = 11,
                    Title = "Photography Workshop",
                    Description = "Learn professional photography techniques",
                    DateTime = new DateTime(2025, 10, 5, 14, 0, 0),
                    Location = "Art Studio, Minsk",
                    Category = "Art",
                    ImageUrl = "#EC4899",
                    Price = 500,
                    Participants = new List<EventParticipant>
                    {
                        new EventParticipant { UserId = 2, Name = "Artem", Email = "artemmekena@gmail.com", RegistrationDate = new DateTime(2025, 9, 25) }
                    }
                },
                new Event
                {
                    EventId = 12,
                    Title = "Morning Yoga Session",
                    Description = "Start your day with energizing yoga",
                    DateTime = new DateTime(2025, 10, 20, 8, 0, 0),
                    Location = "Yoga Center, Minsk",
                    Category = "Sport",
                    ImageUrl = "#10B981",
                    Price = 0,
                    Participants = new List<EventParticipant>
                    {
                        new EventParticipant { UserId = 2, Name = "Artem", Email = "artemmekena@gmail.com", RegistrationDate = new DateTime(2025, 10, 15) }
                    }
                }
            };
                SaveEventsToFile();
            }
            
            // Check and add past events for review demonstration (if they don't exist yet)
            if (!_events.Any(e => e.Title == "IT Conference 2025" || e.Title == "Photography Workshop" || e.Title == "Morning Yoga Session"))
            {
                int maxId = _events.Count > 0 ? _events.Max(e => e.EventId) : 0;
                
                var pastEvents = new List<Event>
                {
                    new Event
                    {
                        EventId = maxId + 1,
                        Title = "IT Conference 2025",
                        Description = "Annual IT conference with latest technology trends",
                        DateTime = new DateTime(2025, 9, 15, 10, 0, 0),
                        Location = "Conference Hall, Minsk",
                        Category = "IT",
                        ImageUrl = "#6366F1",
                        Price = 0,
                        Participants = new List<EventParticipant>
                        {
                            new EventParticipant { UserId = 2, Name = "Artem", Email = "artemmekena@gmail.com", RegistrationDate = new DateTime(2025, 9, 1) }
                        }
                    },
                    new Event
                    {
                        EventId = maxId + 2,
                        Title = "Photography Workshop",
                        Description = "Learn professional photography techniques",
                        DateTime = new DateTime(2025, 10, 5, 14, 0, 0),
                        Location = "Art Studio, Minsk",
                        Category = "Art",
                        ImageUrl = "#EC4899",
                        Price = 500,
                        Participants = new List<EventParticipant>
                        {
                            new EventParticipant { UserId = 2, Name = "Artem", Email = "artemmekena@gmail.com", RegistrationDate = new DateTime(2025, 9, 25) }
                        }
                    },
                    new Event
                    {
                        EventId = maxId + 3,
                        Title = "Morning Yoga Session",
                        Description = "Start your day with energizing yoga",
                        DateTime = new DateTime(2025, 10, 20, 8, 0, 0),
                        Location = "Yoga Center, Minsk",
                        Category = "Sport",
                        ImageUrl = "#10B981",
                        Price = 0,
                        Participants = new List<EventParticipant>
                        {
                            new EventParticipant { UserId = 2, Name = "Artem", Email = "artemmekena@gmail.com", RegistrationDate = new DateTime(2025, 10, 15) }
                        }
                    }
                };
                
                _events.AddRange(pastEvents);
                SaveEventsToFile();
            }
            
            // Delete all reviews from user artemmekena@gmail.com
            DeleteUserReviewsByEmail("artemmekena@gmail.com");
        }

        private void LoadDataFromFile()
        {
            // Load users
            try
            {
                if (File.Exists(_usersFilePath))
                {
                    string json = File.ReadAllText(_usersFilePath);
                    var loadedUsers = JsonSerializer.Deserialize<List<User>>(json);
                    
                    if (loadedUsers != null && loadedUsers.Count > 0)
                    {
                        _users = loadedUsers;
                    }
                    else
                    {
                        _users = new List<User>();
                    }
                }
                else
                {
                    _users = new List<User>();
                }
            }
            catch (Exception)
            {
                _users = new List<User>();
            }
            
            // Load events
            try
            {
                if (File.Exists(_eventsFilePath))
                {
                    string json = File.ReadAllText(_eventsFilePath);
                    var loadedEvents = JsonSerializer.Deserialize<List<Event>>(json);
                    
                    if (loadedEvents != null && loadedEvents.Count > 0)
                    {
                        _events = loadedEvents;
                    }
                    else
                    {
                        _events = new List<Event>();
                    }
                }
                else
                {
                    _events = new List<Event>();
                }
            }
            catch (Exception)
            {
                _events = new List<Event>();
            }
            
            // Load notifications
            try
            {
                if (File.Exists(_notificationsFilePath))
                {
                    string json = File.ReadAllText(_notificationsFilePath);
                    var loadedNotifications = JsonSerializer.Deserialize<List<Notification>>(json);
                    
                    if (loadedNotifications != null && loadedNotifications.Count > 0)
                    {
                        _notifications = loadedNotifications;
                    }
                    else
                    {
                        _notifications = new List<Notification>();
                    }
                }
                else
                {
                    _notifications = new List<Notification>();
                }
            }
            catch (Exception)
            {
                _notifications = new List<Notification>();
            }
            
            // Load reviews
            try
            {
                if (File.Exists(_reviewsFilePath))
                {
                    string json = File.ReadAllText(_reviewsFilePath);
                    var loadedReviews = JsonSerializer.Deserialize<List<Review>>(json);
                    
                    if (loadedReviews != null && loadedReviews.Count > 0)
                    {
                        _reviews = loadedReviews;
                    }
                    else
                    {
                        _reviews = new List<Review>();
                    }
                }
                else
                {
                    _reviews = new List<Review>();
                }
            }
            catch (Exception)
            {
                _reviews = new List<Review>();
            }
        }

        private void SaveUsersToFile()
        {
            try
            {
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(_usersFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save data to file
                string json = JsonSerializer.Serialize(_users, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_usersFilePath, json);
            }
            catch (Exception)
            {
                // Ignore save errors
            }
        }
        
        private void SaveEventsToFile()
        {
            try
            {
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(_eventsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save data to file
                string json = JsonSerializer.Serialize(_events, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_eventsFilePath, json);
            }
            catch (Exception)
            {
                // Ignore save errors
            }
        }
        
        private void SaveNotificationsToFile()
        {
            try
            {
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(_notificationsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save data to file
                string json = JsonSerializer.Serialize(_notifications, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_notificationsFilePath, json);
            }
            catch (Exception)
            {
                // Ignore save errors
            }
        }

        public List<Event> GetAllEvents()
        {
            return _events.ToList();
        }

        public List<Event> GetEventsByCategory(string category)
        {
            if (category == "All")
                return _events.ToList();

            return _events.Where(e => e.Category.Contains(category)).ToList();
        }

        public Event GetEventById(int id)
        {
            return _events.FirstOrDefault(e => e.EventId == id);
        }
        
        public List<Event> GetEventsByOrganizer(int organizerId)
        {
            return _events.Where(e => e.OrganizerId == organizerId).ToList();
        }
        
        public List<Event> GetOrganizerFutureEvents(int organizerId)
        {
            return _events.Where(e => e.OrganizerId == organizerId && e.DateTime >= DateTime.Now).OrderBy(e => e.DateTime).ToList();
        }
        
        public List<Event> GetOrganizerPastEvents(int organizerId)
        {
            return _events.Where(e => e.OrganizerId == organizerId && e.DateTime < DateTime.Now).OrderByDescending(e => e.DateTime).ToList();
        }

        public void AddEvent(Event eventItem)
        {
            eventItem.EventId = _events.Count > 0 ? _events.Max(e => e.EventId) + 1 : 1;
            _events.Add(eventItem);
            SaveEventsToFile();
        }

        public void UpdateEvent(Event eventItem)
        {
            var existing = _events.FirstOrDefault(e => e.EventId == eventItem.EventId);
            if (existing != null)
            {
                var index = _events.IndexOf(existing);
                _events[index] = eventItem;
                SaveEventsToFile();
            }
        }

        public void DeleteEvent(int id)
        {
            var eventItem = _events.FirstOrDefault(e => e.EventId == id);
            if (eventItem != null)
            {
                _events.Remove(eventItem);
                SaveEventsToFile();
            }
        }

        public bool ValidateLogin(string loginOrEmail, string password)
        {
            // Can login by email or username
            var user = _users.FirstOrDefault(u => 
                u.Email.Equals(loginOrEmail, StringComparison.OrdinalIgnoreCase) || 
                u.Name.Equals(loginOrEmail, StringComparison.OrdinalIgnoreCase) ||
                (u.Name + u.Surname).Replace(" ", "").Equals(loginOrEmail.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
            
            if (user != null && user.Password == password)
            {
                CurrentUser = user;
                return true;
            }
            
            return false;
        }

        public void RegisterUser(User user)
        {
            user.UserId = _users.Count > 0 ? _users.Max(u => u.UserId) + 1 : 1;
            _users.Add(user);
            CurrentUser = user;
            SaveUsersToFile();
        }

        public List<User> GetAllUsers()
        {
            return _users.ToList();
        }

        public void DeleteUser(int userId)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                _users.Remove(user);
                SaveUsersToFile();
            }
        }
        
        public void UpdateCurrentUser()
        {
            // Update user in list and save
            var existingUser = _users.FirstOrDefault(u => u.UserId == CurrentUser?.UserId);
            if (existingUser != null && CurrentUser != null)
            {
                int index = _users.IndexOf(existingUser);
                _users[index] = CurrentUser;
                SaveUsersToFile();
                
                // Update participant info in all events where they are registered
                UpdateUserParticipantInfo(CurrentUser.UserId, CurrentUser.Name, CurrentUser.Surname, CurrentUser.Email);
            }
        }
        
        private void UpdateUserParticipantInfo(int userId, string name, string surname, string email)
        {
            bool eventsUpdated = false;
            bool reviewsUpdated = false;
            
            // Update participant info in all events
            foreach (var eventItem in _events)
            {
                if (eventItem.Participants != null && eventItem.Participants.Any(p => p.UserId == userId))
                {
                    var participant = eventItem.Participants.FirstOrDefault(p => p.UserId == userId);
                    if (participant != null)
                    {
                        participant.Name = $"{name} {surname}";
                        participant.Email = email;
                        eventsUpdated = true;
                    }
                }
            }
            
            // Update username in all their reviews
            foreach (var review in _reviews.Where(r => r.UserId == userId))
            {
                review.UserName = $"{name} {surname}";
                reviewsUpdated = true;
            }
            
            if (eventsUpdated)
            {
                SaveEventsToFile();
            }
            
            if (reviewsUpdated)
            {
                SaveReviewsToFile();
            }
        }
        
        // Methods for working with favorites
        public void ToggleFavoriteEvent(int eventId)
        {
            if (CurrentUser == null) return;
            
            if (CurrentUser.FavoriteEventIds.Contains(eventId))
            {
                CurrentUser.FavoriteEventIds.Remove(eventId);
            }
            else
            {
                CurrentUser.FavoriteEventIds.Add(eventId);
            }
            
            UpdateCurrentUser();
        }
        
        public bool IsEventFavorite(int eventId)
        {
            if (CurrentUser == null) return false;
            return CurrentUser.FavoriteEventIds.Contains(eventId);
        }
        
        public List<Event> GetFavoriteEvents()
        {
            if (CurrentUser == null) return new List<Event>();
            return _events.Where(e => CurrentUser.FavoriteEventIds.Contains(e.EventId)).ToList();
        }
        
        // Methods for working with notifications
        public void AddNotification(Notification notification)
        {
            notification.NotificationId = _notifications.Count > 0 ? _notifications.Max(n => n.NotificationId) + 1 : 1;
            _notifications.Add(notification);
            SaveNotificationsToFile();
        }
        
        public List<Notification> GetUserNotifications(int userId)
        {
            return _notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.Date).ToList();
        }
        
        public void UpdateNotification(Notification notification)
        {
            var existing = _notifications.FirstOrDefault(n => n.NotificationId == notification.NotificationId);
            if (existing != null)
            {
                var index = _notifications.IndexOf(existing);
                _notifications[index] = notification;
                SaveNotificationsToFile();
            }
        }
        
        public void DeleteNotification(int notificationId)
        {
            var notification = _notifications.FirstOrDefault(n => n.NotificationId == notificationId);
            if (notification != null)
            {
                _notifications.Remove(notification);
                SaveNotificationsToFile();
            }
        }
        
        public void ClearUserNotifications(int userId)
        {
            _notifications.RemoveAll(n => n.UserId == userId);
            SaveNotificationsToFile();
        }
        
        // Methods for working with reviews
        private void SaveReviewsToFile()
        {
            try
            {
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(_reviewsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Сохраняем данные в файл
                string json = JsonSerializer.Serialize(_reviews, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_reviewsFilePath, json);
            }
            catch (Exception)
            {
                // Игнорируем ошибки сохранения
            }
        }
        
        public void AddReview(Review review)
        {
            review.ReviewId = _reviews.Count > 0 ? _reviews.Max(r => r.ReviewId) + 1 : 1;
            review.CreatedDate = DateTime.Now;
            _reviews.Add(review);
            SaveReviewsToFile();
        }
        
        public List<Review> GetEventReviews(int eventId)
        {
            return _reviews.Where(r => r.EventId == eventId).OrderByDescending(r => r.CreatedDate).ToList();
        }
        
        public List<Review> GetOrganizerReviews(int organizerId)
        {
            // Получаем все события организатора
            var organizerEventIds = _events.Where(e => e.OrganizerId == organizerId).Select(e => e.EventId).ToList();
            
            // Получаем все отзывы на эти события
            return _reviews.Where(r => organizerEventIds.Contains(r.EventId)).OrderByDescending(r => r.CreatedDate).ToList();
        }
        
        public List<Review> GetUserReviews(int userId)
        {
            return _reviews.Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedDate).ToList();
        }
        
        public bool HasUserReviewedEvent(int userId, int eventId)
        {
            return _reviews.Any(r => r.UserId == userId && r.EventId == eventId);
        }
        
        public double GetEventAverageRating(int eventId)
        {
            var eventReviews = _reviews.Where(r => r.EventId == eventId).ToList();
            if (eventReviews.Count == 0) return 0;
            return eventReviews.Average(r => r.Rating);
        }
        
        public double GetOrganizerAverageRating(int organizerId)
        {
            var organizerReviews = GetOrganizerReviews(organizerId);
            if (organizerReviews.Count == 0) return 0;
            return organizerReviews.Average(r => r.Rating);
        }
        
        public int GetOrganizerReviewsCount(int organizerId)
        {
            return GetOrganizerReviews(organizerId).Count;
        }
        
        public void DeleteUserReviews(int userId)
        {
            _reviews.RemoveAll(r => r.UserId == userId);
            SaveReviewsToFile();
        }
        
        public void DeleteUserReviewsByEmail(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                DeleteUserReviews(user.UserId);
            }
        }
    }
}

