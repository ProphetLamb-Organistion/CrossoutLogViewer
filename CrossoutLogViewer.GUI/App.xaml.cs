using ControlzEx.Theming;

using CrossoutLogView.Common;
using CrossoutLogView.Database;
using CrossoutLogView.Database.Data;

using MahApps.Metro.Controls.Dialogs;

using System;
using System.IO;
using System.Windows;

namespace CrossoutLogView.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static Theme _theme = ThemeManager.Current.DetectTheme();
        internal static Theme Theme
        {
            get => _theme;
            set
            {
                ThemeManager.Current.ChangeTheme(Application.Current, _theme = value);
                Settings.Current.ColorScheme = _theme.ColorScheme;
                Settings.Current.BaseColorScheme = Theme.BaseColorScheme;
            }
        }

        internal static string BaseColorScheme => Theme.BaseColorScheme;

        internal static string AccentColorScheme => Theme.ColorScheme;

        internal static byte DarkThemeColorAlpha = 0x38;

        internal static byte LightThemeColorAlpha = 0x7A;

        internal static ControlService SessionControlService;

        private static bool _initialized = false;
        internal static void InitializeEnvironment()
        {
            if (!_initialized)
            {
                Logging.WriteLine<App>("Initialize environment.", true);
                if (File.Exists(Strings.DataBaseEventLogPath) && PathUtility.GetFileSize(Strings.DataBaseEventLogPath) >= 1048576 /**1mb in bytes**/)
                    File.WriteAllText(Strings.DataBaseEventLogPath, String.Empty); //clear log
                MainWindowViewModel.ApplyColors();
                SessionControlService = new ControlService();
                SessionControlService.Start();
                MainWindowViewModel.UpdateStaticCollections();
                MainWindowViewModel.SubscribeToStatisticsUploader();
                Logging.WriteLine<App>("Initialized in {TP}.");
            }
            _initialized = true;
        }
    }
}
