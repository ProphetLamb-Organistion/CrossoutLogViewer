using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class Killing : ILogEntry
    {

        public Killing(long timeStamp, string victim, string killer)
        {
            TimeStamp = timeStamp;
            Victim = victim;
            Killer = killer;
        }

        public Killing() { }

        public long TimeStamp { get; set; }
        public string Victim;
        public string Killer;

        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out Killing deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            var parser = new Tokenizer();
            if (!parser.First(logLine, "Kill. Victim: ")) return false;
            if (!parser.MoveNext(logLine, "killer: ")) return false;
            var victim = parser.CurrentString;
            parser.End(logLine);
            var killer = parser.CurrentString;
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new Killing(timeStamp, victim, killer);
            return true;
        }
    }
}
