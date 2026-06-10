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
    public partial class BonusHistoryPage : Page
    {
        public BonusHistoryPage()
        {
            InitializeComponent();
            Loaded += BonusHistoryPage_Loaded;
        }

        private async void BonusHistoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BonusHistoryPage_Loaded;
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
                var transactions = await ApiService.Instance.GetBonusHistoryAsync();
                await ApiService.Instance.LoadProfileAsync();
                BonusBalanceText.Text = ApiService.Instance.CurrentUser?.BonusBalance.ToString();
                LoadTransactions(transactions);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void LoadTransactions(List<BonusTransaction> transactions)
        {
            TransactionsPanel.Children.Clear();

            if (transactions.Count == 0)
            {
                TransactionsPanel.Children.Add(new TextBlock
                {
                    Text = "Операций пока нет",
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                });
                return;
            }

            foreach (var transaction in transactions)
            {
                var card = CreateTransactionCard(transaction);
                TransactionsPanel.Children.Add(card);
            }
        }

        private Border CreateTransactionCard(BonusTransaction transaction)
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icon
            var iconBorder = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(20),
                Background = transaction.Amount > 0 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2")),
                Margin = new Thickness(0, 0, 12, 0)
            };

            var iconText = new TextBlock
            {
                Text = transaction.Amount > 0 ? "↑" : "↓",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = transaction.Amount > 0 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626")),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            iconBorder.Child = iconText;
            Grid.SetColumn(iconBorder, 0);
            grid.Children.Add(iconBorder);

            // Info
            var infoPanel = new StackPanel();
            infoPanel.Children.Add(new TextBlock
            {
                Text = transaction.Description,
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A2E")),
                Margin = new Thickness(0, 0, 0, 4)
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = transaction.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ACACAC"))
            });

            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            // Amount
            var amountText = new TextBlock
            {
                Text = $"{(transaction.Amount > 0 ? "+" : "")}{transaction.Amount}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = transaction.Amount > 0 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626")),
                TextAlignment = TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(amountText, 2);
            grid.Children.Add(amountText);

            card.Child = grid;
            return card;
        }
    }
}
