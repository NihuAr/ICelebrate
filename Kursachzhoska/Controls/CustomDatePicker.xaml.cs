using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kursachzhoska.Controls
{
    public partial class CustomDatePicker : UserControl
    {
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(CustomDatePicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(CustomDatePicker),
                new PropertyMetadata("Выберите дату"));

        public DateTime? SelectedDate
        {
            get { return (DateTime?)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        private DateTime _displayedMonth;
        private List<DayInfo> _days = new List<DayInfo>();

        public CustomDatePicker()
        {
            InitializeComponent();
            _displayedMonth = DateTime.Now;
            UpdateCalendar();
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CustomDatePicker;
            if (control != null)
            {
                control.UpdateDisplayText();
                control.UpdateCalendar();
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CalendarPopup.IsOpen = !CalendarPopup.IsOpen;
            if (CalendarPopup.IsOpen)
            {
                MainBorder.BorderBrush = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColor");
            }
        }

        private void CalendarPopup_Closed(object sender, EventArgs e)
        {
            MainBorder.BorderBrush = (System.Windows.Media.Brush)Application.Current.FindResource("BorderColor");
        }

        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            _displayedMonth = _displayedMonth.AddMonths(-1);
            UpdateCalendar();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _displayedMonth = _displayedMonth.AddMonths(1);
            UpdateCalendar();
        }

        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is DateTime date)
            {
                SelectedDate = date;
                CalendarPopup.IsOpen = false;
            }
        }

        private void UpdateDisplayText()
        {
            if (SelectedDate.HasValue)
            {
                DisplayText.Text = SelectedDate.Value.ToString("dd.MM.yyyy");
                DisplayText.Visibility = Visibility.Visible;
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
            else
            {
                DisplayText.Visibility = Visibility.Collapsed;
                PlaceholderText.Visibility = Visibility.Visible;
                PlaceholderText.Text = Placeholder;
            }
        }

        private void UpdateCalendar()
        {
            // Update month/year header
            var culture = new CultureInfo("ru-RU");
            MonthYearText.Text = _displayedMonth.ToString("MMMM yyyy", culture);
            MonthYearText.Text = char.ToUpper(MonthYearText.Text[0]) + MonthYearText.Text.Substring(1);

            // Generate days
            _days = new List<DayInfo>();
            var firstDayOfMonth = new DateTime(_displayedMonth.Year, _displayedMonth.Month, 1);
            
            // Get the day of week (Monday = 1, Sunday = 7)
            int firstDayOfWeek = ((int)firstDayOfMonth.DayOfWeek == 0) ? 7 : (int)firstDayOfMonth.DayOfWeek;
            int daysToSubtract = firstDayOfWeek - 1;

            // Start from the Monday of the week containing the first day
            var startDate = firstDayOfMonth.AddDays(-daysToSubtract);

            // Generate 42 days (6 weeks)
            for (int i = 0; i < 42; i++)
            {
                var date = startDate.AddDays(i);
                _days.Add(new DayInfo
                {
                    Date = date,
                    Day = date.Day,
                    IsCurrentMonth = date.Month == _displayedMonth.Month,
                    IsToday = date.Date == DateTime.Today,
                    IsSelected = SelectedDate.HasValue && date.Date == SelectedDate.Value.Date
                });
            }

            DaysItemsControl.ItemsSource = _days;
        }
    }

    public class DayInfo
    {
        public DateTime Date { get; set; }
        public int Day { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool IsSelected { get; set; }
    }
}

