using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Kursachzhoska.Controls;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class BecomeOrganizerPage : Page
    {
        public BecomeOrganizerPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfilePage());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfilePage());
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Validation and data saving
            if (string.IsNullOrWhiteSpace(CompanyNameTextBox.Text))
            {
                CustomMessageBox.Show("Введите название компании", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                CompanyNameTextBox.Focus();
                return;
            }

            bool atLeastOneSelected = BooksCheckBox.IsChecked == true ||
                                     ArtCheckBox.IsChecked == true ||
                                     MusicCheckBox.IsChecked == true ||
                                     ITCheckBox.IsChecked == true ||
                                     SportCheckBox.IsChecked == true ||
                                     BusinessCheckBox.IsChecked == true ||
                                     EducationCheckBox.IsChecked == true ||
                                     EntertainmentCheckBox.IsChecked == true;

            if (!atLeastOneSelected)
            {
                CustomMessageBox.Show("Выберите хотя бы одну категорию", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            // Update current user data
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null)
            {
                try
                {
                    // Collect selected categories
                    var selectedCategories = new List<string>();
                    if (BooksCheckBox.IsChecked == true) selectedCategories.Add("Книги");
                    if (ArtCheckBox.IsChecked == true) selectedCategories.Add("Искусство");
                    if (MusicCheckBox.IsChecked == true) selectedCategories.Add("Музыка");
                    if (ITCheckBox.IsChecked == true) selectedCategories.Add("IT");
                    if (SportCheckBox.IsChecked == true) selectedCategories.Add("Спорт");
                    if (BusinessCheckBox.IsChecked == true) selectedCategories.Add("Бизнес");
                    if (EducationCheckBox.IsChecked == true) selectedCategories.Add("Образование");
                    if (EntertainmentCheckBox.IsChecked == true) selectedCategories.Add("Развлечения");

                    await ApiService.Instance.BecomeOrganizerAsync(CompanyNameTextBox.Text, selectedCategories);
                    CustomMessageBox.Show("Поздравляем! Теперь вы организатор!", "Успех", CustomMessageBox.MessageBoxButton.OK);

                    // Switch to organizer mode
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.MainFrame.Navigate(new MainPage(true));
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
        }
    }
}

