using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class MainPage : Page
    {
        private bool isOrganizerMode = false;
        private bool isMenuExpanded = true;
        private Button _activeButton;

        public MainPage(bool organizerMode = false)
        {
            InitializeComponent();
            
            // Проверяем, является ли текущий пользователь организатором
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null && currentUser.IsOrganizer)
            {
                isOrganizerMode = true;
            }
            else
            {
                isOrganizerMode = organizerMode;
            }
            
            LoadUserData();
            UpdateMenuForMode();
            _ = UpdateNotificationIconAsync();
            
            // По умолчанию открываем страницу каталога
            ContentFrame.Navigate(new CatalogPage());
            SetActiveButton(AfficheButton);
            
            // Подписываемся на событие навигации для обновления данных
            ContentFrame.Navigated += ContentFrame_Navigated;
            
            // Подписываемся на событие Loaded для обновления при возврате на страницу
            this.Loaded += MainPage_Loaded;
        }
        
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Обновляем данные пользователя при каждой загрузке страницы
            LoadUserData();
            _ = UpdateNotificationIconAsync();
        }
        
        private void ContentFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Обновляем данные пользователя после навигации
            LoadUserData();
            _ = UpdateNotificationIconAsync();
        }
        
        public void OnNavigatedTo(object parameter)
        {
            // Публичный метод для принудительного обновления данных
            LoadUserData();
            _ = UpdateNotificationIconAsync();
        }

        private void LoadUserData()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null)
            {
                UserAvatarText.Text = !string.IsNullOrEmpty(currentUser.Name) ? currentUser.Name.Substring(0, 1).ToUpper() : "?";
                UserNameText.Text = $"{currentUser.Name} {currentUser.Surname}";
                UserEmailText.Text = currentUser.Email;
                
                // Загружаем фото профиля
                if (!string.IsNullOrEmpty(currentUser.ProfileImagePath) && File.Exists(currentUser.ProfileImagePath))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new System.Uri(currentUser.ProfileImagePath, System.UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        
                        UserAvatarImage.Source = bitmap;
                        UserAvatarImage.Visibility = Visibility.Visible;
                        UserAvatarText.Visibility = Visibility.Collapsed;
                    }
                    catch
                    {
                        UserAvatarImage.Visibility = Visibility.Collapsed;
                        UserAvatarText.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    UserAvatarImage.Visibility = Visibility.Collapsed;
                    UserAvatarText.Visibility = Visibility.Visible;
                }
            }
        }

        private void UpdateMenuForMode()
        {
            if (isOrganizerMode)
            {
                OrganizerSeparator.Visibility = Visibility.Visible;
                AddEventButton.Visibility = Visibility.Visible;
                MyEventsButton.Visibility = Visibility.Visible;
                AddEventButton.IsEnabled = true;
                MyEventsButton.IsEnabled = true;
            }
            else
            {
                OrganizerSeparator.Visibility = Visibility.Collapsed;
                AddEventButton.Visibility = Visibility.Collapsed;
                MyEventsButton.Visibility = Visibility.Collapsed;
                AddEventButton.IsEnabled = false;
                MyEventsButton.IsEnabled = false;
            }
            
            // ReviewsButton всегда видна для всех пользователей
            ReviewsButton.Visibility = Visibility.Visible;
        }
        
        private async Task UpdateNotificationIconAsync()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null)
            {
                try
                {
                    var notifications = await ApiService.Instance.GetNotificationsAsync();
                    bool hasUnread = notifications.Any(n => n.RequiresConfirmation && n.IsConfirmed == null);
                    
                    NotificationIcon.Fill = hasUnread ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(99, 102, 241)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                }
                catch
                {
                    NotificationIcon.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                }
            }
        }

        private void SetActiveButton(Button button)
        {
            var buttons = new[]
            {
                AfficheButton, FavoritesButton, HistoryButton, ApplicationsButton, ReviewsButton,
                AddEventButton, MyEventsButton, PromosButton, BonusButton, NotificationsButton
            };

            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                btn.Opacity = 0.65;
            }

            if (button != null)
            {
                button.Opacity = 1.0;
                _activeButton = button;
            }
        }
        
        private void ToggleMenuButton_Click(object sender, RoutedEventArgs e)
        {
            isMenuExpanded = !isMenuExpanded;
            
            if (isMenuExpanded)
            {
                // Развернутое меню
                SidebarColumn.Width = new GridLength(260);
                LogoText.Visibility = Visibility.Visible;
                MenuPanel.Margin = new Thickness(16, 0, 16, 0);
                
                // Показываем текст в кнопках
                AfficheText.Visibility = Visibility.Visible;
                FavoritesText.Visibility = Visibility.Visible;
                HistoryText.Visibility = Visibility.Visible;
                ReviewsText.Visibility = Visibility.Visible;
                AddEventText.Visibility = Visibility.Visible;
                MyEventsText.Visibility = Visibility.Visible;
                NotificationsText.Visibility = Visibility.Visible;
                
                // Выравнивание по левому краю и padding
                AfficheButton.HorizontalContentAlignment = HorizontalAlignment.Left;
                FavoritesButton.HorizontalContentAlignment = HorizontalAlignment.Left;
                HistoryButton.HorizontalContentAlignment = HorizontalAlignment.Left;
                ReviewsButton.HorizontalContentAlignment = HorizontalAlignment.Left;
                AddEventButton.HorizontalContentAlignment = HorizontalAlignment.Left;
                MyEventsButton.HorizontalContentAlignment = HorizontalAlignment.Left;
                NotificationsButton.HorizontalContentAlignment = HorizontalAlignment.Left;
                
                AfficheButton.Padding = new Thickness(14);
                FavoritesButton.Padding = new Thickness(14);
                HistoryButton.Padding = new Thickness(14);
                ReviewsButton.Padding = new Thickness(14);
                AddEventButton.Padding = new Thickness(14);
                MyEventsButton.Padding = new Thickness(14);
                NotificationsButton.Padding = new Thickness(14);
                
                // Показываем информацию профиля
                UserInfoPanel.Visibility = Visibility.Visible;
                ProfileArrow.Visibility = Visibility.Visible;
                ProfileBorder.Margin = new Thickness(16, 0, 16, 16);
                ProfileBorder.Padding = new Thickness(12);
                UserAvatar.Margin = new Thickness(0, 0, 12, 0);
                ProfileGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
            else
            {
                // Свернутое меню
                SidebarColumn.Width = new GridLength(74);
                LogoText.Visibility = Visibility.Collapsed;
                MenuPanel.Margin = new Thickness(0, 0, 0, 0);
                
                // Скрываем текст в кнопках
                AfficheText.Visibility = Visibility.Collapsed;
                FavoritesText.Visibility = Visibility.Collapsed;
                HistoryText.Visibility = Visibility.Collapsed;
                AddEventText.Visibility = Visibility.Collapsed;
                MyEventsText.Visibility = Visibility.Collapsed;
                ReviewsText.Visibility = Visibility.Collapsed;
                NotificationsText.Visibility = Visibility.Collapsed;
                
                // Выравнивание по центру и минимальный padding
                AfficheButton.HorizontalContentAlignment = HorizontalAlignment.Center;
                FavoritesButton.HorizontalContentAlignment = HorizontalAlignment.Center;
                HistoryButton.HorizontalContentAlignment = HorizontalAlignment.Center;
                ReviewsButton.HorizontalContentAlignment = HorizontalAlignment.Center;
                AddEventButton.HorizontalContentAlignment = HorizontalAlignment.Center;
                MyEventsButton.HorizontalContentAlignment = HorizontalAlignment.Center;
                NotificationsButton.HorizontalContentAlignment = HorizontalAlignment.Center;
                
                AfficheButton.Padding = new Thickness(0, 12, 0, 12);
                FavoritesButton.Padding = new Thickness(0, 12, 0, 12);
                HistoryButton.Padding = new Thickness(0, 12, 0, 12);
                AddEventButton.Padding = new Thickness(0, 12, 0, 12);
                MyEventsButton.Padding = new Thickness(0, 12, 0, 12);
                ReviewsButton.Padding = new Thickness(0, 12, 0, 12);
                NotificationsButton.Padding = new Thickness(0, 12, 0, 12);
                
                // Скрываем информацию профиля
                UserInfoPanel.Visibility = Visibility.Collapsed;
                ProfileArrow.Visibility = Visibility.Collapsed;
                ProfileBorder.Margin = new Thickness(13, 0, 13, 16);
                ProfileBorder.Padding = new Thickness(13, 12, 13, 12);
                UserAvatar.Margin = new Thickness(0);
                ProfileGrid.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        private void AfficheButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new CatalogPage());
            SetActiveButton(AfficheButton);
        }

        private void FavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new FavoritesPage());
            SetActiveButton(FavoritesButton);
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new EventHistoryPage());
            SetActiveButton(HistoryButton);
        }

        private void AddEventButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new CreateEventPage());
            SetActiveButton(AddEventButton);
        }

        private void MyEventsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new MyEventsPage());
            SetActiveButton(MyEventsButton);
        }

        private void ReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new MyReviewsPage());
            SetActiveButton(ReviewsButton);
        }

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new NotificationsPage());
            SetActiveButton(NotificationsButton);
            _ = UpdateNotificationIconAsync();
        }

        private void ApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new MyApplicationsPage());
            SetActiveButton(ApplicationsButton);
        }

        private void PromosButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new PromosPage());
            SetActiveButton(PromosButton);
        }

        private void BonusButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new BonusHistoryPage());
            SetActiveButton(BonusButton);
        }

        private void ProfileButton_Click(object sender, MouseButtonEventArgs e)
        {
            ContentFrame.Navigate(new ProfilePage());
            SetActiveButton(null);
        }

        public void SwitchToOrganizerMode()
        {
            isOrganizerMode = true;
            UpdateMenuForMode();
        }
    }
}
