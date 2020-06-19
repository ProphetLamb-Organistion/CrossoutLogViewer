using ControlzEx.Theming;

using CrossoutLogView.Common;
using CrossoutLogView.Database;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Services;
using CrossoutLogView.GUI.WindowsAuxilary;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using Microsoft.Extensions.Configuration;

using NLog.Targets;

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace CrossoutLogView.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : ILogging
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
                ThemeManager.Current.ChangeTheme(App.Current, _theme);
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
                logger.TraceResource("AppInit");
                // Start ControlService
                SessionControlService = new ControlService();
                SessionControlService.Start();
                logger.TraceResource("AppInitD");
            }
            isInitialized = true;
        }

        internal static string GetWindowResource(string key)
        {
            return ResourceManagerService.GetResourceString("WindowResources", key);
        }

        internal static string GetControlResource(string key)
        {
            return ResourceManagerService.GetResourceString("ControlResources", key);
        }

        internal static string GetSharedResource(string key)
        {
            return ResourceManagerService.GetResourceString("SharedResources", key);
        }

        internal static string GetLogResource(string key)
        {
            return ResourceManagerService.GetResourceString("LogResources", key);
        }

        private void App_Start(object sender, StartupEventArgs e)
        {
            // Ensure write permission to containing folder
            if (!FolderPermissionHelper.CanRWXByDummy())
            {

            }

            var path = Strings.ConfigPath + @"\NLog.config";
            if (File.Exists(path))
            {
                // Load logger setting
                NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(path);
            }
            // Register Resources with ResourceManagerService
            ResourceManagerService.RegisterManager("LogResources", LocalResources.LogResources.ResourceManager);
            ResourceManagerService.RegisterManager("WindowResources", LocalResources.WindowResources.ResourceManager);
            ResourceManagerService.RegisterManager("ControlResources", LocalResources.ControlResources.ResourceManager);
            ResourceManagerService.RegisterManager("SharedResources", LocalResources.SharedResources.ResourceManager);
            ResourceManagerService.Refresh();
            // Subscribe logger to all exceptions
            AppDomain.CurrentDomain.FirstChanceException += OnAppDomain_Exception;
            // Initialize Settings.Current
            Settings.Update();
            // Apply locale setting to ResourceManagerService
            ResourceManagerService.ChangeLocale(Settings.Current.Locale);

            MetroWindow launchWindow = null;
            bool startMinimized = false;
            for (int i = 0; i < e.Args.Length; i++)
            {
                var arg = e.Args[i].TrimStart('/', '\\', '-');
                if (arg.Length == e.Args[i].Length) continue; // No prefix -> invalid command line arg
                switch (arg)
                {
                    case "LaunchCollectedStatistics":
                        if (launchWindow == null)
                            launchWindow = new CollectedStatisticsWindow();
                        break;
                    case "LaunchLiveTracking":
                        if (launchWindow == null)
                            launchWindow = new LiveTrackingWindow();
                        break;
                    case "StartMinimized":
                        startMinimized = true;
                        break;
                    default:
                        break;
                }
            }

            if (launchWindow == null) launchWindow = new LauncherWindow();
            if (startMinimized) launchWindow.WindowState = WindowState.Minimized;

            launchWindow.Show();
        }

        private static void OnAppDomain_Exception(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (sender != null && typeof(ILogging).IsAssignableFrom(sender.GetType())) // Attempt to use the Logger of the cause
                (sender as ILogging).Logger.Error(e.Exception);
            else // Fallback to the App logger
                logger.Error(e.Exception);
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; }
        #endregion
    }
}
