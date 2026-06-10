using System;
using System.Windows;
using System.Windows.Controls;
using Kursachzhoska.Controls;
using Kursachzhoska.Helpers;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class PasswordResetPage : Page
    {
        private string _otpId;

        public PasswordResetPage()
        {
            InitializeComponent();
        }

        private async void SendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text;

            if (!ValidationHelper.IsValidEmail(email))
            {
                CustomMessageBox.Show("Введите корректный адрес электронной почты", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                EmailTextBox.Focus();
                return;
            }

            try
            {
                var otpId = await ApiService.Instance.RequestPasswordResetAsync(email);
                if (!string.IsNullOrEmpty(otpId))
                {
                    _otpId = otpId;
                    Step1Panel.Visibility = Visibility.Collapsed;
                    Step2Panel.Visibility = Visibility.Visible;
                    OtpCodeTextBox.Focus();
                    CustomMessageBox.Show("Код сброса пароля успешно отправлен на ваш email", "Успех", CustomMessageBox.MessageBoxButton.OK);
                }
                else
                {
                    CustomMessageBox.Show("Не удалось отправить код. Пожалуйста, убедитесь, что email зарегистрирован и подтвержден.", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var code = OtpCodeTextBox.Text;
            var newPassword = NewPasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(code))
            {
                CustomMessageBox.Show("Введите код подтверждения", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                OtpCodeTextBox.Focus();
                return;
            }

            if (!ValidationHelper.IsValidPassword(newPassword))
            {
                CustomMessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                NewPasswordBox.Focus();
                return;
            }

            if (newPassword != confirmPassword)
            {
                CustomMessageBox.Show("Пароли не совпадают", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                ConfirmPasswordBox.Focus();
                return;
            }

            try
            {
                var success = await ApiService.Instance.ConfirmPasswordResetAsync(_otpId, code, newPassword);
                if (success)
                {
                    CustomMessageBox.Show("Пароль успешно сброшен! Теперь вы можете войти с новым паролем.", "Успех", CustomMessageBox.MessageBoxButton.OK);
                    NavigationService?.Navigate(new LoginPage());
                }
                else
                {
                    CustomMessageBox.Show("Неверный код или срок действия кода истёк", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }

        private void BackToStep1_Click(object sender, RoutedEventArgs e)
        {
            Step2Panel.Visibility = Visibility.Collapsed;
            Step1Panel.Visibility = Visibility.Visible;
        }
    }
}
