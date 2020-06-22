using CrossoutLogView.Common;
using CrossoutLogView.Database.Events;
using CrossoutLogView.Log;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace CrossoutLogView.Database.Data
{
    public class Settings
    {
        #region Instance members
        private string _myName;
        public string MyName { get => _myName; set => Set(ref _myName, value); }

        private int _myUserID;
        public int MyUserID { get => _myUserID; set => Set(ref _myUserID, value); }

        private string _logRootPath;
        public string LogRootPath { get => _logRootPath; set => Set(ref _logRootPath, value); }

        private LogConfig _logConfiguration;
        public LogConfig LogConfiguration { get => _logConfiguration; set => Set(ref _logConfiguration, value); }

        private long _statitisticsParseDate;
        public long StatisticsParseDateTime { get => _statitisticsParseDate; set => Set(ref _statitisticsParseDate, value); }

        private string _baseColorScheme = "Dark";
        public string BaseColorScheme { get => _baseColorScheme; set => Set(ref _baseColorScheme, value); }

        private string _colorScheme = "Cobalt";
        public string ColorScheme { get => _colorScheme; set => Set(ref _colorScheme, value); }

        private bool _colorWindowTitlebar = true;
        public bool ColorWindowTitlebar { get => _colorWindowTitlebar; set => Set(ref _colorWindowTitlebar, value); }

        private string _teamWonColor = "Green";
        public string TeamWon { get => _teamWonColor; set => Set(ref _teamWonColor, value); }

        private string _teamLostColor = "Red";
        public string TeamLost { get => _teamLostColor; set => Set(ref _teamLostColor, value); }

        private string _totalDamageColor = "Red";
        public string TotalDamage { get => _totalDamageColor; set => Set(ref _totalDamageColor, value); }

        private string _criticalDamageColor = "Orange";
        public string CriticalDamage { get => _criticalDamageColor; set => Set(ref _criticalDamageColor, value); }

        private string _armorDamageColor = "DodgerBlue";
        public string ArmorDamage { get => _armorDamageColor; set => Set(ref _armorDamageColor, value); }

        private string _suicideColor = "Red";
        public string Suicide { get => _suicideColor; set => Set(ref _suicideColor, value); }

        private string _despawnColor = "Brown";
        public string Despawn { get => _despawnColor; set => Set(ref _despawnColor, value); }

        private int _dimensions = 7;
        public int Dimensions { get => _dimensions; set => Set(ref _dimensions, value); }

        private string _locale = "en-US";
        public string Locale { get => _locale; set => Set(ref _locale, value); }

        private bool _startupMaximized = false;
        public bool StartupMaximized { get => _startupMaximized; set => Set(ref _startupMaximized, value); }

        private void Set<T>(ref T field, T newValue = default, [CallerMemberName] string name = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return;
            var oldValue = field;
            field = newValue;
            if (!lockWrite)
            {
                WriteInstance();
                SettingsPropertyChanged?.Invoke(this, new SettingsChangedEventArgs(name, oldValue, field));
            }
        }
        #endregion

        #region Static members
        private static bool lockWrite = false;

        public static event SettingsChangedEventHandler SettingsPropertyChanged;

        public static Settings Current { get; private set; } = default;

        public static Settings Default { get; } = new Settings();

        public static Settings ReadInstance()
        {
            lockWrite = true;
            if (!File.Exists(Strings.DataBaseCurrentSettingsPath)) Update();
            else Current = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Strings.DataBaseCurrentSettingsPath));
            lockWrite = false;
            return Current;
        }

        public static void WriteInstance()
        {
            lockWrite = true;
            if (!Directory.Exists(Strings.DataBasePath)) Directory.CreateDirectory(Strings.DataBasePath);
            File.WriteAllText(Strings.DataBaseCurrentSettingsPath, JsonConvert.SerializeObject(Current, Formatting.Indented));
            lockWrite = false;
        }

        public static void Update()
        {
            lockWrite = true;
            if (File.Exists(Strings.DataBaseCurrentSettingsPath))
            {
                Current = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Strings.DataBaseCurrentSettingsPath));
                if (String.IsNullOrWhiteSpace(Current.MyName) || Current.MyUserID == Default.MyUserID)
                {
                    lockWrite = true;
                    GetMyInfo(out var name, out var uid);
                    Current.MyName = name;
                    Current.MyUserID = uid;
                    lockWrite = false;
                    WriteInstance();
                }
            }
            else
            {
                Current = Default;
                Current.LogRootPath = PathUtility.NormalizePath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/My Games/Crossout/logs");
                Current.LogConfiguration = default;
                Current.Locale = CultureInfo.CurrentCulture.Name;
                GetMyInfo(out var name, out var uid);
                Current.MyName = name;
                Current.MyUserID = uid;
                File.WriteAllText(Strings.DataBaseCurrentSettingsPath, JsonConvert.SerializeObject(Current, Formatting.Indented));
            }
            lockWrite = false;
        }

        public static LogConfig GetLatestLog()
        {
            //list all directories created by crossout
            var logDirs = new List<LogMetadata>();
            foreach (var dir in Directory.GetDirectories(Current.LogRootPath))
            {
                if (PathUtility.TryParseCrossoutLogDirectoryName(PathUtility.GetDirectoryName(dir), out var dateTime))
                {
                    logDirs.Add(new LogMetadata(dir, dateTime.Ticks));
                }
            }
            if (logDirs.Count != 0)
            {
                //sort most recent/highest to top
                logDirs.Sort((x, y) => y.DateTime.CompareTo(x.DateTime));
                var filePath = Path.Combine(logDirs[0].Path, Strings.ComatLogName);
                return new LogConfig(logDirs[0].Path, logDirs[0].DateTime, 0, PathUtility.GetFileSize(filePath));
            }
            throw new FileNotFoundException("No log files found");
        }

        private static void GetMyInfo(out string name, out int userId)
        {
            name = String.Empty;
            userId = -1;
            try
            {
                using var fs = new FileStream(Path.Combine(GetLatestLog().Path, Strings.GameLogName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fs);
                ReadOnlySpan<char> line;
                var parser = new Tokenizer();
                while ((line = sr.ReadLine()) != null)
                {
                    if (!parser.First(line, "TSConnectionManager: negotiation complete: uid ")) continue;
                    if (!parser.MoveNext(line, ", nickName '")) continue;
                    userId = parser.CurrentInt32;
                    if (!parser.MoveNext(line, "', sessionId ")) continue;
                    name = parser.CurrentString;
                    break;
                }
            }
            catch (FileNotFoundException) { }
        }
        #endregion
    }
}
