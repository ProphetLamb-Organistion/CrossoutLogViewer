using CrossoutLogView.GUI.Events;

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for SessionCalendar.xaml
    /// </summary>
    public partial class SessionCalendar : UserControl
    {
        public event DateChangedEventHandler DateChanged;
        public event SessionClickEventHandler SessionClick;

        public SessionCalendar()
        {
            InitializeComponent();
        }

        public DateTime Date { get => (DateTime)GetValue(DateProperty); set => SetValue(DateProperty, value); }
        public static readonly DependencyProperty DateProperty = DependencyProperty.Register(nameof(Date), typeof(DateTime), typeof(SessionCalendar), new PropertyMetadata(DateTime.Now, OnDatePropertyChanged));

        private static void OnDatePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is SessionCalendar cntr && e.NewValue is DateTime newValue)
            {
                cntr.DateChanged?.Invoke(cntr, new DateChangedEventArgs((DateTime?)e.OldValue, newValue));
                cntr.SelectedMonth.LoadMonth(newValue);
            }
        }

        private void Button_PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            Date = Date.AddMonths(-1);
        }

        private void Button_NextMonth_Click(object sender, RoutedEventArgs e)
        {
            Date = Date.AddMonths(1);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedMonth.LoadMonth(Date);
        }

        private void SelectedMonth_SessionClickEvent(object sender, SessionClickEventArgs e)
        {
            SessionClick?.Invoke(sender, e);
        }
    }
}
