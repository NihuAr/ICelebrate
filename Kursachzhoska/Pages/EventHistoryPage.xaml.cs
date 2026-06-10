using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kursachzhoska.Controls;
using Kursachzhoska.Models;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class EventHistoryPage : Page
    {
        private List<Event> historyEvents = new();
        private string searchText = "";

        public EventHistoryPage()
        {
            InitializeComponent();
            Loaded += EventHistoryPage_Loaded;
        }

        private async void EventHistoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= EventHistoryPage_Loaded;
            await LoadHistoryAsync();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            searchText = SearchBox.Text.ToLower();
            _ = LoadHistoryAsync();
        }
        
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchPlaceholder.Visibility = Visibility.Collapsed;
        }
        
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                SearchPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private async Task LoadHistoryAsync()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser == null)
            {
                EmptyStatePanel.Visibility = Visibility.Visible;
                EventsPanel.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var myApps = await ApiService.Instance.GetUserApplicationsAsync();
                var pastEventIds = myApps
                    .Where(a => a.EventId != 0)
                    .Select(a => a.EventId)
                    .Distinct()
                    .ToList();

                var events = new List<Event>();
                foreach (var id in pastEventIds)
                {
                    var ev = await ApiService.Instance.GetEventByIdAsync(id);
                    if (ev != null && ev.DateTime < DateTime.Now)
                    {
                        events.Add(ev);
                    }
                }

                historyEvents = events
                    .OrderByDescending(e => e.DateTime)
                    .ToList();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                historyEvents = new List<Event>();
            }
            
            // Применяем поиск
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                historyEvents = historyEvents.Where(e => 
                    e.Title.ToLower().Contains(searchText) || 
                    e.Description.ToLower().Contains(searchText) ||
                    e.Location.ToLower().Contains(searchText)
                ).ToList();
            }

            if (historyEvents.Count == 0)
            {
                EmptyStatePanel.Visibility = Visibility.Visible;
                EventsPanel.Visibility = Visibility.Collapsed;
                return;
            }

            EmptyStatePanel.Visibility = Visibility.Collapsed;
            EventsPanel.Visibility = Visibility.Visible;

            EventsPanel.Children.Clear();
            foreach (var eventData in historyEvents)
            {
                var card = CreateEventCard(eventData);
                EventsPanel.Children.Add(card);
            }
        }

        private Border CreateEventCard(Event eventData)
        {
            var card = new Border
            {
                Style = Application.Current.Resources["Card"] as Style,
                Width = 240,
                Height = 340,
                Margin = new Thickness(0, 0, 20, 20)
            };

            var stackPanel = new StackPanel();

            // Изображение
            var imageContainer = new Border
            {
                Height = 140,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 12)
            };

            // Проверяем наличие изображения
            if (!string.IsNullOrEmpty(eventData.EventImagePath) && File.Exists(eventData.EventImagePath))
            {
                var imageBrush = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(eventData.EventImagePath, UriKind.Absolute)),
                    Stretch = Stretch.UniformToFill
                };
                imageContainer.Background = imageBrush;
            }
            else if (!string.IsNullOrEmpty(eventData.ImageUrl))
            {
                imageContainer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(eventData.ImageUrl));
            }
            else
            {
                imageContainer.Background = new SolidColorBrush(Colors.Gray);
            }
            
            stackPanel.Children.Add(imageContainer);

            // Категория
            var categoryText = new TextBlock
            {
                Text = eventData.Category,
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 11,
                Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                Margin = new Thickness(0, 0, 0, 4)
            };
            stackPanel.Children.Add(categoryText);

            // Название
            var titleText = new TextBlock
            {
                Text = eventData.Title,
                FontFamily = Application.Current.Resources["HeadingFont"] as FontFamily,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush,
                Margin = new Thickness(0, 0, 0, 8),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            stackPanel.Children.Add(titleText);

            // Дата
            var dateText = new TextBlock
            {
                Text = "📅 " + eventData.DateTime.ToString("dd.MM.yyyy HH:mm"),
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 12,
                Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                Margin = new Thickness(0, 0, 0, 4)
            };
            stackPanel.Children.Add(dateText);

            // Место
            var locationText = new TextBlock
            {
                Text = "📍 " + eventData.Location,
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 12,
                Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                Margin = new Thickness(0, 0, 0, 12)
            };
            stackPanel.Children.Add(locationText);

            // Проверяем, оставлял ли пользователь отзыв
            var currentUser = ApiService.Instance.CurrentUser;
            bool hasReviewed = currentUser != null && 
                              ApiService.Instance.HasUserReviewedEvent(currentUser.UserId, eventData.EventId);

            // Кнопка оставить отзыв или текст "Reviewed"
            if (hasReviewed)
            {
                var reviewedText = new TextBlock
                {
                    Text = "✓ Отзыв оставлен",
                    FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                    FontSize = 12,
                    Foreground = Application.Current.Resources["SuccessColor"] as Brush,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 8, 0, 0)
                };
                stackPanel.Children.Add(reviewedText);
            }
            else
            {
                var feedbackButton = new Button
                {
                    Content = "Оставить отзыв",
                    Style = Application.Current.Resources["SmallDarkButton"] as Style,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Tag = eventData.EventId
                };
                feedbackButton.Click += LeaveReviewButton_Click;
                stackPanel.Children.Add(feedbackButton);
            }

            card.Child = stackPanel;
            return card;
        }

        private void LeaveReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int eventId)
            {
                NavigationService?.Navigate(new CreateReviewPage(eventId));
            }
        }

        private void GoToAffiche_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CatalogPage());
        }
    }
}
