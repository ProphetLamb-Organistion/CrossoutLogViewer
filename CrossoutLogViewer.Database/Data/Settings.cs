using CrossoutLogView.Common;
using CrossoutLogView.Log;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;

namespace CrossoutLogView.Database.Data
{
    public class Settings
    {
        private string _myName;
        public string MyName
        {
            get => _myName;
            set
            {
                _myName = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private int _myUserID;
        public int MyUserID
        {
            get => _myUserID;
            set
            {
                _myUserID = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private string _logRootPath;
        public string LogRootPath
        {
            get => _logRootPath;
            set
            {
                _logRootPath = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private LogConfig _logConfiguration;
        public LogConfig LogConfiguration
        {
            get => _logConfiguration;
            set
            {
                _logConfiguration = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private long _statitisticsParseDate;
        public long StatisticsParseDateTime
        {
            get => _statitisticsParseDate;
            set
            {
                _statitisticsParseDate = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private string _baseColorScheme;
        public string BaseColorScheme
        {
            get => _baseColorScheme;
            set
            {
                _baseColorScheme = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private string _colorScheme;
        public string ColorScheme
        {
            get => _colorScheme;
            set
            {
                _colorScheme = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private bool _colorWindowTitlebar;
        public bool ColorWindowTitlebar
        {
            get => _colorWindowTitlebar;
            set
            {
                _colorWindowTitlebar = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private string _teamWonColor = "Green";
        public string TeamWon
        {
            get => _teamWonColor;
            set
            {
                _teamWonColor = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private string _teamLostColor = "Red";
        public string TeamLost
        {
            get => _teamLostColor;
            set
            {
                _teamLostColor = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private string _totalDamageColor = "Red";
        public string TotalDamage
        {
            get => _totalDamageColor;
            set
            {
                _totalDamageColor = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private string _criticalDamageColor = "Orange";
        public string CriticalDamage
        {
            get => _criticalDamageColor;
            set
            {
                _criticalDamageColor = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private string _armorDamageColor = "DodgerBlue";
        public string ArmorDamage
        {
            get => _armorDamageColor;
            set
            {
                _armorDamageColor = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private string _suicideColor = "Red";
        public string Suicide
        {
            get => _suicideColor;
            set
            {
                _suicideColor = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private string _despawnColor = "Brown";
        public string Despawn
        {
            get => _despawnColor;
            set
            {
                _despawnColor = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private int _dimensions = 7;
        public int Dimensions
        {
            get => _dimensions;
            set
            {
                _dimensions = value;
                if (!lockWrite) WriteInstance();
            }
        }

        private static bool lockWrite = false;

        public static Settings Current { get; private set; } = default;

        public static Settings Default { get; } = new Settings
        {
            _myName = String.Empty,
            _myUserID = -1,
            _baseColorScheme = "Light",
            _colorScheme = "Cobalt",
            _colorWindowTitlebar = true,
            _teamWonColor = "Green",
            _teamLostColor = "Red",
            _totalDamageColor = "Red",
            _criticalDamageColor = "Orange",
            _armorDamageColor = "DodgerBlue",
            _suicideColor = "Red",
            _despawnColor = "Brown",
            _dimensions = 7
        };

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
            if (!Directory.Exists(Strings.DataBaseRootPath)) Directory.CreateDirectory(Strings.DataBaseRootPath);
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
            catch (FileNotFoundException ex)
            {
                Logging.WriteLine<Settings>(ex);
            }
        }
    }
}
