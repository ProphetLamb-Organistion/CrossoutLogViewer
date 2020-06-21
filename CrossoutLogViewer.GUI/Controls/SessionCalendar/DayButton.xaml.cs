using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;

using System;
using System.Collections.Generic;
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
    /// Interaction logic for DayButton.xaml
    /// </summary>
    public partial class DayButton : UserControl
    {
        public event SessionClickEventHandler SessionClick;

        public DayButton()
        {
            InitializeComponent();
        }

        public DateTime Day { get => (DateTime)GetValue(DayProperty); set => SetValue(DayProperty, value); }
        public static readonly DependencyProperty DayProperty = DependencyProperty.Register(nameof(Day), typeof(DateTime), typeof(DayButton), new PropertyMetadata(DateTime.MinValue, OnDayPropertyChanged));

        public SessionTimes Sessions { get => (SessionTimes)GetValue(SessionsProperty); protected set => SetValue(SessionsPropertyKey, value); }
        protected static readonly DependencyPropertyKey SessionsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Sessions), typeof(SessionTimes), typeof(DayButton), new PropertyMetadata(SessionTimes.None, OnSessionsPropertyChanged));
        public static readonly DependencyProperty SessionsProperty = SessionsPropertyKey.DependencyProperty;

        public bool Highlight { get => (bool)GetValue(HighliightProperty); set => SetValue(HighliightProperty, value); }
        public static readonly DependencyProperty HighliightProperty = DependencyProperty.Register(nameof(Highlight), typeof(bool), typeof(DayButton));

        private static void OnDayPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is DayButton cntr && e.NewValue is DateTime newValue)
            {
                // Assign Sessions that occur on specified date
                cntr.Sessions = newValue.GetSessionTimes();
                // Highlight if the day is today
                if (newValue.Date == DateTime.Now.Date)
                    cntr.Highlight = true;
            }
        }

        private static void OnSessionsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is DayButton cntr && e.NewValue is SessionTimes newValue)
            {
                // Show buttons specified by newValue
                cntr.ButtonNight.Visibility = (newValue & SessionTimes.Night) == SessionTimes.Night ? Visibility.Visible : Visibility.Collapsed;
                cntr.ButtonNoon.Visibility = (newValue & SessionTimes.Noon) == SessionTimes.Noon ? Visibility.Visible : Visibility.Collapsed;
                cntr.ButtonAfternoon.Visibility = (newValue & SessionTimes.Afternoon) == SessionTimes.Afternoon ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var session = btn.Name switch
                {
                    "Button_Night" => SessionTimes.Night,
                    "Button_Noon" => SessionTimes.Noon,
                    "Button_Afternoon" => SessionTimes.Afternoon,
                    _ => SessionTimes.None
                };
                SessionClick?.Invoke(this, new SessionClickEventArgs(session, Day));
            }
        }
    }
}
