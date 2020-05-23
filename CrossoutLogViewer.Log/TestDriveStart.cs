using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class TestDriveStart : ILogEntry
    {
        public TestDriveStart(long timeStamp) => TimeStamp = timeStamp;

        public TestDriveStart() { }

        public long TimeStamp { get; set; }

        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out TestDriveStart deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            if (!logLine.Contains("====== TestDrive started ======", StringComparison.InvariantCultureIgnoreCase)) return false;
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new TestDriveStart(timeStamp);
            return true;
        }
    }
}
