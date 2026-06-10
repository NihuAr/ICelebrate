using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Kursachzhoska.Controls;
using Kursachzhoska.Helpers;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class RegisterPage : Page
    {
        private int currentStep = 1;
        private List<string> selectedCategories = new List<string>();
        private Dictionary<string, Border> categoryButtons = new Dictionary<string, Border>();

        public RegisterPage()
        {
            InitializeComponent();
            InitializeCategories();
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }

        private void InitializeCategories()
        {
            try
            {
                var categories = new[] { "Искусство", "Книги", "IT", "Музыка", "Кино", "Спорт", "Языки" };

                foreach (var category in categories)
                {
                    var border = new Border
                    {
                        Style = Application.Current.FindResource("CategoryTag") as Style,
                        Cursor = Cursors.Hand
                    };

                    var textBlock = new TextBlock
                    {
                        Text = category,
                        FontFamily = Application.Current.FindResource("PrimaryFont") as FontFamily,
                        FontSize = 13,
                        Foreground = Application.Current.FindResource("TextPrimaryColor") as Brush,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    border.Child = textBlock;
                    border.MouseLeftButtonDown += (s, e) => ToggleCategory(category, border, textBlock);

                    categoryButtons[category] = border;
                    CategoriesPanel.Children.Add(border);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка инициализации категорий: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void ToggleCategory(string category, Border border, TextBlock textBlock)
        {
            if (selectedCategories.Contains(category))
            {
                selectedCategories.Remove(category);
                border.Style = Application.Current.FindResource("CategoryTag") as Style;
                textBlock.Foreground = Application.Current.FindResource("TextPrimaryColor") as Brush;
            }
            else
            {
                selectedCategories.Add(category);
                border.Style = Application.Current.FindResource("CategoryTagSelected") as Style;
                textBlock.Foreground = Brushes.White;
            }
        }

        private void Step1Continue_Click(object sender, RoutedEventArgs e)
        {
            // Валидация имени
            if (!ValidationHelper.IsValidName(NameTextBox.Text))
            {
                CustomMessageBox.Show("Введите корректное имя (минимум 2 символа, только буквы)", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                NameTextBox.Focus();
                return;
            }

            // Валидация фамилии
            if (!ValidationHelper.IsValidName(SurnameTextBox.Text))
            {
                CustomMessageBox.Show("Введите корректную фамилию (минимум 2 символа, только буквы)", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                SurnameTextBox.Focus();
                return;
            }

            // Валидация даты рождения
            if (!ValidationHelper.IsValidDateOfBirth(DateOfBirthPicker.SelectedDate))
            {
                CustomMessageBox.Show("Введите корректную дату рождения (возраст от 16 до 100 лет)", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                DateOfBirthPicker.Focus();
                return;
            }

            // Проверка согласия на обработку персональных данных
            if (ConsentCheckBox.IsChecked != true)
            {
                CustomMessageBox.Show("Необходимо дать согласие на обработку персональных данных", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            currentStep = 2;
            UpdateStepDisplay();
        }

        private void Step2Back_Click(object sender, RoutedEventArgs e)
        {
            currentStep = 1;
            UpdateStepDisplay();
        }

        private void Step2Continue_Click(object sender, RoutedEventArgs e)
        {
            // Валидация email
            if (!ValidationHelper.IsValidEmail(EmailTextBox.Text))
            {
                CustomMessageBox.Show("Введите корректный адрес электронной почты", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                EmailTextBox.Focus();
                return;
            }

            // Валидация пароля
            if (!ValidationHelper.IsValidPassword(RegPasswordBox.Password))
            {
                CustomMessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                RegPasswordBox.Focus();
                return;
            }

            // Валидация телефона
            if (!ValidationHelper.IsValidPhone(PhoneTextBox.Text))
            {
                CustomMessageBox.Show("Введите корректный номер телефона (напр., +1234567890)", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                PhoneTextBox.Focus();
                return;
            }

            currentStep = 3;
            UpdateStepDisplay();
        }

        private void Step3Back_Click(object sender, RoutedEventArgs e)
        {
            currentStep = 2;
            UpdateStepDisplay();
        }

        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCategories.Count == 0)
            {
                CustomMessageBox.Show("Выберите хотя бы одну категорию", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            try
            {
                bool success = await ApiService.Instance.RegisterAsync(
                    NameTextBox.Text,
                    SurnameTextBox.Text,
                    EmailTextBox.Text,
                    RegPasswordBox.Password,
                    PhoneTextBox.Text,
                    DateOfBirthPicker.SelectedDate ?? DateTime.Now,
                    selectedCategories.ToList()
                );

                if (success)
                {
                    // Попытка залогинить сразу
                    var step1 = await ApiService.Instance.LoginStep1Async(EmailTextBox.Text, RegPasswordBox.Password);
                    if (step1.Success && !step1.RequiresOtp)
                    {
                        CustomMessageBox.Show($"Регистрация успешна!\n\nДобро пожаловать, {NameTextBox.Text}!", "Успех", CustomMessageBox.MessageBoxButton.OK);
                        NavigationService?.Navigate(new MainPage());
                    }
                    else
                    {
                        CustomMessageBox.Show("Регистрация успешна! Войдите в аккаунт.", "Успех", CustomMessageBox.MessageBoxButton.OK);
                        NavigationService?.Navigate(new LoginPage());
                    }
                }
                else
                {
                    CustomMessageBox.Show("Не удалось зарегистрироваться. Возможно, этот email уже занят.", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка регистрации: {ex.Message}\n\n{ex.InnerException?.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void UpdateStepDisplay()
        {
            // Обновляем индикаторы прогресса
            Step1Indicator.Background = currentStep >= 1 ? Application.Current.FindResource("AccentColor") as Brush : Application.Current.FindResource("BorderColor") as Brush;
            Step2Indicator.Background = currentStep >= 2 ? Application.Current.FindResource("AccentColor") as Brush : Application.Current.FindResource("BorderColor") as Brush;
            Step3Indicator.Background = currentStep >= 3 ? Application.Current.FindResource("AccentColor") as Brush : Application.Current.FindResource("BorderColor") as Brush;

            // Обновляем описание шага
            switch (currentStep)
            {
                case 1:
                    StepDescription.Text = "Шаг 1 из 3: Личные данные";
                    break;
                case 2:
                    StepDescription.Text = "Шаг 2 из 3: Контактная информация";
                    break;
                case 3:
                    StepDescription.Text = "Шаг 3 из 3: Предпочтения";
                    break;
            }

            // Показываем соответствующий панель
            Step1Panel.Visibility = currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            Step2Panel.Visibility = currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
            Step3Panel.Visibility = currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
