using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Kursachzhoska.Controls;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            Loaded += ProfilePage_Loaded;
        }

        private async void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ProfilePage_Loaded;
            try
            {
                await ApiService.Instance.LoadProfileAsync();
            }
            catch { }
            LoadUserData();
        }

        private void LoadUserData()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null)
            {
                // Обновляем текстовые поля с данными пользователя
                FullNameTextBlock.Text = $"{currentUser.Name} {currentUser.Surname}";
                EmailTextBlock.Text = currentUser.Email;
                PhoneTextBlock.Text = currentUser.PhoneNumber;
                DateOfBirthTextBlock.Text = currentUser.DateOfBirth.ToString("dd.MM.yyyy");
                
                // Обновляем начальную букву в аватаре
                InitialTextBlock.Text = !string.IsNullOrEmpty(currentUser.Name) ? currentUser.Name.Substring(0, 1).ToUpper() : "?";
                
                // Загружаем фото профиля
                LoadProfileImage();
                
                // Устанавливаем стиль кнопок в зависимости от типа аккаунта
                UpdateAccountTypeButtons();
            }
        }
        
        private void UpdateAccountTypeButtons()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null)
            {
                if (currentUser.IsOrganizer)
                {
                    RegularAccountButton.Style = Application.Current.Resources["LightButton"] as Style;
                    BusinessAccountButton.Style = Application.Current.Resources["DarkButton"] as Style;
                }
                else
                {
                    RegularAccountButton.Style = Application.Current.Resources["DarkButton"] as Style;
                    BusinessAccountButton.Style = Application.Current.Resources["LightButton"] as Style;
                }
            }
        }
        
        private void LoadProfileImage()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null)
            {
                if (!string.IsNullOrEmpty(currentUser.ProfileImagePath) && File.Exists(currentUser.ProfileImagePath))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(currentUser.ProfileImagePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        
                        ProfileImage.Source = bitmap;
                        ProfileImage.Visibility = Visibility.Visible;
                        InitialTextBlock.Visibility = Visibility.Collapsed;
                    }
                    catch
                    {
                        ProfileImage.Visibility = Visibility.Collapsed;
                        InitialTextBlock.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    ProfileImage.Visibility = Visibility.Collapsed;
                    InitialTextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        private void EditAccountButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new EditProfilePage());
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var result = CustomMessageBox.Show(
                "Вы уверены, что хотите удалить аккаунт? Это действие нельзя отменить.",
                "Удаление аккаунта",
                CustomMessageBox.MessageBoxButton.YesNo);

            if (result == CustomMessageBox.MessageBoxResult.Yes)
            {
                var currentUser = ApiService.Instance.CurrentUser;
                if (currentUser != null)
                {
                    // Удаляем пользователя из базы данных
                    ApiService.Instance.DeleteUser(currentUser.UserId);
                    
                    // Очищаем текущего пользователя
                    ApiService.Instance.CurrentUser = null!;
                    
                    CustomMessageBox.Show("Аккаунт успешно удалён", "Успех", CustomMessageBox.MessageBoxButton.OK);
                    
                    // Возврат на страницу входа
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.MainFrame.Navigate(new WelcomePage());
                }
            }
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = CustomMessageBox.Show(
                "Вы уверены, что хотите выйти?",
                "Выход",
                CustomMessageBox.MessageBoxButton.YesNo);

            if (result == CustomMessageBox.MessageBoxResult.Yes)
            {
                // Очищаем текущего пользователя
                ApiService.Instance.CurrentUser = null!;
                
                // Возврат на страницу входа
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new WelcomePage());
            }
        }

        private async void RegularAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser == null)
                return;

            // Если уже Regular аккаунт, ничего не делаем
            if (!currentUser.IsOrganizer)
                return;

            // Пытаемся переключиться с Business на Regular
            var result = CustomMessageBox.Show(
                "Вы уверены, что хотите переключиться на обычный аккаунт? Вы потеряете права организатора.",
                "Подтверждение",
                CustomMessageBox.MessageBoxButton.YesNo);

            if (result == CustomMessageBox.MessageBoxResult.Yes)
            {
                currentUser.IsOrganizer = false;
                currentUser.CompanyName = null;
                currentUser.OrganizerCategories = null;
                try
                {
                    await ApiService.Instance.UpdateProfileAsync();
                    CustomMessageBox.Show("Ваш аккаунт переключён на обычный.", "Успех", CustomMessageBox.MessageBoxButton.OK);
                    
                    // Обновляем MainPage
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.MainFrame.Navigate(new MainPage());
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка переключения: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
        }

        private void BusinessAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser == null)
                return;

            // Если уже Business аккаунт, ничего не делаем
            if (currentUser.IsOrganizer)
                return;

            // Пытаемся стать организатором
            var result = CustomMessageBox.Show(
                "Хотите стать организатором?",
                "Бизнес-аккаунт",
                CustomMessageBox.MessageBoxButton.YesNo);

            if (result == CustomMessageBox.MessageBoxResult.Yes)
            {
                // Переключаем на режим организатора
                NavigationService?.Navigate(new BecomeOrganizerPage());
            }
        }
    }
}
