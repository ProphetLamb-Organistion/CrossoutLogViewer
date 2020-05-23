using System;

namespace CrossoutLogView.Statistics
{
    [Flags]
    public enum GameMode
    {
        PvP = 1 << 0,
        PvE = 1 << 1,
        Brawl = 1 << 2,
        ClanWars = 1 << 3,
        All = ~0
    }
}
