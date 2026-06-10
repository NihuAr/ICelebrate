using System;
using System.Collections.Generic;
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
    public partial class MyApplicationsPage : Page
    {
        public MyApplicationsPage()
        {
            InitializeComponent();
            Loaded += MyApplicationsPage_Loaded;
        }

        private async void MyApplicationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MyApplicationsPage_Loaded;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var applications = await ApiService.Instance.GetUserApplicationsAsync();
                LoadApplications(applications);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void LoadApplications(List<UserApplication> applications)
        {
            ApplicationsPanel.Children.Clear();

            if (applications.Count == 0)
            {
                ApplicationsPanel.Children.Add(new TextBlock
                {
                    Text = "У вас нет заявок",
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                });
                return;
            }

            foreach (var app in applications)
            {
                var card = CreateApplicationCard(app);
                ApplicationsPanel.Children.Add(card);
            }
        }

        private Border CreateApplicationCard(UserApplication app)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E6")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftPanel = new StackPanel();
            leftPanel.Children.Add(new TextBlock
            {
                Text = app.EventTitle,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A2E")),
                Margin = new Thickness(0, 0, 0, 4)
            });

            var statusColor = GetStatusColor(app.Status);
            leftPanel.Children.Add(new TextBlock
            {
                Text = GetStatusText(app.Status),
                FontSize = 12,
                Foreground = new SolidColorBrush(statusColor),
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 8)
            });

            // Attendance label if set
            if (!string.Equals(app.Attendance, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                leftPanel.Children.Add(new TextBlock
                {
                    Text = GetAttendanceText(app.Attendance),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(GetAttendanceColor(app.Attendance)),
                    FontWeight = FontWeights.Medium,
                    Margin = new Thickness(0, 0, 0, 8)
                });
            }

            leftPanel.Children.Add(new TextBlock
            {
                Text = app.AppliedDate.ToShortDateString(),
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ACACAC"))
            });

            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            var rightPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right };

            if (app.Status == "pending" || app.Status == "approved")
            {
                var cancelBtn = new Button
                {
                    Content = "Отменить",
                    Style = Application.Current.Resources["LightButton"] as Style,
                    Padding = new Thickness(12, 6, 12, 6),
                    Tag = app.ApplicationId
                };
                cancelBtn.Click += CancelButton_Click;
                rightPanel.Children.Add(cancelBtn);
            }

            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            card.Child = grid;
            return card;
        }

        private Color GetStatusColor(string status)
        {
            return status switch
            {
                "pending" => (Color)ColorConverter.ConvertFromString("#F59E0B"),
                "approved" => (Color)ColorConverter.ConvertFromString("#10B981"),
                "rejected" => (Color)ColorConverter.ConvertFromString("#EF4444"),
                "cancelled" => (Color)ColorConverter.ConvertFromString("#8B8B8B"),
                _ => (Color)ColorConverter.ConvertFromString("#8B8B8B")
            };
        }

        private string GetStatusText(string status)
        {
            return status switch
            {
                "pending" => "На рассмотрении",
                "approved" => "Одобрена",
                "rejected" => "Отклонена",
                "cancelled" => "Отменена",
                _ => status
            };
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is int appId)
            {
                try
                {
                    await ApiService.Instance.ApplicationActionAsync(appId, "cancel");
                    CustomMessageBox.Show("Заявка отменена", "Успех", CustomMessageBox.MessageBoxButton.OK);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
        }

        private string GetAttendanceText(string attendance)
        {
            return attendance switch
            {
                "attended" => "Посетил",
                "missed" => "Не пришёл",
                _ => string.Empty
            };
        }

        private Color GetAttendanceColor(string attendance)
        {
            return attendance switch
            {
                "attended" => (Color)ColorConverter.ConvertFromString("#10B981"),
                "missed" => (Color)ColorConverter.ConvertFromString("#EF4444"),
                _ => (Color)ColorConverter.ConvertFromString("#8B8B8B")
            };
        }
    }
}
