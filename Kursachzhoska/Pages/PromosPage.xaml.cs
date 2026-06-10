using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kursachzhoska.Controls;
using Kursachzhoska.Models;
using Kursachzhoska.Services;

namespace Kursachzhoska.Pages
{
    public partial class PromosPage : Page
    {
        private List<Promo> promos = new List<Promo>();
        private List<UserPromo> myPromos = new List<UserPromo>();

        public PromosPage()
        {
            InitializeComponent();
            Loaded += PromosPage_Loaded;
        }

        private async void PromosPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PromosPage_Loaded;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var currentUser = ApiService.Instance.CurrentUser;
            if (currentUser != null)
            {
                BonusBalanceText.Text = currentUser.BonusBalance.ToString();
            }

            try
            {
                promos = await ApiService.Instance.GetPromosAsync();
                myPromos = await ApiService.Instance.GetMyPromosAsync();
                LoadCatalog();
                LoadMyPromos();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void LoadCatalog()
        {
            CatalogPanel.Children.Clear();

            if (promos.Count == 0)
            {
                CatalogPanel.Children.Add(new TextBlock
                {
                    Text = "Промокодов пока нет",
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                });
                return;
            }

            foreach (var promo in promos)
            {
                var card = CreatePromoCard(promo);
                CatalogPanel.Children.Add(card);
            }
        }

        private void LoadMyPromos()
        {
            MyPromosPanel.Children.Clear();

            if (myPromos.Count == 0)
            {
                MyPromosPanel.Children.Add(new TextBlock
                {
                    Text = "Вы ещё не купили промокодов",
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                });
                return;
            }

            foreach (var userPromo in myPromos)
            {
                var card = CreateUserPromoCard(userPromo);
                MyPromosPanel.Children.Add(card);
            }
        }

        private Border CreatePromoCard(Promo promo)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E6")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftPanel = new StackPanel();
            leftPanel.Children.Add(new TextBlock
            {
                Text = promo.Title,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A2E")),
                Margin = new Thickness(0, 0, 0, 4)
            });

            if (!string.IsNullOrEmpty(promo.Description))
            {
                leftPanel.Children.Add(new TextBlock
                {
                    Text = promo.Description,
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B")),
                    Margin = new Thickness(0, 0, 0, 8),
                    TextWrapping = TextWrapping.Wrap
                });
            }

            var infoPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
            infoPanel.Children.Add(new TextBlock
            {
                Text = $"От: {(string.IsNullOrEmpty(promo.OrganizerCompanyName) ? promo.OrganizerName : promo.OrganizerCompanyName)}",
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ACACAC")),
                Margin = new Thickness(0, 0, 12, 0)
            });

            if (promo.ValidUntil.HasValue)
            {
                infoPanel.Children.Add(new TextBlock
                {
                    Text = $"До: {promo.ValidUntil.Value.ToShortDateString()}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ACACAC"))
                });
            }

