using System;

namespace CrossoutLogView.GUI.Helpers
{
    [Flags]
    public enum Dimensions
    {
        Score = 1 << 0,
        Kills = 1 << 1,
        Assists = 1 << 2,
        Deaths = 1 << 3,
        ArmorDamageDealt = 1 << 4,
        CriticalDamageDealt = 1 << 5,
        ArmorDamageTaken = 1 << 6,
        CriticalDamageTaken = 1 << 7,
        TotalDamageDealt = 1 << 8,
        TotalDamageTaken = 1 << 9
    }
}
