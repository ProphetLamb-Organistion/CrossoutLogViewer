using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.Common
{
    public static class DateTimeExtention
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime StartOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        public static DateTime EndOfMonth(this DateTime dt)
        {
            return dt.StartOfMonth().AddDays(DateTime.DaysInMonth(dt.Year, dt.Month)).AddSeconds(-1);
        }
    }
}
