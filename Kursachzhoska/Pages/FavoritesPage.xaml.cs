using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kursachzhoska.Controls;
using Kursachzhoska.Models;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class FavoritesPage : Page
    {
        private string searchText = "";
        private List<Event> _favorites = new();
        private List<ApiApplication> _myApplications = new();

        public FavoritesPage()
        {
            InitializeComponent();
            Loaded += FavoritesPage_Loaded;
        }

        private async void FavoritesPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= FavoritesPage_Loaded;
            await ReloadAppsAsync();
            await LoadFavoritesAsync();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            searchText = SearchBox.Text.ToLower();
            _ = ApplyFiltersAsync();
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

        private async Task ReloadAppsAsync()
        {
            try
            {
                _myApplications = await ApiService.Instance.GetMyApplicationsAsync();
            }
            catch
            {
                _myApplications = new List<ApiApplication>();
            }
        }

        private async Task LoadFavoritesAsync()
        {
            try
            {
                _favorites = await ApiService.Instance.GetFavoriteEventsAsync();
            }
            catch
            {
                _favorites = new List<Event>();
            }

            await ApplyFiltersAsync();
        }

        private async Task ApplyFiltersAsync()
        {
            var favoriteEvents = _favorites.Where(e => e.DateTime >= DateTime.Now).ToList();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                favoriteEvents = favoriteEvents.Where(e => 
                    e.Title.ToLower().Contains(searchText) || 
                    e.Description.ToLower().Contains(searchText) ||
                    e.Location.ToLower().Contains(searchText)
                ).ToList();
            }
            
            if (favoriteEvents == null || favoriteEvents.Count == 0)
            {
                EmptyStatePanel.Visibility = Visibility.Visible;
                EventsPanel.Visibility = Visibility.Collapsed;
                return;
            }

            EmptyStatePanel.Visibility = Visibility.Collapsed;
            EventsPanel.Visibility = Visibility.Visible;

            EventsPanel.Children.Clear();
            foreach (var eventData in favoriteEvents)
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
                Margin = new Thickness(0, 0, 20, 20),
                Cursor = Cursors.Hand,
                Tag = eventData.EventId
            };
            card.MouseLeftButtonDown += EventCard_Click;

            var stackPanel = new StackPanel();

            // Изображение
            var favoriteButton = new Button
            {
                Content = "♥",
                Style = Application.Current.Resources["IconButton"] as Style,
                Width = 32,
                Height = 32,
                FontSize = 16,
                Background = Application.Current.Resources["SurfaceColor"] as Brush,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 8, 8, 0),
                Tag = eventData.EventId
            };
            
            favoriteButton.Click += FavoriteButton_Click;
            
            // Изображение или цветной фон
            Border imageContainer;
            if (!string.IsNullOrEmpty(eventData.EventImagePath) && File.Exists(eventData.EventImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(eventData.EventImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    
                    var image = new Image
                    {
                        Source = bitmap,
                        Stretch = Stretch.UniformToFill
                    };
                    
                    imageContainer = new Border
                    {
                        Height = 140,
                        CornerRadius = new CornerRadius(8),
                        Margin = new Thickness(0, 0, 0, 12),
                        Child = new Grid
                        {
                            Children = { image, favoriteButton }
                        }
                    };
                }
                catch
                {
                    // Если не удалось загрузить изображение, используем цветной фон
                    imageContainer = new Border
                    {
                        Height = 140,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(eventData.ImageUrl)),
                        CornerRadius = new CornerRadius(8),
                        Margin = new Thickness(0, 0, 0, 12),
                        Child = new Grid
                        {
                            Children = { favoriteButton }
                        }
                    };
                }
            }
            else
            {
                // Используем цветной фон по умолчанию
                imageContainer = new Border
                {
                    Height = 140,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(eventData.ImageUrl)),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 0, 0, 12),
                    Child = new Grid
                    {
                        Children = { favoriteButton }
                    }
                };
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

            // Кнопка регистрации/отмены
            var registerButton = new Button
            {
                Style = Application.Current.Resources["DarkButton"] as Style,
                Padding = new Thickness(16, 10, 16, 10),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Tag = eventData.EventId
            };

            var myApp = _myApplications.FirstOrDefault(a => a.Event == eventData.EventId);
            if (myApp == null || myApp.Status == "cancelled" || myApp.Status == "rejected")
            {
                registerButton.Content = "Зарегистрироваться";
            }
            else if (myApp.Status == "pending" || myApp.Status == "approved")
            {
                registerButton.Content = "Отменить заявку";
            }
            else
            {
                registerButton.Content = "Заявка";
            }

            registerButton.Click += RegisterButton_Click;
            stackPanel.Children.Add(registerButton);

            card.Child = stackPanel;
            return card;
        }
        
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag is int eventId)
            {
                var currentUser = ApiService.Instance.CurrentUser;
                if (currentUser == null)
                {
                    CustomMessageBox.Show("Войдите в аккаунт для регистрации на мероприятия", "Требуется вход", CustomMessageBox.MessageBoxButton.OK);
                    return;
                }
                
                var existing = _myApplications.FirstOrDefault(a => a.Event == eventId);

                try
                {
                    if (existing == null || existing.Status == "cancelled" || existing.Status == "rejected")
                    {
                        await ApiService.Instance.ApplyToEventAsync(eventId);
                        CustomMessageBox.Show("Заявка отправлена", "Успех", CustomMessageBox.MessageBoxButton.OK);
                    }
                    else if (existing.Status == "pending" || existing.Status == "approved")
                    {
                        await ApiService.Instance.ApplicationActionAsync(existing.Id, "cancel");
                        CustomMessageBox.Show("Заявка отменена", "Информация", CustomMessageBox.MessageBoxButton.OK);
                    }

                    await ReloadAppsAsync();
                    await LoadFavoritesAsync();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
        }
        
        private void EventCard_Click(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, что клик не был на кнопке
            if (e.OriginalSource is Button || e.OriginalSource is Border parentBorder && parentBorder.Parent is Button)
            {
                e.Handled = true;
                return;
            }

            var card = sender as Border;
            if (card != null && card.Tag is int eventId)
            {
                NavigationService?.Navigate(new EventDetailsPage(eventId));
            }
        }
        
        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag is int eventId)
            {
                await Task.Run(() => ApiService.Instance.ToggleFavoriteEvent(eventId));
                await LoadFavoritesAsync();
            }
        }

        private void GoToAffiche_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CatalogPage());
        }
    }
}
