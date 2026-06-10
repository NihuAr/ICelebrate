using System.Windows;
using System.Windows.Controls;

namespace Kursachzhoska.Controls
{
    public partial class TextBoxWithPlaceholder : UserControl
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(TextBoxWithPlaceholder),
                new PropertyMetadata("Enter text", OnPlaceholderChanged));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextBoxWithPlaceholder),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public TextBoxWithPlaceholder()
        {
            InitializeComponent();
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TextBoxWithPlaceholder;
            if (control != null)
            {
                control.PlaceholderText.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Text = TextBox.Text;
            UpdatePlaceholderVisibility();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Collapsed;
            MainBorder.BorderBrush = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColor");
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
            MainBorder.BorderBrush = (System.Windows.Media.Brush)Application.Current.FindResource("BorderColor");
        }

        private void UpdatePlaceholderVisibility()
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(TextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        public new void Focus()
        {
            TextBox.Focus();
        }
    }
}

