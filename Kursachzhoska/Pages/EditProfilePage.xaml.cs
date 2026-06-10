using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kursachzhoska.Controls;
using Kursachzhoska.Helpers;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class EditProfilePage : Page
    {
        private List<string> selectedCategories = new List<string>();

        public EditProfilePage()
        {
            InitializeComponent();
            LoadUserData();
            InitializeCategories();
        }

        private void LoadUserData()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null)
            {
                FirstNameTextBox.Text = currentUser.Name;
                LastNameTextBox.Text = currentUser.Surname;
                EmailTextBox.Text = currentUser.Email;
                PhoneTextBox.Text = currentUser.PhoneNumber;
                DateOfBirthPicker.SelectedDate = currentUser.DateOfBirth;
                selectedCategories = new List<string>(currentUser.Preferences);
                
                // Загружаем фото профиля
                LoadProfileImage();
            }
        }
        
        private void LoadProfileImage()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null)
            {
                ProfileInitial.Text = !string.IsNullOrEmpty(currentUser.Name) ? currentUser.Name.Substring(0, 1).ToUpper() : "?";
                
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
                        ProfileInitial.Visibility = Visibility.Collapsed;
                    }
                    catch
                    {
                        ProfileImage.Visibility = Visibility.Collapsed;
                        ProfileInitial.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    ProfileImage.Visibility = Visibility.Collapsed;
                    ProfileInitial.Visibility = Visibility.Visible;
                }
            }
        }
        
        private async void UploadPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser == null) return;
            
            var newImagePath = FileHelper.SelectAndSaveImage(currentUser.ProfileImagePath);
            if (!string.IsNullOrEmpty(newImagePath))
            {
                currentUser.ProfileImagePath = newImagePath;
                try
                {
                    await ApiService.Instance.UpdateProfileAsync();
                    LoadProfileImage();
                    CustomMessageBox.Show("Фото профиля успешно обновлено!", "Успех", CustomMessageBox.MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка обновления фото: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
        }

        private void InitializeCategories()
        {
            var categories = new[] { "Спорт", "IT", "Книги", "Музыка", "Мода", "Искусство", "Языки" };

            foreach (var category in categories)
            {
                var isSelected = selectedCategories.Contains(category);
                
                var border = new Border
                {
                    Style = isSelected ? Application.Current.FindResource("CategoryTagSelected") as Style : Application.Current.FindResource("CategoryTag") as Style,
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(0, 0, 8, 8)
                };

                var textBlock = new TextBlock
                {
                    Text = isSelected ? category : category + " +",
                    FontFamily = Application.Current.FindResource("PrimaryFont") as FontFamily,
                    FontSize = 13,
                    Foreground = isSelected ? Brushes.White : Application.Current.FindResource("TextPrimaryColor") as Brush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                border.Child = textBlock;
                border.Tag = category;
                border.MouseLeftButtonDown += CategoryBorder_Click;

                CategoriesPanel.Children.Add(border);
            }
        }

        private void CategoryBorder_Click(object sender, MouseButtonEventArgs e)
        {
            var clickedBorder = sender as Border;
            var category = clickedBorder?.Tag?.ToString();
            
            if (string.IsNullOrEmpty(category))
                return;

            if (selectedCategories.Contains(category))
            {
                selectedCategories.Remove(category);
                clickedBorder.Style = Application.Current.FindResource("CategoryTag") as Style;
                
                if (clickedBorder.Child is TextBlock tb)
                {
                    tb.Text = category + " +";
                    tb.Foreground = Application.Current.FindResource("TextPrimaryColor") as Brush;
                }
            }
            else
            {
                selectedCategories.Add(category);
                clickedBorder.Style = Application.Current.FindResource("CategoryTagSelected") as Style;
                
                if (clickedBorder.Child is TextBlock tb)
                {
                    tb.Text = category;
                    tb.Foreground = Brushes.White;
                }
            }
        }

        private async void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) || 
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                CustomMessageBox.Show("Заполните все обязательные поля", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            // Обновляем данные текущего пользователя
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null)
            {
                currentUser.Name = FirstNameTextBox.Text;
                currentUser.Surname = LastNameTextBox.Text;
                currentUser.Email = EmailTextBox.Text;
                currentUser.PhoneNumber = PhoneTextBox.Text;
                if (DateOfBirthPicker.SelectedDate.HasValue)
                {
                    currentUser.DateOfBirth = DateOfBirthPicker.SelectedDate.Value;
                }
                currentUser.Preferences = selectedCategories;
                
                try
                {
                    // Сохраняем изменения в файл
                    await ApiService.Instance.UpdateProfileAsync();
                    CustomMessageBox.Show("Изменения успешно сохранены!", "Успех", CustomMessageBox.MessageBoxButton.OK);
                    NavigationService?.Navigate(new ProfilePage());
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
