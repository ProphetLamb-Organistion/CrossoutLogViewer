using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CrossoutLogView.GUI.ValueConverters
{
    public class TitleBarColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color.GetLightness() <= 96
                    ? new SolidColorBrush(Color.FromArgb(brush.Color.A,
                            (byte)Math.Min(brush.Color.R + 15, 255),
                            (byte)Math.Min(brush.Color.G + 15, 255),
                            (byte)Math.Min(brush.Color.B + 15, 255)))
                    : new SolidColorBrush(Color.FromArgb(brush.Color.A,
                            (byte)Math.Max(brush.Color.R - 30, 0),
                            (byte)Math.Max(brush.Color.G - 30, 0),
                            (byte)Math.Max(brush.Color.B - 30, 0)));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
                return brush;
            return null;
        }
    }
}
