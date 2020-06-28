using CrossoutLogView.Common;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;

using MahApps.Metro.Controls;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrossoutLogView.GUI.Controls.SessionCalendar
{
    /// <summary>
    /// Interaction logic for SessionMonth.xaml
    /// </summary>
    public partial class SessionMonth : UserControl
    {
        private SessionMonthModel viewModel;
        private BackgroundWorker generateControlsWorker = new BackgroundWorker();
        private bool lockGeneration;

        public event SessionClickEventHandler SessionClick;

        public SessionMonth()
        {
            InitializeComponent();
            InitializeWorkers();
            DataContext = viewModel = new SessionMonthModel();
        }
        public DateTime StartOfMonth { get => (DateTime)GetValue(StartOfMonthProperty); set => SetValue(StartOfMonthProperty, value); }
        public static readonly DependencyProperty StartOfMonthProperty = DependencyProperty.Register(nameof(StartOfMonth), typeof(DateTime), typeof(SessionMonth), new PropertyMetadata(default(DateTime), OnStartOfMonthPropertyChanged));

        public DateTime EndOfMonth { get => (DateTime)GetValue(EndOfMonthProperty); set => SetValue(EndOfMonthProperty, value); }
        public static readonly DependencyProperty EndOfMonthProperty = DependencyProperty.Register(nameof(EndOfMonth), typeof(DateTime), typeof(SessionMonth), new PropertyMetadata(default(DateTime), OnEndOfMonthPropertyChanged));

        private static void OnStartOfMonthPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is SessionMonth cntr && e.NewValue is DateTime)
            {
                cntr.GenerateControls();
            }
        }

        private static void OnEndOfMonthPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is SessionMonth cntr && e.NewValue is DateTime)
            {
                cntr.GenerateControls();
            }
        }

        public void LoadMonth(DateTime date)
        {
            lockGeneration = true;
            StartOfMonth = date.StartOfMonth();
            EndOfMonth = date.EndOfMonth();
            lockGeneration = false;
            GenerateControls();
        }

        private void InitializeWorkers()
        {
            generateControlsWorker.DoWork += async delegate (object sender, DoWorkEventArgs e)
            {
                (DateTime start, DateTime end) = (ValueTuple<DateTime, DateTime>)e.Argument;
                var monthSpan = end - start;
                var month = start.Month;
                // Month cannot be longer then a 31 days
                if (monthSpan.Days > 31)
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "The difference between the values of StartOfMonth and EndOfMonth cannot be greater then 31d23h59m59s.999. StartOfMonth: {0}, EndOfMonth {1}.", start, end));
                // Calculate the days of the first week in the month
                DateTime startOfWeek = start.Date, endOfWeek = GetEndOfWeekInMonth(startOfWeek, month);
                var weeks = new List<DateWeek>(5);
                do
                {
                    // Add a new week with the calculated days to the list
                    weeks.Add(new DateWeek { StartOfWeek = startOfWeek, EndOfWeek = endOfWeek });
                    // Caclcualte the days of the next week in the month
                    startOfWeek = endOfWeek.AddMilliseconds(1);
                    endOfWeek = GetEndOfWeekInMonth(startOfWeek, month);
                }
                while (startOfWeek.Month == month);

                await Dispatcher.InvokeAsync(delegate
                {
                    // Apply the weeks to the view model
                    viewModel.Weeks = new ObservableCollection<DateWeek>(weeks);
                });
            };
        }

        private void GenerateControls()
        {
            if (!lockGeneration && StartOfMonth != default && EndOfMonth != default)
            {
                generateControlsWorker.RunWorkerAsync((StartOfMonth, EndOfMonth));
            }
        }

        private static DateTime GetEndOfWeekInMonth(DateTime dayInWeek, int month)
        {
            // Find end of the week
            var endOfWeek = dayInWeek.EndOfWeek();
            // While the month is different
            while (endOfWeek.Month != month && endOfWeek > dayInWeek)
            {
                // Remove one day
                endOfWeek = endOfWeek.AddDays(-1);
            }
            return endOfWeek;
        }

        private void SessionWeek_SessionClickEvent(object sender, SessionClickEventArgs e)
        {
            SessionClick?.Invoke(sender, e);
        }
    }
}
