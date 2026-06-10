using System;
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
    public partial class MyEventsPage : Page
    {
        private bool showFuture = true;
        private string searchText = "";
        private System.Collections.Generic.List<Event> _events = new();

        public MyEventsPage()
        {
            InitializeComponent();
            Loaded += MyEventsPage_Loaded;
        }

        private async void MyEventsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MyEventsPage_Loaded;
            await LoadEventsAsync();
            LoadFutureEvents();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            searchText = SearchBox.Text.ToLower();
            
            if (showFuture)
                LoadFutureEvents();
            else
                LoadPastEvents();
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

        private async Task LoadEventsAsync()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser == null)
            {
                _events = new System.Collections.Generic.List<Event>();
                return;
            }

            try
            {
                _events = await ApiService.Instance.GetEventsByOrganizerAsync(currentUser.UserId);
            }
            catch
            {
                _events = new System.Collections.Generic.List<Event>();
            }
        }

        private async void LoadFutureEvents()
        {
            showFuture = true;
            UpdateButtonStyles();
            if (_events.Count == 0)
            {
                await LoadEventsAsync();
            }

            var events = _events.Where(e => e.DateTime >= DateTime.Now).OrderBy(e => e.DateTime).ToList();
            
            // Применяем поиск
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                events = events.Where(e => 
                    e.Title.ToLower().Contains(searchText) || 
                    e.Description.ToLower().Contains(searchText) ||
                    e.Location.ToLower().Contains(searchText)
                ).ToList();
            }
            
            DisplayEvents(events);
        }

        private async void LoadPastEvents()
        {
            showFuture = false;
            UpdateButtonStyles();
            if (_events.Count == 0)
            {
                await LoadEventsAsync();
            }

            var events = _events.Where(e => e.DateTime < DateTime.Now).OrderByDescending(e => e.DateTime).ToList();
            
            // Применяем поиск
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                events = events.Where(e => 
                    e.Title.ToLower().Contains(searchText) || 
                    e.Description.ToLower().Contains(searchText) ||
                    e.Location.ToLower().Contains(searchText)
                ).ToList();
            }
            
            DisplayEvents(events);
        }

        private void UpdateButtonStyles()
        {
            if (showFuture)
            {
                FutureButton.Style = Application.Current.Resources["DarkButton"] as Style;
                PastButton.Style = Application.Current.Resources["LightButton"] as Style;
            }
            else
            {
                FutureButton.Style = Application.Current.Resources["LightButton"] as Style;
                PastButton.Style = Application.Current.Resources["DarkButton"] as Style;
            }
        }

        private void DisplayEvents(System.Collections.Generic.List<Event> events)
        {
            EventsPanel.Children.Clear();

            foreach (var eventData in events)
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
                Width = 280,
                Height = 340,
                Margin = new Thickness(0, 0, 20, 20)
            };

            var stackPanel = new StackPanel();

            // Изображение с иконками (удалить / заявки)
            var actionsGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 8, 8, 0)
            };
            actionsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            actionsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var deleteButton = new Button
            {
                Content = "🗑",
                Style = Application.Current.Resources["IconButton"] as Style,
                Width = 32,
                Height = 32,
                FontSize = 16,
                Background = Application.Current.Resources["SurfaceColor"] as Brush,
                Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush,
                Tag = eventData.EventId
            };
            deleteButton.Click += DeleteButton_Click;
            Grid.SetColumn(deleteButton, 0);
            actionsGrid.Children.Add(deleteButton);

            var appsButton = new Button
            {
                Content = "👥",
                Style = Application.Current.Resources["IconButton"] as Style,
                Width = 32,
                Height = 32,
                FontSize = 16,
                Background = Application.Current.Resources["SurfaceColor"] as Brush,
                Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush,
                Tag = eventData.EventId
            };
            appsButton.Click += ParticipantsButton_Click;
            Grid.SetColumn(appsButton, 1);
            actionsGrid.Children.Add(appsButton);
            
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
                        Height = 160,
                        CornerRadius = new CornerRadius(8),
                        Margin = new Thickness(0, 0, 0, 12),
                        Child = new Grid
                        {
                            Children = { image, actionsGrid }
                        }
                    };
                }
                catch
                {
                    // Если не удалось загрузить изображение, используем цветной фон
                    imageContainer = new Border
                    {
                        Height = 160,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(eventData.ImageUrl)),
                        CornerRadius = new CornerRadius(8),
                        Margin = new Thickness(0, 0, 0, 12),
                        Child = actionsGrid
                    };
                }
            }
            else
            {
                // Используем цветной фон по умолчанию
                imageContainer = new Border
                {
                    Height = 160,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(eventData.ImageUrl)),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 0, 0, 12),
                    Child = actionsGrid
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
                Text = "Дата: " + eventData.DateTime.ToString("dd.MM.yyyy HH:mm"),
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 12,
                Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                Margin = new Thickness(0, 0, 0, 4)
            };
            stackPanel.Children.Add(dateText);

            // Место
            var locationText = new TextBlock
            {
                Text = "Место: " + eventData.Location,
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 12,
                Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                Margin = new Thickness(0, 0, 0, 12)
            };
            stackPanel.Children.Add(locationText);

            // Кнопки действий
            var buttonsPanel = new Grid { Margin = new Thickness(0, 0, 0, 0) };
            buttonsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttonsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            buttonsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            var editButton = new Button
            {
                Content = "Редактировать",
                Style = Application.Current.Resources["DarkButton"] as Style,
                Padding = new Thickness(16, 8, 16, 8),
                Tag = eventData.EventId
            };
            editButton.Click += EditButton_Click;
            Grid.SetColumn(editButton, 0);
            buttonsPanel.Children.Add(editButton);

            var participantsButton = new Button
            {
                Content = "👥",
                Style = Application.Current.Resources["IconButton"] as Style,
                Width = 40,
                Height = 40,
                FontSize = 16,
                Background = Application.Current.Resources["SurfaceColor"] as Brush,
                Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush,
                Tag = eventData.EventId
            };
            participantsButton.Click += ParticipantsButton_Click;
            Grid.SetColumn(participantsButton, 2);
            buttonsPanel.Children.Add(participantsButton);
            
            stackPanel.Children.Add(buttonsPanel);

            card.Child = stackPanel;
            return card;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int eventId)
            {
                var result = CustomMessageBox.Show(
                    "Вы уверены, что хотите удалить это мероприятие?",
                    "Удаление мероприятия",
                    CustomMessageBox.MessageBoxButton.YesNo);

                if (result == CustomMessageBox.MessageBoxResult.Yes)
                {
                    await ApiService.Instance.DeleteEventAsync(eventId);
                    await LoadEventsAsync();
                    if (showFuture)
                        LoadFutureEvents();
                    else
                        LoadPastEvents();
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int eventId)
            {
                NavigationService?.Navigate(new EditEventPage(eventId));
            }
        }

        private void ParticipantsButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int eventId)
            {
                NavigationService?.Navigate(new RegisteredParticipantsPage(eventId));
            }
        }

        private void FutureButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFutureEvents();
        }

        private void PastButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPastEvents();
        }
    }
}
