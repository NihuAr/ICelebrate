using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kursachzhoska.Controls;
using Kursachzhoska.Helpers;
using Kursachzhoska.Models;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class EventDetailsPage : Page
    {
        private readonly int _eventId;
        private Event eventData;
        private ApiApplication _myApplication;

        public EventDetailsPage(int eventId)
        {
            InitializeComponent();
            _eventId = eventId;
            Loaded += EventDetailsPage_Loaded;
        }

        private async void EventDetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= EventDetailsPage_Loaded;

            eventData = await ApiService.Instance.GetEventByIdAsync(_eventId);
            if (eventData == null)
            {
                CustomMessageBox.Show("Мероприятие не найдено", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                NavigationService?.GoBack();
                return;
            }

            await LoadMyApplicationAsync();
            LoadEventData();
        }

        private void LoadEventData()
        {
            // Load basic info
            CategoryText.Text = eventData.Category;
            TitleText.Text = eventData.Title;
            DateTimeText.Text = eventData.DateTime.ToString("dd.MM.yyyy HH:mm");
            LocationText.Text = eventData.Location;
            PriceText.Text = eventData.Price == 0 ? "Бесплатно" : $"{eventData.Price}₽";
            DescriptionText.Text = eventData.Description;

            // Load organizer info
            LoadOrganizerInfo();

            // Load image
            if (!string.IsNullOrEmpty(eventData.EventImagePath) && File.Exists(eventData.EventImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(eventData.EventImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    EventImage.Source = bitmap;
                    EventImage.Visibility = Visibility.Visible;
                }
                catch
                {
                    EventImage.Visibility = Visibility.Collapsed;
                    EventImageContainer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(eventData.ImageUrl));
                }
            }
            else
            {
                EventImage.Visibility = Visibility.Collapsed;
                EventImageContainer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(eventData.ImageUrl));
            }

            UpdateFavoriteButton();
            UpdateRegisterButton();

            // Load contractors (if backend later provides)
            ContractorsTitle.Visibility = Visibility.Collapsed;
            ContractorsPanel.Children.Clear();

            // Load documents (if backend later provides)
            DocumentsTitle.Visibility = Visibility.Collapsed;
            DocumentsPanel.Children.Clear();

            // Load reviews
            LoadReviews();
        }

        private void LoadReviews()
        {
            var reviews = ApiService.Instance.GetEventReviews(eventData.EventId);
            
            if (reviews != null && reviews.Count > 0)
            {
                ReviewsTitle.Visibility = Visibility.Visible;
                ReviewsPanel.Children.Clear();

                foreach (var review in reviews)
                {
                    var border = new Border
                    {
                        Style = Application.Current.FindResource("Card") as Style,
                        Margin = new Thickness(0, 0, 0, 12),
                        Padding = new Thickness(16),
                        Background = Application.Current.FindResource("CardBackground") as Brush
                    };

                    var mainPanel = new StackPanel();

                    // Header: Name and Stars
                    var headerPanel = new StackPanel
                    {
                        Margin = new Thickness(0, 0, 0, 8)
                    };

                    var nameText = new TextBlock
                    {
                        Text = review.UserName,
                        FontFamily = Application.Current.FindResource("HeadingFont") as FontFamily,
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = Application.Current.FindResource("TextPrimaryColor") as Brush,
                        Margin = new Thickness(0, 0, 0, 4)
                    };
                    headerPanel.Children.Add(nameText);

                    // Stars
                    int fullStars = review.Rating;
                    int emptyStars = 5 - review.Rating;
                    var starsText = new TextBlock
                    {
                        Text = new string('★', fullStars) + new string('☆', emptyStars),
                        FontSize = 14,
                        Foreground = Application.Current.FindResource("AccentColor") as Brush,
                        Margin = new Thickness(0, 0, 0, 4)
                    };
                    headerPanel.Children.Add(starsText);

                    // Date
                    var dateText = new TextBlock
                    {
                        Text = review.CreatedDate.ToString("dd.MM.yyyy"),
                        FontFamily = Application.Current.FindResource("PrimaryFont") as FontFamily,
                        FontSize = 12,
                        Foreground = Application.Current.FindResource("TextSecondaryColor") as Brush
                    };
                    headerPanel.Children.Add(dateText);

                    mainPanel.Children.Add(headerPanel);

                    // Comment
                    if (!string.IsNullOrEmpty(review.Comment))
                    {
                        var commentText = new TextBlock
                        {
                            Text = review.Comment,
                            FontFamily = Application.Current.FindResource("PrimaryFont") as FontFamily,
                            FontSize = 14,
                            Foreground = Application.Current.FindResource("TextPrimaryColor") as Brush,
                            TextWrapping = TextWrapping.Wrap,
                            LineHeight = 20
                        };
                        mainPanel.Children.Add(commentText);
                    }

                    border.Child = mainPanel;
                    ReviewsPanel.Children.Add(border);
                }
            }
        }

        private void DocumentBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string filePath)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show($"Ошибка открытия документа: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                    }
                }
            }
        }

        private void UpdateFavoriteButton()
        {
            bool isFavorite = ApiService.Instance.IsEventFavorite(eventData.EventId);
            var iconKey = isFavorite ? "FavoriteIcon" : "FavoriteBorderIcon";
            FavoriteIcon.Data = (Geometry)FindResource(iconKey);
            FavoriteIcon.Fill = new SolidColorBrush(Color.FromRgb(239, 68, 68));
        }

        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => ApiService.Instance.ToggleFavoriteEvent(eventData.EventId));
            UpdateFavoriteButton();
        }

        private async Task LoadMyApplicationAsync()
        {
            try
            {
                var apps = await ApiService.Instance.GetMyApplicationsAsync();
                _myApplication = apps.FirstOrDefault(a => a.Event == eventData.EventId);
            }
            catch
            {
                _myApplication = null;
            }
        }

        private void UpdateRegisterButton()
        {
            if (_myApplication == null || _myApplication.Status == "cancelled" || _myApplication.Status == "rejected")
            {
                RegisterButton.IsEnabled = true;
                RegisterButton.Content = "Зарегистрироваться";
            }
            else if (_myApplication.Status == "pending" || _myApplication.Status == "approved")
            {
                RegisterButton.IsEnabled = true;
                RegisterButton.Content = "Отменить заявку";
            }
            else
            {
                RegisterButton.IsEnabled = false;
                RegisterButton.Content = "Заявка отправлена";
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser == null)
            {
                CustomMessageBox.Show("Войдите в аккаунт для регистрации на мероприятия", "Требуется вход", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            try
            {
                if (_myApplication == null || _myApplication.Status == "cancelled" || _myApplication.Status == "rejected")
                {
                    await ApiService.Instance.ApplyToEventAsync(eventData.EventId);
                    CustomMessageBox.Show("Заявка отправлена", "Успех", CustomMessageBox.MessageBoxButton.OK);
                }
                else if (_myApplication.Status == "pending" || _myApplication.Status == "approved")
                {
                    await ApiService.Instance.ApplicationActionAsync(_myApplication.Id, "cancel");
                    CustomMessageBox.Show("Заявка отменена", "Информация", CustomMessageBox.MessageBoxButton.OK);
                }

                await LoadMyApplicationAsync();
                UpdateRegisterButton();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void LoadOrganizerInfo()
        {
            var organizerDisplay = !string.IsNullOrWhiteSpace(eventData.OrganizerCompanyName)
                ? eventData.OrganizerCompanyName
                : (string.IsNullOrWhiteSpace(eventData.OrganizerName) ? "iCelebrate" : eventData.OrganizerName);

            OrganizerNameText.Text = organizerDisplay;

            var avgRating = eventData.Rating;
            var fullStars = (int)Math.Round(avgRating);
            if (fullStars > 5) fullStars = 5;
            if (fullStars < 0) fullStars = 0;
            OrganizerRatingStars.Text = new string('★', fullStars) + new string('☆', 5 - fullStars);
            OrganizerRatingText.Text = avgRating > 0 ? $"{avgRating:F1}" : "Отзывов пока нет";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}

