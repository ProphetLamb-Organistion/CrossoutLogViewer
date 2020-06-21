using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.GUI.Events
{
    public delegate void DateChangedEventHandler(object sender, DateChangedEventArgs e);

    public class DateChangedEventArgs : EventArgs
    {
        public DateChangedEventArgs(DateTime? oldValue, DateTime? newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public DateTime? OldValue { get; }
        public DateTime? NewValue { get; }
    }
}