            leftPanel.Children.Add(infoPanel);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            var rightPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right };
            rightPanel.Children.Add(new TextBlock
            {
                Text = $"{promo.BonusPrice}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4338CA")),
                TextAlignment = TextAlignment.Right
            });

            rightPanel.Children.Add(new TextBlock
            {
                Text = "баллов",
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B")),
                TextAlignment = TextAlignment.Right
            });

            if (promo.IsAvailable)
            {
                var buyBtn = new Button
                {
                    Content = "Купить",
                    Style = Application.Current.Resources["DarkButton"] as Style,
                    Padding = new Thickness(12, 6, 12, 6),
                    Margin = new Thickness(0, 8, 0, 0),
                    Tag = promo.PromoId
                };
                buyBtn.Click += BuyButton_Click;
                rightPanel.Children.Add(buyBtn);
            }
            else
            {
                rightPanel.Children.Add(new TextBlock
                {
                    Text = "Недоступен",
                    FontSize = 11,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B")),
                    Margin = new Thickness(0, 8, 0, 0),
                    TextAlignment = TextAlignment.Right
                });
            }

            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            card.Child = grid;
            return card;
        }

        private Border CreateUserPromoCard(UserPromo userPromo)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E6")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftPanel = new StackPanel();
            leftPanel.Children.Add(new TextBlock
            {
                Text = userPromo.PromoTitle,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A2E")),
                Margin = new Thickness(0, 0, 0, 4)
            });

            var codePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            codePanel.Children.Add(new TextBlock
            {
                Text = "Код: ",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B"))
            });

            var codeBox = new TextBlock
            {
                Text = userPromo.UniqueCode,
                FontSize = 12,
                FontFamily = new FontFamily("Courier New"),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A2E")),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F9")),
                Padding = new Thickness(4, 2, 4, 2)
            };
            codePanel.Children.Add(codeBox);
            leftPanel.Children.Add(codePanel);

            leftPanel.Children.Add(new TextBlock
            {
                Text = userPromo.PurchasedAt.ToShortDateString(),
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ACACAC"))
            });

            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            var qrBtn = new Button
            {
                Content = "QR-код",
                Style = Application.Current.Resources["LightButton"] as Style,
                Padding = new Thickness(12, 8, 12, 8),
                Tag = userPromo.UserPromoId
            };
            qrBtn.Click += QRButton_Click;
            Grid.SetColumn(qrBtn, 1);
            grid.Children.Add(qrBtn);

            card.Child = grid;
            return card;
        }

        private async void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is int promoId)
            {
                try
                {
                    var ok = await ApiService.Instance.PurchasePromoAsync(promoId);
                    if (ok)
                    {
                        await ApiService.Instance.LoadProfileAsync();
                        BonusBalanceText.Text = ApiService.Instance.CurrentUser?.BonusBalance.ToString();
                        promos = await ApiService.Instance.GetPromosAsync();
                        myPromos = await ApiService.Instance.GetMyPromosAsync();
                        LoadCatalog();
                        LoadMyPromos();
                        CustomMessageBox.Show("Промокод успешно куплен!", "Успех", CustomMessageBox.MessageBoxButton.OK);
                    }
                    else
                    {
                        CustomMessageBox.Show("Не удалось купить промокод", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
        }

        private async void QRButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is int userPromoId)
            {
                try
                {
                    var qrData = await ApiService.Instance.GetPromoQRAsync(userPromoId);
                    if (qrData != null)
                    {
                        // Создаём окно для отображения QR-кода
                        var window = new Window
                        {
                            Title = "QR-код промокода",
                            Width = 400,
                            Height = 500,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner,
                            Owner = Window.GetWindow(this)
                        };

                        var panel = new StackPanel { Margin = new Thickness(20) };
                        panel.Children.Add(new TextBlock
                        {
                            Text = "QR-код промокода",
                            FontSize = 16,
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 0, 0, 16),
                            TextAlignment = TextAlignment.Center
                        });

                        // Декодируем base64 и отображаем QR
                        if (!string.IsNullOrEmpty(qrData.QRImageBase64))
                        {
                            var imageBytes = Convert.FromBase64String(qrData.QRImageBase64);
                            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = new System.IO.MemoryStream(imageBytes);
                            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                            bitmap.EndInit();

                            panel.Children.Add(new Image
                            {
                                Source = bitmap,
                                Width = 300,
                                Height = 300,
                                Margin = new Thickness(0, 0, 0, 16)
                            });
                        }

                        panel.Children.Add(new TextBlock
                        {
                            Text = $"Код: {qrData.UniqueCode}",
                            FontSize = 12,
                            TextAlignment = TextAlignment.Center,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B"))
                        });

                        var closeBtn = new Button
                        {
                            Content = "Закрыть",
                            Style = Application.Current.Resources["DarkButton"] as Style,
                            Padding = new Thickness(16, 10, 16, 10),
                            Margin = new Thickness(0, 16, 0, 0),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        closeBtn.Click += (s, args) => window.Close();
                        panel.Children.Add(closeBtn);

                        window.Content = panel;
                        window.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
                }
            }
        }

        private void CatalogTab_Click(object sender, RoutedEventArgs e)
        {
            CatalogScroll.Visibility = Visibility.Visible;
            MyPromosScroll.Visibility = Visibility.Collapsed;
            CatalogTabBtn.Style = Application.Current.Resources["DarkButton"] as Style;
            MyPromosTabBtn.Style = Application.Current.Resources["LightButton"] as Style;
        }

        private void MyPromosTab_Click(object sender, RoutedEventArgs e)
        {
            CatalogScroll.Visibility = Visibility.Collapsed;
            MyPromosScroll.Visibility = Visibility.Visible;
            CatalogTabBtn.Style = Application.Current.Resources["LightButton"] as Style;
            MyPromosTabBtn.Style = Application.Current.Resources["DarkButton"] as Style;
            LoadMyPromos();
        }
    }
}
