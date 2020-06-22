using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CrossoutLogView.GUI.Events
{
    public delegate void ValueChangedEventHandler<TValue>(object sender, ValueChangedEventArgs<TValue> e);

    public class ValueChangedEventArgs<TValue> : RoutedEventArgs
    {
        public ValueChangedEventArgs(TValue oldValue, TValue newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public TValue OldValue { get; protected set; }
        public TValue NewValue { get; protected set; }
    }
}
