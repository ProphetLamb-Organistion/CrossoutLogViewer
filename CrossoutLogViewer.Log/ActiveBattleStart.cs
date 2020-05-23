using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class ActiveBattleStart : ILogEntry
    {
        public ActiveBattleStart(long timeStamp) => TimeStamp = timeStamp;
        public ActiveBattleStart() { }

        public long TimeStamp { get; set; }

        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out ActiveBattleStart deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            if (!logLine.Contains("Active battle started.", StringComparison.InvariantCultureIgnoreCase)) return false;
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new ActiveBattleStart(timeStamp);
            return true;
        }
    }
}
