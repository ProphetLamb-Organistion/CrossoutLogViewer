using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class KillAssist : ILogEntry
    {
        public KillAssist(long timeStamp, string assistant, string weapon, double elapsed, double totalDamage, DamageFlag damageFlags)
        {
            TimeStamp = timeStamp;
            Assistant = assistant;
            Weapon = weapon;
            Elapsed = elapsed;
            TotalDamage = totalDamage;
            DamageFlags = damageFlags;
        }

        public KillAssist() { }

        public long TimeStamp { get; set; }
        public string Assistant;
        public string Weapon;
        public double Elapsed;
        public double TotalDamage;
        public DamageFlag DamageFlags;
        public bool IsCriticalDamage;

        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out KillAssist deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            var parser = new Tokenizer();
            if (!parser.First(logLine, "assist by ")) return false;
            if (!parser.MoveNext(logLine, "weapon: '")) return false; ;
            var assistant = parser.CurrentString;
            if (!parser.MoveNext(logLine, "', ")) return false; ;
            var weapon = parser.CurrentString;
            if (!parser.MoveNext(logLine, " sec ago, damage: ")) return false; ;
            var elapsed = parser.CurrentSingle;
            if (!parser.MoveNext(logLine, " ")) return false; ;
            var damage = parser.CurrentSingle;
            parser.End(logLine);
            var damageFlags = DamageFlagsUtility.FromString(parser.CurrentString);
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new KillAssist(timeStamp, assistant, weapon, elapsed, damage, damageFlags);
            return true;
        }
    }
}
