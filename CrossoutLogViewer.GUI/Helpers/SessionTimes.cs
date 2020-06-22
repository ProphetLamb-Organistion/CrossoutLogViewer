using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.GUI.Helpers
{
    [Flags]
    public enum SessionTimes
    {
        None =      0,
        Noon =      1 << 0,
        Afternoon = 1 << 1,
        Night =     1 << 2
    }

    public static class SessionTimesHelper
    {
        public static SessionTimes GetSessionTimes(this DateTime date) => date.DayOfWeek switch
        {
            DayOfWeek.Monday => SessionTimes.Noon,
            DayOfWeek.Tuesday => SessionTimes.Afternoon,
            DayOfWeek.Wednesday => SessionTimes.Night,
            DayOfWeek.Thursday => SessionTimes.Afternoon,
            DayOfWeek.Friday => SessionTimes.Night,
            DayOfWeek.Saturday => SessionTimes.Noon | SessionTimes.Afternoon,
            DayOfWeek.Sunday => SessionTimes.Night | SessionTimes.Noon,
            _ => SessionTimes.None                    
        };
    }
}
