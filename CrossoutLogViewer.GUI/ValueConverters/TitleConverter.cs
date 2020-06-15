using CrossoutLogView.Database.Data;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CrossoutLogView.GUI.ValueConverters
{
    public class TitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string) && value is string title)
                return title + " (" + Settings.Current.MyName + ")";
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string) && value is string str)
                return str.Split(" (")[0];
            return null;
        }
    }
}
