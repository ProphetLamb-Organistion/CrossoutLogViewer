using System;

namespace CrossoutLogView.Common
{
    public static class Strings
    {
        public const string DataBaseRootPath = @".\data";
        public const string DataBaseLogPath = DataBaseRootPath + @"\logentries.db";
        public const string DataBaseStatisticsPath = DataBaseRootPath + @"\statistics.db";
        public const string DataBaseCurrentSettingsPath = DataBaseRootPath + @"\settings.json";
        public const string DataBaseEventLogPath = DataBaseRootPath + @"\event.log";
        public const string ComatLogName = "combat.log";
        public const string GameLogName = "game.log";
        public const string LogDamageFlagsCriticalDamage = "HUD_IMPORTANT";
        public const string LevelLoadNameTestDrive = "levels/maps/hangar";
        public const string LevelLoadNameMainMenu = "levels/maps/mainmenu";
        public const string GameModePvE = "Pve_";
        public const string GameModeBrawl = "Brawl_";
        public const string GameModeClanWars = "BestOf3";
        public const char ArrayDelimiter = ',';
        public const char EnumDelimiter = '|';
        public const string CenterDotSeparator = " • ";

        public static bool NameEquals(string name, string other)
        {
            if (name == null || other == null) return false;
            return name.Equals(other, StringComparison.InvariantCulture);
        }
        public static ReadOnlySpan<char> TrimName(ReadOnlySpan<char> name)
        {
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == ':')
                    return name.Slice(0, i);
            }
            return name;
        }
    }
}
