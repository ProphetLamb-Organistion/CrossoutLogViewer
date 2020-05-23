using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class TestDriveFinish : ILogEntry
    {
        public TestDriveFinish(long timeStamp) => TimeStamp = timeStamp;

        public TestDriveFinish() { }

        public long TimeStamp { get; set; }

        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out TestDriveFinish deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            if (!logLine.Contains("====== TestDrive finish ======", StringComparison.InvariantCultureIgnoreCase)) return false;
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new TestDriveFinish(timeStamp);
            return true;
        }
    }
}
