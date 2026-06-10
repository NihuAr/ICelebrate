using System.Windows;
using System.Windows.Controls;

namespace Kursachzhoska.Controls
{
    public partial class CustomMessageBox : Window
    {
        public enum MessageBoxButton
        {
            OK,
            OKCancel,
            YesNo,
            YesNoCancel
        }

        public enum MessageBoxResult
        {
            None,
            OK,
            Cancel,
            Yes,
            No
        }

        public MessageBoxResult Result { get; private set; }

        private CustomMessageBox(string message, string title, MessageBoxButton buttons)
        {
            InitializeComponent();
            
            TitleTextBlock.Text = title;
            MessageTextBlock.Text = message;
            
            CreateButtons(buttons);
        }

        private void CreateButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton("ОК", MessageBoxResult.OK, true);
                    break;
                
                case MessageBoxButton.OKCancel:
                    AddButton("ОК", MessageBoxResult.OK, true);
                    AddButton("Отмена", MessageBoxResult.Cancel, false, true);
                    break;
                
                case MessageBoxButton.YesNo:
                    AddButton("Да", MessageBoxResult.Yes, true);
                    AddButton("Нет", MessageBoxResult.No, false, true);
                    break;
                
                case MessageBoxButton.YesNoCancel:
                    AddButton("Да", MessageBoxResult.Yes, true);
                    AddButton("Нет", MessageBoxResult.No, false);
                    AddButton("Отмена", MessageBoxResult.Cancel, false, true);
                    break;
            }
        }

        private void AddButton(string content, MessageBoxResult result, bool isDark, bool isLast = false)
        {
            var button = new Button
            {
                Content = content,
                Style = isDark ? (Style)FindResource("DarkButton") : (Style)FindResource("LightButton"),
                MinWidth = 100,
                Margin = new Thickness(0, 0, isLast ? 0 : 12, 0)
            };

            button.Click += (s, e) =>
            {
                Result = result;
                DialogResult = true;
                Close();
            };

            ButtonsPanel.Children.Add(button);
        }

        public static MessageBoxResult Show(string message, string title = "Информация", MessageBoxButton button = MessageBoxButton.OK)
        {
            var messageBox = new CustomMessageBox(message, title, button);
            messageBox.Owner = Application.Current.MainWindow;
            messageBox.ShowDialog();
            return messageBox.Result;
        }
    }
}

