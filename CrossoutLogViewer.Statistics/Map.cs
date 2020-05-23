using System;

namespace CrossoutLogView.Statistics
{
    public struct Map : IStatisticData, IEquatable<Map>
    {
        public string Name;
        public int GamesPlayed;

        public Map(string name, int gamesPlayed)
        {
            Name = name;
            GamesPlayed = gamesPlayed;
        }
        
        public override bool Equals(object obj) => obj is Map map && Equals(map);
        public bool Equals(Map other) => Name == other.Name && GamesPlayed == other.GamesPlayed;
        public override int GetHashCode() => HashCode.Combine(Name, GamesPlayed);

        public static bool operator ==(Map left, Map right) => left.Equals(right);
        public static bool operator !=(Map left, Map right) => !(left == right);

        public override string ToString() => String.Concat(nameof(Map), " ", Name, " ", GamesPlayed);
    }
}
