using CrossoutLogView.Database.Data;

using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.Database.Events
{
    public delegate void SettingsChangedEventHandler(Settings sender, SettingsChangedEventArgs e);

    public class SettingsChangedEventArgs : EventArgs
    {
        public SettingsChangedEventArgs(string name, object oldValue, object newValue)
        {
            Name = name;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public string Name { get; }
        public object OldValue { get; }
        public object NewValue { get; }
    }
}
