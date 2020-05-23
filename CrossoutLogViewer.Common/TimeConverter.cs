using System;
using System.Globalization;

namespace CrossoutLogView.Common
{
    public static class TimeConverter
    {
        public static long FromString(ReadOnlySpan<char> lineTimeStamp, DateTime logDate)
        {
            int hour, minute, second, millisecond;
            hour = Int32.Parse(lineTimeStamp[0..2], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat);
            minute = Int32.Parse(lineTimeStamp[3..5], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat);
            second = Int32.Parse(lineTimeStamp[6..8], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat);
            millisecond = Int32.Parse(lineTimeStamp[9..12], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat);
            var dateTime = new DateTime(logDate.Year, logDate.Month, logDate.Day, hour, minute, second, millisecond);
            return dateTime.Ticks;
        }
    }
}
