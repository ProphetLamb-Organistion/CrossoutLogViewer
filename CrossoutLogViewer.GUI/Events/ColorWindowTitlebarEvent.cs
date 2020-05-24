using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.GUI.Events
{
    public delegate void ColorWindowTitlebarEventHandler(object sender, ColorWindowTitlebarEventArgs e);
    public class ColorWindowTitlebarEventArgs : EventArgs
    {
        public ColorWindowTitlebarEventArgs(bool oldValue, bool newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public bool OldValue { get; }
        public bool NewValue { get; }
    }
}
