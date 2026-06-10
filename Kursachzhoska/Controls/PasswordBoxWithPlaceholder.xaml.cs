using System.Windows;
using System.Windows.Controls;

namespace Kursachzhoska.Controls
{
    public partial class PasswordBoxWithPlaceholder : UserControl
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(PasswordBoxWithPlaceholder),
                new PropertyMetadata("Enter password", OnPlaceholderChanged));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(PasswordBoxWithPlaceholder),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public PasswordBoxWithPlaceholder()
        {
            InitializeComponent();
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PasswordBoxWithPlaceholder;
            if (control != null)
            {
                control.PlaceholderText.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = PasswordBox.Password;
            UpdatePlaceholderVisibility();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Collapsed;
            MainBorder.BorderBrush = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColor");
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
            MainBorder.BorderBrush = (System.Windows.Media.Brush)Application.Current.FindResource("BorderColor");
        }

        private void UpdatePlaceholderVisibility()
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

