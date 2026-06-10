using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kursachzhoska.Controls;
using Kursachzhoska.Models;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class CreateReviewPage : Page
    {
        private int selectedRating = 0;
        private int eventId;
        private Event eventData;

        public CreateReviewPage(int eventId)
        {
            InitializeComponent();
            this.eventId = eventId;
            Loaded += CreateReviewPage_Loaded;
        }

        private async void CreateReviewPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= CreateReviewPage_Loaded;

            eventData = await ApiService.Instance.GetEventByIdAsync(eventId);
            if (eventData != null)
            {
                EventTitleText.Text = eventData.Title;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                selectedRating = int.Parse(tag);
                UpdateStars();
            }
        }

        private void UpdateStars()
        {
            var accentBrush = Application.Current.FindResource("AccentColor") as SolidColorBrush;
            var secondaryBrush = Application.Current.FindResource("TextSecondaryColor") as SolidColorBrush;
            
            // Обновляем визуальное отображение звезд
            Star1.Content = selectedRating >= 1 ? "★" : "☆";
            Star1.Foreground = selectedRating >= 1 ? accentBrush : secondaryBrush;
            
            Star2.Content = selectedRating >= 2 ? "★" : "☆";
            Star2.Foreground = selectedRating >= 2 ? accentBrush : secondaryBrush;
            
            Star3.Content = selectedRating >= 3 ? "★" : "☆";
            Star3.Foreground = selectedRating >= 3 ? accentBrush : secondaryBrush;
            
            Star4.Content = selectedRating >= 4 ? "★" : "☆";
            Star4.Foreground = selectedRating >= 4 ? accentBrush : secondaryBrush;
            
            Star5.Content = selectedRating >= 5 ? "★" : "☆";
            Star5.Foreground = selectedRating >= 5 ? accentBrush : secondaryBrush;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что пользователь залогинен
            if (ApiService.Instance.CurrentUser == null)
            {
                CustomMessageBox.Show("Войдите в аккаунт, чтобы оставить отзыв", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                return;
            }
            
            // Валидация
            if (selectedRating == 0)
            {
                CustomMessageBox.Show("Выберите оценку", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            if (string.IsNullOrWhiteSpace(CommentTextBox.Text))
            {
                CustomMessageBox.Show("Напишите ваш отзыв", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                return;
            }
            
            // Проверяем, не оставлял ли пользователь уже отзыв
            if (await ApiService.Instance.HasUserReviewedEventAsync(ApiService.Instance.CurrentUser.UserId, eventId))
            {
                CustomMessageBox.Show("Вы уже оставляли отзыв на это мероприятие", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            // Создаем новый отзыв
            var review = new Review
            {
                EventId = eventId,
                UserId = ApiService.Instance.CurrentUser.UserId,
                UserName = $"{ApiService.Instance.CurrentUser.Name} {ApiService.Instance.CurrentUser.Surname}",
                Rating = selectedRating,
                Comment = CommentTextBox.Text
            };

            // Сохраняем отзыв
            await ApiService.Instance.AddReviewAsync(review);

            CustomMessageBox.Show("Отзыв успешно отправлен!", "Успех", CustomMessageBox.MessageBoxButton.OK);
            NavigationService?.GoBack();
        }
    }
}

