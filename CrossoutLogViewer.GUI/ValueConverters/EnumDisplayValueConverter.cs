using CrossoutLogView.Common;
using CrossoutLogView.GUI.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CrossoutLogView.GUI.ValueConverters
{
    public class EnumDisplayValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string) || targetType == typeof(object))
            {
                var t = value?.GetType();
                if (t != null && t.IsEnum)
                    return EnumHelper<DisplayMode>.GetDisplayValue((DisplayMode)value);
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
