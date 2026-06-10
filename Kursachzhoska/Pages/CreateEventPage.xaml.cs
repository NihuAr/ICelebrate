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
using Kursachzhoska.Models;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class CreateEventPage : Page
    {
        private List<string> selectedCategories = new List<string>();
        private List<Contractor> contractors = new List<Contractor>();
        private List<string> documents = new List<string>();
        private string? eventImagePath = null;

        public CreateEventPage()
        {
            InitializeComponent();
            
            try
            {
                InitializeCategories();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка инициализации: {ex.Message}\n\n{ex.StackTrace}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void InitializeCategories()
        {
            if (CategoriesPanel == null) return;
            
            var categories = new[] { "Спорт", "IT", "Книги", "Музыка", "Мода", "Искусство", "Языки" };

            foreach (var category in categories)
            {
                var border = new Border
                {
                    Style = Application.Current.FindResource("CategoryTag") as Style,
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(0, 0, 8, 8)
                };

                var textBlock = new TextBlock
                {
                    Text = category + " +",
                    FontFamily = Application.Current.FindResource("PrimaryFont") as FontFamily,
                    FontSize = 13,
                    Foreground = Application.Current.FindResource("TextPrimaryColor") as Brush,
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
            
            if (string.IsNullOrEmpty(category)) return;

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

        private void AddContractorButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ContractorNameTextBox.Text) || 
                string.IsNullOrWhiteSpace(ContractorRoleTextBox.Text))
            {
                CustomMessageBox.Show("Введите имя и роль подрядчика", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            var contractor = new Contractor
            {
                Name = ContractorNameTextBox.Text,
                Category = ContractorRoleTextBox.Text
            };

            contractors.Add(contractor);
            
            // Добавляем визуальный элемент
            AddContractorToUI(contractor);
            
            // Очищаем поля
            ContractorNameTextBox.Text = "";
            ContractorRoleTextBox.Text = "";
        }
        
        private void AddContractorToUI(Contractor contractor)
        {
            var border = new Border
            {
                Style = Application.Current.Resources["Card"] as Style,
                Margin = new Thickness(0, 0, 0, 8),
                Padding = new Thickness(12),
                MaxWidth = 600,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var stackPanel = new StackPanel();
            var nameText = new TextBlock
            {
                Text = contractor.Name,
                FontFamily = Application.Current.Resources["HeadingFont"] as FontFamily,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush
            };
            stackPanel.Children.Add(nameText);

            var roleText = new TextBlock
            {
                Text = contractor.Category,
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 12,
                Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                Margin = new Thickness(0, 4, 0, 0)
            };
            stackPanel.Children.Add(roleText);

            Grid.SetColumn(stackPanel, 0);
            grid.Children.Add(stackPanel);

            var removeButton = new Button
            {
                Content = "✕",
                Style = Application.Current.Resources["IconButton"] as Style,
                Width = 24,
                Height = 24,
                FontSize = 14,
                Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                Tag = contractor
            };
            removeButton.Click += RemoveContractorButton_Click;
            Grid.SetColumn(removeButton, 1);
            grid.Children.Add(removeButton);

            border.Child = grid;
            ContractorsPanel.Children.Add(border);
        }
        
        private void RemoveContractorButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Contractor contractor)
            {
                contractors.Remove(contractor);
                
                // Удаляем визуальный элемент
                var parent = button.Parent as Grid;
                var border = parent?.Parent as Border;
                if (border != null)
                {
                    ContractorsPanel.Children.Remove(border);
                }
            }
        }
        
        private void UploadDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            var documentPath = FileHelper.SelectAndSaveDocument();
            if (!string.IsNullOrEmpty(documentPath))
            {
                documents.Add(documentPath);
                AddDocumentToUI(documentPath);
            }
        }
        
        private void AddDocumentToUI(string documentPath)
        {
            var border = new Border
            {
                Background = Application.Current.Resources["SurfaceColor"] as Brush,
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 4),
                MaxWidth = 600,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var icon = new TextBlock
            {
                Text = "📄",
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetColumn(icon, 0);
            grid.Children.Add(icon);

            var fileName = new TextBlock
            {
                Text = FileHelper.GetFileName(documentPath),
                FontFamily = Application.Current.Resources["PrimaryFont"] as FontFamily,
                FontSize = 12,
                Foreground = Application.Current.Resources["TextPrimaryColor"] as Brush,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(fileName, 1);
            grid.Children.Add(fileName);

            var removeButton = new Button
            {
                Content = "✕",
                Style = Application.Current.Resources["IconButton"] as Style,
                Width = 20,
                Height = 20,
                FontSize = 12,
                Foreground = Application.Current.Resources["TextSecondaryColor"] as Brush,
                Tag = documentPath
            };
            removeButton.Click += RemoveDocumentButton_Click;
            Grid.SetColumn(removeButton, 2);
            grid.Children.Add(removeButton);

            border.Child = grid;
            DocumentsPanel.Children.Add(border);
        }
        
        private void RemoveDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is string documentPath)
            {
                documents.Remove(documentPath);
                
                // Удаляем визуальный элемент
                var parent = button.Parent as Grid;
                var border = parent?.Parent as Border;
                if (border != null)
                {
                    DocumentsPanel.Children.Remove(border);
                }
            }
        }
        
        private void UploadEventPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var imagePath = FileHelper.SelectAndSaveImage(eventImagePath);
            if (!string.IsNullOrEmpty(imagePath))
            {
                eventImagePath = imagePath;
                LoadEventImage();
            }
        }
        
        private void LoadEventImage()
        {
            if (!string.IsNullOrEmpty(eventImagePath) && File.Exists(eventImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(eventImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    
                    EventImage.Source = bitmap;
                    EventImage.Visibility = Visibility.Visible;
                    EventImagePlaceholder.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    EventImage.Visibility = Visibility.Collapsed;
                    EventImagePlaceholder.Visibility = Visibility.Visible;
                }
            }
            else
            {
                EventImage.Visibility = Visibility.Collapsed;
                EventImagePlaceholder.Visibility = Visibility.Visible;
            }
        }

        private async void SaveEventButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                EventDatePicker.SelectedDate == null ||
                string.IsNullOrWhiteSpace(LocationTextBox.Text) ||
                selectedCategories.Count == 0)
            {
                CustomMessageBox.Show("Заполните все обязательные поля", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            // Создаем новое событие
            var newEvent = new Models.Event
            {
                Title = TitleTextBox.Text,
                Description = DescriptionTextBox.Text,
                DateTime = EventDatePicker.SelectedDate.Value,
                Location = LocationTextBox.Text,
                Category = string.Join(", ", selectedCategories),
                ImageUrl = GetColorForCategory(selectedCategories[0]),
                EventImagePath = eventImagePath,
                OrganizerId = ApiService.Instance.CurrentUser?.UserId ?? 0,
                Contractors = new List<Contractor>(contractors),
                Documents = new List<string>(documents)
            };

            try
            {
                // Добавляем событие через ApiService
                await ApiService.Instance.AddEventAsync(newEvent);
                CustomMessageBox.Show("Мероприятие успешно создано!", "Успех", CustomMessageBox.MessageBoxButton.OK);
                NavigationService?.Navigate(new MyEventsPage());
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при создании мероприятия: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private string GetColorForCategory(string category)
        {
            return category.ToLower() switch
            {
                "спорт" => "#8FBC8F",
                "книги" => "#F4A460",
                "искусство" => "#DDA0DD",
                "языки" => "#ADD8E6",
                "it" => "#9370DB",
                "музыка" => "#FFD700",
                "кино" => "#4682B4",
                "мода" => "#FF69B4",
                _ => "#6366F1"
            };
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
