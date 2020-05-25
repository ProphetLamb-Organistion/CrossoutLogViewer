using ControlzEx.Theming;

using CrossoutLogView.Common;
using CrossoutLogView.Database;
using CrossoutLogView.Database.Data;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using System;
using System.IO;
using System.Windows;

namespace CrossoutLogView.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static Theme _theme = ThemeManager.Current.DetectTheme();

        private static bool isInitialized = false;

        internal const byte DarkThemeColorAlpha = 0x38;

        internal const byte LightThemeColorAlpha = 0x7A;

        internal static ControlService SessionControlService;

        internal static Theme Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                ThemeManager.Current.ChangeTheme(Application.Current, _theme);
                Settings.Current.ColorScheme = _theme.ColorScheme;
                Settings.Current.BaseColorScheme = Theme.BaseColorScheme;
            }
        }

        internal static string BaseColorScheme => Theme.BaseColorScheme;

        internal static string AccentColorScheme => Theme.ColorScheme;
        internal static void InitializeSession()
        {
            if (!isInitialized)
            {
                Logging.TrimFile(1048576);
                Logging.WriteLine<App>("Initialize environment.", true);
                SessionControlService = new ControlService();
                SessionControlService.Start();
                Logging.WriteLine<App>("Initialized in {TP}.");
            }
            isInitialized = true;
        }

        private void App_Start(object sender, StartupEventArgs e)
        {
            MetroWindow launchWindow = null;
            bool startMinimized = false;
            for (int i = 0; i < e.Args.Length; i++)
            {
                var arg = e.Args[i].TrimStart('/', '\\', '-');
                if (arg.Length == e.Args[i].Length) continue; //no prefix -> invalid command line arg
                try
                {
                    switch (arg)
                    {
                        case "LaunchCollectedStatistics":
                            launchWindow = new CollectedStatisticsWindow();
                            break;
                        case "LaunchLiveTracking":
                            launchWindow = new LiveTrackingWindow();
                            break;
                        case "StartMinimized":
                            startMinimized = true;
                            break;
                        default:
                            throw new ArgumentException("Invalid statup argument: '" + arg + "'", nameof(e.Args));
                    }
                }
                catch (ArgumentException ex)
                {
                    Logging.WriteLine<App>(ex);
                }
            }

            if (launchWindow == null) launchWindow = new LauncherWindow();
            if (startMinimized) launchWindow.WindowState = WindowState.Minimized;

            launchWindow.Show();
        }
    }
}
