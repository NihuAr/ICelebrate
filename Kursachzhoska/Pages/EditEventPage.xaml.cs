using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public partial class EditEventPage : Page
    {
        private string selectedCategory = "";
        private List<Contractor> selectedContractors = new List<Contractor>();
        private List<string> uploadedDocuments = new List<string>();
        private string eventImagePath = "";
        private int eventId;
        private Event currentEvent;

        public EditEventPage(int eventId)
        {
            InitializeComponent();
            this.eventId = eventId;
            Loaded += EditEventPage_Loaded;
        }

        private async void EditEventPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= EditEventPage_Loaded;
            try
            {
                currentEvent = await ApiService.Instance.GetEventByIdAsync(eventId);
                
                if (currentEvent != null)
                {
                    InitializeCategories();
                    LoadEventData();
                }
                else
                {
                    CustomMessageBox.Show("Мероприятие не найдено", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void LoadEventData()
        {
            // Загружаем основные данные
            TitleTextBox.Text = currentEvent.Title;
            LocationTextBox.Text = currentEvent.Location;
            DescriptionTextBox.Text = currentEvent.Description;
            EventDatePicker.SelectedDate = currentEvent.DateTime;
            PriceTextBox.Text = currentEvent.Price.ToString();
            
            // Загружаем категорию
            selectedCategory = currentEvent.Category;
            UpdateCategorySelection();
            
            // Загружаем подрядчиков
            selectedContractors = new List<Contractor>(currentEvent.Contractors);
            UpdateContractorsList();
            
            // Загружаем документы
            uploadedDocuments = new List<string>(currentEvent.Documents);
            UpdateDocumentsList();
            
            // Загружаем изображение
            eventImagePath = currentEvent.EventImagePath ?? "";
            LoadEventImage();
        }

        private void InitializeCategories()
        {
            var categories = new[] { "Спорт", "Книги", "IT", "Музыка", "Искусство", "Кино", "Языки" };

            if (CategoriesPanel != null)
            {
                CategoriesPanel.Children.Clear();

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
                        Text = category,
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
        }

        private void UpdateCategorySelection()
        {
            if (CategoriesPanel != null)
            {
                foreach (var child in CategoriesPanel.Children)
                {
                    if (child is Border border && border.Tag != null)
                    {
                        var isSelected = border.Tag.ToString() == selectedCategory;
                        border.Style = isSelected 
                            ? Application.Current.FindResource("CategoryTagSelected") as Style 
                            : Application.Current.FindResource("CategoryTag") as Style;

                        if (border.Child is TextBlock tb)
                        {
                            tb.Foreground = isSelected 
                                ? Brushes.White 
                                : Application.Current.FindResource("TextPrimaryColor") as Brush;
                        }
                    }
                }
            }
        }

        private void CategoryBorder_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && clickedBorder.Tag != null)
            {
                selectedCategory = clickedBorder.Tag.ToString() ?? "";
                UpdateCategorySelection();
            }
        }

        private void AddContractorButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ContractorNameTextBox.Text) && 
                !string.IsNullOrWhiteSpace(ContractorRoleTextBox.Text))
            {
                selectedContractors.Add(new Contractor
                {
                    Name = ContractorNameTextBox.Text,
                    Category = ContractorRoleTextBox.Text
                });

                ContractorNameTextBox.Text = "";
                ContractorRoleTextBox.Text = "";

                UpdateContractorsList();
            }
        }

        private void UpdateContractorsList()
        {
            if (ContractorsPanel != null)
            {
                ContractorsPanel.Children.Clear();

                foreach (var contractor in selectedContractors)
                {
                    var border = new Border
                    {
                        Style = Application.Current.FindResource("Card") as Style,
                        Margin = new Thickness(0, 0, 0, 8),
                        Padding = new Thickness(12)
                    };

                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var stackPanel = new StackPanel();
                    
                    var nameText = new TextBlock
                    {
                        Text = contractor.Name,
                        FontFamily = Application.Current.FindResource("HeadingFont") as FontFamily,
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = Application.Current.FindResource("TextPrimaryColor") as Brush
                    };
                    stackPanel.Children.Add(nameText);

                    var roleText = new TextBlock
                    {
                        Text = contractor.Category,
                        FontFamily = Application.Current.FindResource("PrimaryFont") as FontFamily,
                        FontSize = 12,
                        Foreground = Application.Current.FindResource("TextSecondaryColor") as Brush
                    };
                    stackPanel.Children.Add(roleText);

                    Grid.SetColumn(stackPanel, 0);
                    grid.Children.Add(stackPanel);

                    var removeButton = new Button
                    {
                        Content = "×",
                        Style = Application.Current.FindResource("IconButton") as Style,
                        FontSize = 20,
                        Width = 24,
                        Height = 24,
                        Tag = contractor
                    };
                    removeButton.Click += RemoveContractorButton_Click;
                    Grid.SetColumn(removeButton, 1);
                    grid.Children.Add(removeButton);

                    border.Child = grid;
                    ContractorsPanel.Children.Add(border);
                }
            }
        }

        private void RemoveContractorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Contractor contractor)
            {
                selectedContractors.Remove(contractor);
                UpdateContractorsList();
            }
        }

        private void UploadDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            var documentPath = FileHelper.SelectAndSaveDocument();
            if (!string.IsNullOrEmpty(documentPath))
            {
                uploadedDocuments.Add(documentPath);
                UpdateDocumentsList();
            }
        }

        private void UpdateDocumentsList()
        {
            if (DocumentsPanel != null)
            {
                DocumentsPanel.Children.Clear();

                foreach (var doc in uploadedDocuments)
                {
                    var border = new Border
                    {
                        Style = Application.Current.FindResource("Card") as Style,
                        Margin = new Thickness(0, 0, 0, 8),
                        Padding = new Thickness(12)
                    };

                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var docName = new TextBlock
                    {
                        Text = "📄 " + FileHelper.GetFileName(doc),
                        FontFamily = Application.Current.FindResource("PrimaryFont") as FontFamily,
                        FontSize = 13,
                        Foreground = Application.Current.FindResource("TextPrimaryColor") as Brush,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    Grid.SetColumn(docName, 0);
                    grid.Children.Add(docName);

                    var removeButton = new Button
                    {
                        Content = "×",
                        Style = Application.Current.FindResource("IconButton") as Style,
                        FontSize = 20,
                        Width = 24,
                        Height = 24,
                        Tag = doc
                    };
                    removeButton.Click += RemoveDocumentButton_Click;
                    Grid.SetColumn(removeButton, 1);
                    grid.Children.Add(removeButton);

                    border.Child = grid;
                    DocumentsPanel.Children.Add(border);
                }
            }
        }

        private void RemoveDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string documentPath)
            {
                uploadedDocuments.Remove(documentPath);
                UpdateDocumentsList();
            }
        }

        private void UploadEventPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var imagePath = FileHelper.SelectAndSaveImage();
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
                string.IsNullOrWhiteSpace(selectedCategory))
            {
                CustomMessageBox.Show("Заполните все обязательные поля", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price))
            {
                CustomMessageBox.Show("Неверный формат цены", "Ошибка валидации", CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            // Обновляем данные мероприятия
            currentEvent.Title = TitleTextBox.Text;
            currentEvent.Category = selectedCategory;
            currentEvent.Location = LocationTextBox.Text;
            currentEvent.DateTime = EventDatePicker.SelectedDate.Value;
            currentEvent.Price = price;
            currentEvent.Description = DescriptionTextBox.Text;
            currentEvent.EventImagePath = eventImagePath;
            currentEvent.Contractors = selectedContractors;
            currentEvent.Documents = uploadedDocuments;

            try
            {
                await ApiService.Instance.UpdateEventAsync(currentEvent);
                CustomMessageBox.Show("Мероприятие успешно обновлено!", "Успех", CustomMessageBox.MessageBoxButton.OK);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}

