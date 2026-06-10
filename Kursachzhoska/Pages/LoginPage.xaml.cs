using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Kursachzhoska.Controls;
using Kursachzhoska.Helpers;
using Kursachzhoska.Services;
// Uses ApiService instead of DataService

namespace Kursachzhoska.Pages
{
    public partial class LoginPage : Page
    {
        private string _otpId;
        private bool _step2;
        private bool _isEmailVerificationMode;

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_step2)
            {
                // Step 1 validation
                if (!ValidationHelper.IsNotEmpty(LoginTextBox.Text))
                {
                    CustomMessageBox.Show("Введите email", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                    LoginTextBox.Focus();
                    return;
                }
                if (!ValidationHelper.IsValidPassword(PasswordBox.Password))
                {
                    CustomMessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                    PasswordBox.Focus();
                    return;
                }

                var step1 = await ApiService.Instance.LoginStep1Async(LoginTextBox.Text, PasswordBox.Password);
                if (!step1.Success)
                {
                    if (step1.Error != null && step1.Error.Contains("Подтвердите email"))
                    {
                        var confirmVerify = CustomMessageBox.Show(
                            "Ваш адрес электронной почты не подтвержден. Отправить код подтверждения на email?",
                            "Подтверждение email",
                            CustomMessageBox.MessageBoxButton.YesNo);

                        if (confirmVerify == CustomMessageBox.MessageBoxResult.Yes)
                        {
                            var otpId = await ApiService.Instance.RequestEmailVerificationAsync(LoginTextBox.Text);
                            if (!string.IsNullOrEmpty(otpId))
                            {
                                _otpId = otpId;
                                _isEmailVerificationMode = true;
                                _step2 = true;
                                Step1Panel.Visibility = Visibility.Collapsed;
                                Step2Panel.Visibility = Visibility.Visible;
                                OtpTextBox.Focus();
                                return;
                            }
                            else
                            {
                                CustomMessageBox.Show("Не удалось отправить код подтверждения. Пожалуйста, попробуйте позже.", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                            }
                        }
                    }
                    else
                    {
                        CustomMessageBox.Show(string.IsNullOrWhiteSpace(step1.Error) ? "Ошибка входа" : step1.Error, "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                    }
                    return;
                }

                if (step1.RequiresOtp)
                {
                    _otpId = step1.OtpId;
                    _isEmailVerificationMode = false;
                    _step2 = true;
                    Step1Panel.Visibility = Visibility.Collapsed;
                    Step2Panel.Visibility = Visibility.Visible;
                    OtpTextBox.Focus();
                    return;
                }

                GoToMain();
            }
            else
            {
                if (!ValidationHelper.IsNotEmpty(OtpTextBox.Text))
                {
                    CustomMessageBox.Show("Введите код", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                    return;
                }

                if (_isEmailVerificationMode)
                {
                    var ok = await ApiService.Instance.ConfirmEmailAsync(_otpId, OtpTextBox.Text);
                    if (!ok)
                    {
                        CustomMessageBox.Show("Неверный код подтверждения", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                        return;
                    }

                    CustomMessageBox.Show("Email успешно подтвержден! Теперь вы можете войти в свой аккаунт.", "Успех", CustomMessageBox.MessageBoxButton.OK);
                    
                    // Возвращаемся на шаг 1
                    _step2 = false;
                    _isEmailVerificationMode = false;
                    Step2Panel.Visibility = Visibility.Collapsed;
                    Step1Panel.Visibility = Visibility.Visible;
                }
                else
                {
                    var ok = await ApiService.Instance.LoginStep2Async(_otpId, OtpTextBox.Text, RememberDeviceCheckBox.IsChecked == true);
                    if (!ok)
                    {
                        CustomMessageBox.Show("Неверный код", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                        return;
                    }
                    GoToMain();
                }
            }
        }

        private void SignUpLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage());
        }

        private void ForgotPasswordLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PasswordResetPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _step2 = false;
            _isEmailVerificationMode = false;
            Step2Panel.Visibility = Visibility.Collapsed;
            Step1Panel.Visibility = Visibility.Visible;
        }

        private void GoToMain()
        {
            try
            {
                var mainPage = new MainPage();
                NavigationService?.Navigate(mainPage);
            }
            catch (System.Exception ex)
            {
                CustomMessageBox.Show($"Ошибка: {ex.Message}\n\n{ex.StackTrace}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }
    }
}
