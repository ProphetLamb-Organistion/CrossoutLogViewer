using ControlzEx.Theming;

using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CrossoutLogView.GUI.WindowsAuxilary
{
    /// <summary>
    /// Interaction logic for LauncherWindow.xaml
    /// </summary>
    public partial class LauncherWindow : MetroWindow, ILogging
    {
        private LoadingWindow loadingWindow;
        public LauncherWindow()
        {
            logger.TraceResource("WinInit");
            InitializeComponent();
            if (!String.IsNullOrEmpty(Settings.Current.BaseColorScheme) && !String.IsNullOrEmpty(Settings.Current.ColorScheme))
                App.Theme = ThemeManager.Current.ChangeTheme(App.Current, Settings.Current.BaseColorScheme, Settings.Current.ColorScheme);
            DataContext = new WindowViewModelBase();
            logger.TraceResource("WinInitD");
        }

        private void CollectedStatisticsClick(object sender, RoutedEventArgs e)
        {
            new CollectedStatisticsWindow().Show();
            Close();
        }

        private void LiveTrackingClick(object sender, RoutedEventArgs e)
        {
            new LiveTrackingWindow().Show();
            Close();
        }
        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                new SettingsWindow().ShowDialog();
            }));
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            loadingWindow = new LoadingWindow
            {
                Header = App.GetWindowResource("Lnch_LoadingHeader"),
                Message = App.GetWindowResource("Lnch_LoadingMessage")
            };
            loadingWindow.IsIndeterminate = true;
            loadingWindow.Show();
            await Task.Run(delegate
            {
                App.InitializeSession();
            });
            loadingWindow.Close();
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
