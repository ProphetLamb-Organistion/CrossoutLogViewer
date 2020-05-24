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

namespace CrossoutLogView.GUI
{
    /// <summary>
    /// Interaction logic for LauncherWindow.xaml
    /// </summary>
    public partial class LauncherWindow : MetroWindow
    {
        public LauncherWindow()
        {
            Logging.WriteLine<LauncherWindow>("Loading LauncherWindow", true);
            InitializeComponent();
            Settings.Update();
            if (!String.IsNullOrEmpty(Settings.Current.BaseColorScheme) && !String.IsNullOrEmpty(Settings.Current.ColorScheme))
                App.Theme = ThemeManager.Current.ChangeTheme(App.Current, Settings.Current.BaseColorScheme, Settings.Current.ColorScheme);
            DataContext = new WindowViewModel();
            Logging.WriteLine<LauncherWindow>("LauncherWindow loaded in {TP}");
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
            new SettingsWindow().ShowDialog();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var controller = await this.ShowProgressAsync("Starting", "Initializing datastructure, collecting logs, evaluating statistics.\r\nPlease stand by.", settings: new MetroDialogSettings
            {
                AnimateHide = false,
                AnimateShow = false,
                ColorScheme = MetroDialogOptions.ColorScheme
            });
            controller.SetIndeterminate();
            await Task.Run(delegate
            {
                App.InitializeSession();
            });
            await controller.CloseAsync();
        }
    }
}
