using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kursachzhoska.Controls;
using Kursachzhoska.Models;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class NotificationsPage : Page
    {
        public NotificationsPage()
        {
            InitializeComponent();
            Loaded += NotificationsPage_Loaded;
        }

        private async void NotificationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= NotificationsPage_Loaded;
            await LoadNotificationsAsync();
            await ApiService.Instance.MarkAllNotificationsReadAsync();
        }

        private async Task LoadNotificationsAsync()
        {
            NotificationsPanel.Children.Clear();

            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser == null) return;

            var notifications = await ApiService.Instance.GetNotificationsAsync();

            if (notifications.Count == 0)
            {
                var emptyMessage = new TextBlock
                {
                    Text = "Уведомлений пока нет",
                    FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                    FontSize = 16,
                    Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                NotificationsPanel.Children.Add(emptyMessage);
                return;
            }

            string currentMonth = "";
            foreach (var notification in notifications)
            {
                // Добавляем разделитель месяца
                string monthName = notification.Date.ToString("MMMM", new CultureInfo("ru-RU"));
                if (currentMonth != monthName)
                {
                    currentMonth = monthName;
                    var monthHeader = new TextBlock
                    {
                        Text = currentMonth,
                        FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                        FontSize = 12,
                        Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                        Margin = new Thickness(0, 16, 0, 8)
                    };
                    NotificationsPanel.Children.Add(monthHeader);
                }

                // Карточка уведомления
                var defaultCardBackground = Application.Current.Resources["CardBackground"] as Brush 
                                             ?? new SolidColorBrush(Color.FromRgb(31, 31, 35));
                var pendingBorderBrush = Application.Current.Resources["AccentColor"] as Brush 
                                          ?? new SolidColorBrush(Color.FromRgb(255, 140, 50));
                var card = new Border
                {
                    Style = Application.Current.Resources["Card"] as Style,
                    Margin = new Thickness(0, 0, 0, 12),
                    Background = defaultCardBackground,
                    BorderThickness = notification.RequiresConfirmation && notification.IsConfirmed == null 
                        ? new Thickness(1) 
                        : new Thickness(0),
                    BorderBrush = notification.RequiresConfirmation && notification.IsConfirmed == null 
                        ? pendingBorderBrush 
                        : Brushes.Transparent
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                // Заголовок
                var title = new TextBlock
                {
                    Text = notification.Title,
                    FontFamily = Application.Current.Resources["HeadingFont"] as FontFamily,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                stackPanel.Children.Add(title);

                // Сообщение
                var message = new TextBlock
                {
                    Text = notification.Message,
                    FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                    FontSize = 13,
                    Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 12)
                };
                stackPanel.Children.Add(message);

                if (notification.RequiresConfirmation && notification.IsConfirmed == null)
                {
                    var statusText = new TextBlock
                    {
                        Text = "Требует подтверждения",
                        FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                        FontSize = 13,
                        FontWeight = FontWeights.Medium,
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 159, 64))
                    };
                    stackPanel.Children.Add(statusText);
                }
                else if (notification.IsConfirmed != null)
                {
                    var statusText = new TextBlock
                    {
                        Text = notification.IsConfirmed == true ? "✓ Подтверждено" : "✗ Отклонено",
                        FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                        FontSize = 13,
                        FontWeight = FontWeights.Medium,
                        Foreground = notification.IsConfirmed == true ? new SolidColorBrush(Color.FromRgb(34, 197, 94)) : new SolidColorBrush(Color.FromRgb(239, 68, 68))
                    };
                    stackPanel.Children.Add(statusText);
                }

                card.Child = stackPanel;
                NotificationsPanel.Children.Add(card);
            }
        }

        private async void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser == null) return;

            var result = CustomMessageBox.Show(
                "Вы уверены, что хотите очистить все уведомления?",
                "Очистка уведомлений",
                CustomMessageBox.MessageBoxButton.YesNo);

            if (result == CustomMessageBox.MessageBoxResult.Yes)
            {
                await ApiService.Instance.ClearNotificationsAsync();
                await LoadNotificationsAsync();
            }
        }
    }
}
