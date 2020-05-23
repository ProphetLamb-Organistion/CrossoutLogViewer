using ControlzEx.Theming;
using CrossoutLogView.Common;
using CrossoutLogView.Database;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

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
        internal static void InitializeEnvironment(bool forceReinitialize = false)
        {
            if (!_initialized || forceReinitialize)
            {
                if (File.Exists(Strings.DataBaseEventLogPath) && PathUtility.GetFileSize(Strings.DataBaseEventLogPath) >= 1048576 /**1mb in bytes**/)
                    File.WriteAllText(Strings.DataBaseEventLogPath, String.Empty); //clear log
                Logging.WriteLine<App>("Initialize environment.", true);
                SessionControlService = new ControlService();
                SessionControlService.Start();
                MainWindowViewModel.UpdateStaticCollections();
                MainWindowViewModel.SubscribeToStatisticsUploader();
                Settings.Update();
                if (!String.IsNullOrEmpty(Settings.Current.BaseColorScheme) && !String.IsNullOrEmpty(Settings.Current.ColorScheme))
                    Theme = ThemeManager.Current.ChangeTheme(Current, Settings.Current.BaseColorScheme, Settings.Current.ColorScheme);
                MainWindowViewModel.ApplyColors();
                Logging.WriteLine<App>("Initialized in {TP}.");
            }
            _initialized = true;
        }
    }
}
