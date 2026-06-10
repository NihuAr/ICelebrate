using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kursachzhoska.Models;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class MyReviewsPage : Page
    {
        private bool _showingReviewsAboutMe = true; // true = reviews about me, false = my reviews

        public MyReviewsPage()
        {
            InitializeComponent();
            Loaded += MyReviewsPage_Loaded;
        }

        private async void MyReviewsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MyReviewsPage_Loaded;
            await LoadReviewsAsync();
        }

        private async Task LoadReviewsAsync()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser == null)
            {
                EmptyStatePanel.Visibility = Visibility.Visible;
                ReviewsPanel.Visibility = Visibility.Collapsed;
                OrganizerTogglePanel.Visibility = Visibility.Collapsed;
                return;
            }

            // Показываем переключатель только для организатора
            if (currentUser.IsOrganizer)
            {
                OrganizerTogglePanel.Visibility = Visibility.Visible;
                PageTitle.Text = "Отзывы";
                UpdateToggleButtons();
            }
            else
            {
                OrganizerTogglePanel.Visibility = Visibility.Collapsed;
                PageTitle.Text = "Мои отзывы";
            }

            // Получаем отзывы в зависимости от типа пользователя и выбранного режима
            List<Review> reviews;
            if (currentUser.IsOrganizer)
            {
                reviews = _showingReviewsAboutMe
                    ? await ApiService.Instance.GetOrganizerReviewsAsync(currentUser.UserId)
                    : await ApiService.Instance.GetUserReviewsAsync(currentUser.UserId);
            }
            else
            {
                reviews = await ApiService.Instance.GetUserReviewsAsync(currentUser.UserId);
            }

            if (reviews.Count == 0)
            {
                EmptyStatePanel.Visibility = Visibility.Visible;
                ReviewsPanel.Visibility = Visibility.Collapsed;
                return;
            }

            EmptyStatePanel.Visibility = Visibility.Collapsed;
            ReviewsPanel.Visibility = Visibility.Visible;

            ReviewsPanel.Children.Clear();

            // Создаем карточки отзывов
            foreach (var review in reviews)
            {
                var eventData = await ApiService.Instance.GetEventByIdAsync(review.EventId);
                if (eventData == null) continue;

                var reviewCard = new Border
                {
                    Style = Application.Current.Resources["Card"] as Style,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                var stackPanel = new StackPanel();

                // Верхняя часть с названием события и датой
                var headerGrid = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 15)
                };
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var leftPanel = new StackPanel();

                // Название мероприятия
                var eventTitle = new TextBlock
                {
                    Text = eventData.Title,
                    Style = Application.Current.Resources["H3"] as Style,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                leftPanel.Children.Add(eventTitle);

                // Звезды
                var starsText = new TextBlock
                {
                    Text = new string('★', review.Rating) + new string('☆', 5 - review.Rating),
                    FontSize = 16,
                    Foreground = Application.Current.Resources["AccentColor"] as Brush
                };
                leftPanel.Children.Add(starsText);

                Grid.SetColumn(leftPanel, 0);
                headerGrid.Children.Add(leftPanel);

                // Дата отзыва
                var dateText = new TextBlock
                {
                    Text = review.CreatedDate.ToString("dd.MM.yyyy"),
                    FontSize = 14,
                    Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(dateText, 1);
                headerGrid.Children.Add(dateText);

                stackPanel.Children.Add(headerGrid);

                // Имя пользователя
                var userNameText = new TextBlock
                {
                    Text = "От: " + review.UserName,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                stackPanel.Children.Add(userNameText);

                // Текст отзыва
                var commentText = new TextBlock
                {
                    Text = review.Comment,
                    FontSize = 14,
                    Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 20
                };
                stackPanel.Children.Add(commentText);

                reviewCard.Child = stackPanel;
                ReviewsPanel.Children.Add(reviewCard);
            }
        }

        private void UpdateToggleButtons()
        {
            if (_showingReviewsAboutMe)
            {
                ReviewsAboutMeButton.Style = Application.Current.Resources["DarkButton"] as Style;
                MyReviewsButton.Style = Application.Current.Resources["LightButton"] as Style;
            }
            else
            {
                ReviewsAboutMeButton.Style = Application.Current.Resources["LightButton"] as Style;
                MyReviewsButton.Style = Application.Current.Resources["DarkButton"] as Style;
            }
        }

        private void ReviewsAboutMeButton_Click(object sender, RoutedEventArgs e)
        {
            _showingReviewsAboutMe = true;
            UpdateToggleButtons();
            _ = LoadReviewsAsync();
        }

        private void MyReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            _showingReviewsAboutMe = false;
            UpdateToggleButtons();
            _ = LoadReviewsAsync();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}

