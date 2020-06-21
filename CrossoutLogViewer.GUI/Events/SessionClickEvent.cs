using CrossoutLogView.GUI.Core;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CrossoutLogView.GUI.Events
{
    public delegate void SessionClickEventHandler(object sender, SessionClickEventArgs e);

    public class SessionClickEventArgs : RoutedEventArgs
    {
        public SessionClickEventArgs(SessionTimes session, DateTime day) : base()
        {
            Session = session;
            Day = day;
        }

        public SessionTimes Session { get; set; }
        public DateTime Day { get; set; }
    }
}
