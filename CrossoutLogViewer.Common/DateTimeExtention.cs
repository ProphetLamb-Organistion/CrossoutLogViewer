using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.Common
{
    public static class DateTimeExtention
    {
        /// <summary>
        /// Returns the first day in the week at 00:00:00.000 relative to the provided <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> used to determine to week.</param>
        /// <param name="startOfWeek">The <see cref="DayOfWeek"/> that counts as the first. Default is <see cref="DayOfWeek.Monday"/>.</param>
        /// <returns>The first day of the week at 00:00:00.000 relative to the provided <see cref="DateTime"/>.</returns>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.Date.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Returns the last day in the week at 23:59:59.999 relative to the provided <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> used to determine to week.</param>
        /// <param name="endOfWeek">The <see cref="DayOfWeek"/> that counts as the last. Default is <see cref="DayOfWeek.Sunday"/>.</param>
        /// <returns>The first day of the week at  23:59:59.999 relative to the provided <see cref="DateTime"/>.</returns>
        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek endOfWeek = DayOfWeek.Sunday)
        {
            // Calcualte days until first day of next week
            int diff = (endOfWeek - dt.DayOfWeek).NonNegModulus(7) + 1;
            // Add days, then substract 1ms
            return dt.Date.AddDays(diff).Date.AddMilliseconds(-1);
        }

        /// <summary>
        /// Return the first day in the month at 00:00:00.000 relative to the provided <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> used to determine to year and month.</param>
        /// <returns>The first day in the month at 00:00:00.000 relative to the provided <see cref="DateTime"/>.</returns>
        public static DateTime StartOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        /// <summary>
        /// Return the last day in the month at 23:59:59.999 relative to the provided <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> used to determine to year and month.</param>
        /// <returns>The last day in the month at 23:59:59.999 relative to the provided <see cref="DateTime"/>.</returns>
        public static DateTime EndOfMonth(this DateTime dt)
        {
            return dt.StartOfMonth().AddDays(DateTime.DaysInMonth(dt.Year, dt.Month)).AddMilliseconds(-1);
        }

        /// <summary>
        /// a -> [0,n)
        /// </summary>
        private static int NonNegModulus(this int a, int n)
        {
            var mod = a - n * (int)MathF.Truncate(a / (float)n);
            return mod < 0 ? mod + n : mod;
        }
    }
}
