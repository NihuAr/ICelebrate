using System.Windows;
using System.Windows.Controls;

namespace Kursachzhoska.Pages
{
    public partial class EventDetailPage : Page
    {
        public EventDetailPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new WelcomePage());
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfilePage());
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Открыть диалог регистрации на мероприятие
            MessageBox.Show("Форма регистрации будет реализована позже", "Регистрация");
        }

        private void AddToFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Мероприятие добавлено в понравившиеся!", "Успешно");
        }
    }
}

