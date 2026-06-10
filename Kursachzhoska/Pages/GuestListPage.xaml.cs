using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kursachzhoska.Pages
{
    public partial class GuestListPage : Page
    {
        public GuestListPage()
        {
            InitializeComponent();
            LoadParticipants();
        }

        private void LoadParticipants()
        {
            var participants = new[]
            {
                new { Name = "Ilya Martyniuk", Email = "ilushaunitazck@gmail.com", Date = "01.10.2025" },
                new { Name = "Julia Arlovskaya", Email = "ilushaunitazck@gmail.com", Date = "01.10.2025" },
                new { Name = "Ivan Bobko", Email = "ilushaunitazck@gmail.com", Date = "01.10.2025" },
                new { Name = "Artem Danilin", Email = "ilushaunitazck@gmail.com", Date = "01.10.2025" },
                new { Name = "Alesya Zhilevich", Email = "ilushaunitazck@gmail.com", Date = "01.10.2025" },
                new { Name = "Ekaterina Bazakovskaya", Email = "ilushaunitazck@gmail.com", Date = "01.10.2025" },
                new { Name = "Igor Haletskiy", Email = "ilushaunitazck@gmail.com", Date = "01.10.2025" }
            };

            foreach (var participant in participants)
            {
                var card = new Border
                {
                    Style = Resources["Card"] as Style,
                    Margin = new Thickness(0, 0, 0, 12),
                    Padding = new Thickness(20)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Аватар
                var avatar = new Border
                {
                    Width = 50,
                    Height = 50,
                    Background = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    CornerRadius = new CornerRadius(25),
                    Margin = new Thickness(0, 0, 16, 0)
                };
                Grid.SetColumn(avatar, 0);
                grid.Children.Add(avatar);

                // Информация
                var infoStack = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center
                };

                var nameText = new TextBlock
                {
                    Text = participant.Name,
                    FontFamily = Resources["HeadingFont"] as FontFamily,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Resources["TextPrimaryColor"] as Brush,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                infoStack.Children.Add(nameText);

                var emailText = new TextBlock
                {
                    Text = participant.Email,
                    FontFamily = Resources["PrimaryFont"] as FontFamily,
                    FontSize = 13,
                    Foreground = Resources["TextSecondaryColor"] as Brush
                };
                infoStack.Children.Add(emailText);

                Grid.SetColumn(infoStack, 1);
                grid.Children.Add(infoStack);

                // Дата регистрации
                var dateStack = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 24, 0)
                };

                var dateLabel = new TextBlock
                {
                    Text = "Дата регистрации",
                    FontFamily = Resources["PrimaryFont"] as FontFamily,
                    FontSize = 11,
                    Foreground = Resources["TextSecondaryColor"] as Brush,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                dateStack.Children.Add(dateLabel);

                var dateValue = new TextBlock
                {
                    Text = participant.Date,
                    FontFamily = Resources["PrimaryFont"] as FontFamily,
                    FontSize = 13,
                    Foreground = Resources["TextPrimaryColor"] as Brush
                };
                dateStack.Children.Add(dateValue);

                Grid.SetColumn(dateStack, 2);
                grid.Children.Add(dateStack);

                // Кнопка удаления
                var deleteButton = new Button
                {
                    Content = "🗑",
                    Style = Resources["IconButton"] as Style,
                    Width = 36,
                    Height = 36,
                    FontSize = 16,
                    Foreground = Resources["TextSecondaryColor"] as Brush
                };
                Grid.SetColumn(deleteButton, 3);
                grid.Children.Add(deleteButton);

                card.Child = grid;
                ParticipantsPanel.Children.Add(card);
            }
        }
    }
}
