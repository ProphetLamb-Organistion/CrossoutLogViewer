using CrossoutLogView.Common;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;

using MahApps.Metro.Controls;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for SessionWeek.xaml
    /// </summary>
    public partial class SessionWeek : UserControl
    {
        private SessionWeekModel viewModel;
        private bool lockGeneration = false;

        public event SessionClickEventHandler SessionClickEvent;

        public SessionWeek()
        {
            InitializeComponent();
            DataContext = viewModel = new SessionWeekModel();
        }

        public void LoadWeek(DateTime dayInWeek)
        {
            lockGeneration = true;
            StartOfWeek = dayInWeek.StartOfWeek();
            EndOfWeek = StartOfWeek.AddDays(7).AddSeconds(-1);
            lockGeneration = false;
            GenerateButtons();
        }

        public DateTime StartOfWeek { get => (DateTime)GetValue(StartOfWeekProperty); set => SetValue(StartOfWeekProperty, value); }
        public static readonly DependencyProperty StartOfWeekProperty = DependencyProperty.Register(nameof(StartOfWeek), typeof(DateTime), typeof(SessionWeek), new PropertyMetadata(OnStartOfWeekPropertyChanged));

        public DateTime EndOfWeek { get => (DateTime)GetValue(EndOfWeekProperty); set => SetValue(EndOfWeekProperty, value); }
        public static readonly DependencyProperty EndOfWeekProperty = DependencyProperty.Register(nameof(EndOfWeek), typeof(DateTime), typeof(SessionWeek), new PropertyMetadata(OnEndOfWeekPropertyChanged));

        public static void OnStartOfWeekPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is SessionWeek cntr && e.NewValue is DateTime)
            {
                cntr.GenerateButtons();
            }
        }

        public static void OnEndOfWeekPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is SessionWeek cntr && e.NewValue is DateTime)
            {
                cntr.GenerateButtons();
            }
        }

        private void GenerateButtons()
        {
            if (!lockGeneration && StartOfWeek != default && EndOfWeek != default)
            {

                var weekSpan = EndOfWeek - StartOfWeek;
                // Week cannot be longer then a 7 days
                if (weekSpan.Days > 7)
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "The difference between the values of StartOfWeek and EndOfWeek cannot be greater then 7d23h59m59s.999. StartOfWeek: {0}, EndOfWeek {1}", StartOfWeek, EndOfWeek));
                viewModel.Days = new ObservableCollection<DateTime>().AddDays(StartOfWeek, EndOfWeek);
            }
        }

        private void DayButton_SessionClickEvent(object sender, SessionClickEventArgs e)
        {
            SessionClickEvent?.Invoke(sender, e);
        }
    }
}
