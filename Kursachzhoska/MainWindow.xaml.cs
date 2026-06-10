using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Kursachzhoska.Pages;

namespace Kursachzhoska
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Устанавливаем правильную иконку кнопки максимизации при запуске
            UpdateMaximizeButtonIcon();
            
            // Начинаем с приветственной страницы
            MainFrame.Navigate(new WelcomePage());
        }

        private void UpdateMaximizeButtonIcon()
        {
            var iconKey = WindowState == WindowState.Maximized ? "RestoreIcon" : "MaximizeIcon";
            MaximizeIcon.Data = (Geometry)FindResource(iconKey);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            UpdateMaximizeButtonIcon();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}