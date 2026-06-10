using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kursachzhoska.Controls;
using Kursachzhoska.Models;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class RegisteredParticipantsPage : Page
    {
        private int _eventId;

        public RegisteredParticipantsPage(int eventId)
        {
            InitializeComponent();
            _eventId = eventId;
            LoadParticipants();
        }

        private void LoadParticipants()
        {
            ParticipantsPanel.Children.Clear();

            var apps = ApiService.Instance.GetEventApplicationsAsync(_eventId).Result;
            if (apps == null || apps.Count == 0) return;

            foreach (var app in apps)
            {
                var card = CreateParticipantCard(app);
                ParticipantsPanel.Children.Add(card);
            }
        }

        private Border CreateParticipantCard(ApiApplication app)
        {
            var card = new Border
            {
                Style = Application.Current.Resources["Card"] as Style,
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Аватар с инициалами
            var avatar = new Border
            {
                Width = 48,
                Height = 48,
                Background = new SolidColorBrush(Color.FromRgb(209, 213, 219)),
                CornerRadius = new CornerRadius(24),
                VerticalAlignment = VerticalAlignment.Center
            };

            var initial = new TextBlock
            {
                Text = string.IsNullOrEmpty(app.User?.FirstName) ? "?" : app.User.FirstName.Substring(0, 1).ToUpper(),
                FontFamily = Application.Current.Resources["HeadingFont"] as FontFamily,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            avatar.Child = initial;
            Grid.SetColumn(avatar, 0);
            grid.Children.Add(avatar);

            // Информация об участнике
            var infoPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var nameText = new TextBlock
            {
                Text = $"{app.User?.FirstName} {app.User?.LastName}",
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush,
                Margin = new Thickness(0, 0, 0, 4)
            };
            infoPanel.Children.Add(nameText);

            var emailText = new TextBlock
            {
                Text = !string.IsNullOrEmpty(app.User?.CompanyName) ? $"Компания: {app.User.CompanyName}" : "Участник",
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 13,
                Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush
            };
            infoPanel.Children.Add(emailText);

            Grid.SetColumn(infoPanel, 2);
            grid.Children.Add(infoPanel);

            // Дата регистрации
            var datePanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 20, 0) };

            var dateLabelText = new TextBlock
            {
                Text = "Дата регистрации",
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 11,
                Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                Margin = new Thickness(0, 0, 0, 4)
            };
            datePanel.Children.Add(dateLabelText);

            var dateValueText = new TextBlock
            {
                Text = DateTime.TryParse(app.CreatedAt, out var d) ? d.ToString("dd.MM.yyyy") : "",
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush
            };
            datePanel.Children.Add(dateValueText);

            Grid.SetColumn(datePanel, 3);
            grid.Children.Add(datePanel);

            // Статус + действия
            var statusText = new TextBlock
            {
                Text = app.Status,
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(statusText, 4);
            grid.Children.Add(statusText);

            var actionsPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            if (app.Status == "pending")
            {
                var approveBtn = BuildActionButton("✔", app.Id, (s, e) => HandleAction(app.Id, "approve"));
                var rejectBtn = BuildActionButton("✖", app.Id, (s, e) => HandleAction(app.Id, "reject"));
                actionsPanel.Children.Add(approveBtn);
                actionsPanel.Children.Add(rejectBtn);
            }
            else if (app.Status == "approved" && (app.Attendance ?? "unknown") == "unknown")
            {
                var attendedBtn = BuildActionButton("✓", app.Id, (s, e) => HandleAction(app.Id, "attended"));
                var missedBtn = BuildActionButton("✕", app.Id, (s, e) => HandleAction(app.Id, "missed"));
                actionsPanel.Children.Add(attendedBtn);
                actionsPanel.Children.Add(missedBtn);
            }

            Grid.SetColumn(actionsPanel, 3);
            grid.Children.Add(actionsPanel);

            card.Child = grid;
            return card;
        }

        private Button BuildActionButton(string content, int id, RoutedEventHandler handler)
        {
            var btn = new Button
            {
                Content = content,
                Style = Application.Current.Resources["IconButton"] as Style,
                Width = 32,
                Height = 32,
                Margin = new Thickness(4, 0, 0, 0),
                Tag = id
            };
            btn.Click += handler;
            return btn;
        }

        private async void HandleAction(int appId, string action)
        {
            try
            {
                await ApiService.Instance.ApplicationActionAsync(appId, action);
                LoadParticipants();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }
    }
}

