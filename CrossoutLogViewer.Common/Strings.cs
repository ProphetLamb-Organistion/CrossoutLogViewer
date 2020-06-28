using System;
using System.Globalization;

namespace CrossoutLogView.Common
{
    public static class Strings
    {
        public const string DataBasePath = @".\data";
        public const string ConfigPath = @".\config";
        public const string ImagePath = @".\images";
        public const string PatchPath = @".\patches";
        public const string RemoteConfigPath = @"resources\config";
        public const string RemoteImagePath = @"resources\images";
        public const string RemotePatchPath = @"resources\patches";
        public const string MetadataFile = @"metadata.json";
        public const string ScriptFolderPermissions = @".\fix_folder_permissions.bat";
        public const string DataBaseLogPath = DataBasePath + @"\logentries.db";
        public const string DataBaseStatisticsPath = DataBasePath + @"\statistics.db";
        public const string DataBaseCurrentSettingsPath = ConfigPath + @"\settings.json";
        public const string LocalizationPath = ConfigPath + @"\localization";
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
        public const string RepositoryName = "CrossoutLogViewer";
        public const string RepositoryOwner = "ProphetLamb-Organistion";
        public const string UpdaterFilePath = @".\CrossoutLogViewer.Updater.exe";

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

        public static string TimeSpanStringFactory(double seconds, string format = @"mm\:ss")
        {
            return TimeSpanStringFactory(TimeSpan.FromSeconds(seconds), format);
        }

        public static string TimeSpanStringFactory(TimeSpan span, string format = @"mm\:ss")
        {
            return span.ToString(format, CultureInfo.CurrentUICulture.DateTimeFormat);
        }

        public static string DateTimeStringFactory(DateTime dateTime, string format = "g")
        {
            return dateTime.ToString(format, CultureInfo.CurrentUICulture.DateTimeFormat);
        }
    }
}
