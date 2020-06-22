using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CrossoutLogView.GUI.ValueConverters
{
    public class UserOverviewGroupTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string) || targetType == typeof(object))
            {
                if (value is null)
                    return String.Empty;
                if (value is DisplayMode mode)
                {
                    if (parameter is null || !(parameter is string kind) || String.IsNullOrEmpty(kind))
                        throw new ArgumentException("Parameter must be a string, and cannot be null or empty,");
                    var title = App.GetControlResource("UserOverview_" + kind);
                    if (mode == DisplayMode.Average)
                        title += App.GetControlResource("UserOverview_Avg");
                    return title;
                }
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
