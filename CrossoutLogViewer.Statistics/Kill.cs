using System;
using System.Collections.Generic;

namespace CrossoutLogView.Statistics
{
    public class Kill : IStatisticData, IEquatable<Kill>
    {
        public double Time;
        public string Killer;
        public string Victim;
        public List<Assist> Assists;

        public Kill()
        {
            Time = default;
            Killer = Victim = String.Empty;
        }

        public Kill(double time, string killer, string victim, List<Assist> assists)
        {
            Time = time;
            Killer = killer;
            Victim = victim;
            Assists = assists;
        }

        public override bool Equals(object obj) => obj is Kill kill && Equals(kill);
        public bool Equals(Kill other) => Time == other.Time && Killer == other.Killer && Victim == other.Victim;
        public override int GetHashCode() => HashCode.Combine(Time, Killer, Victim);

        public static bool operator ==(Kill left, Kill right) => left.Equals(right);
        public static bool operator !=(Kill left, Kill right) => !(left == right);

        public static bool IsValidKill(Player killer, Player victim)
        {
            return killer != null && victim != null && !killer.IsBot;
        }

        public override string ToString() => String.Concat(nameof(Kill), " ", Time, " ", Killer, " ", Victim, " ", Assists?.Count);
    }
}
