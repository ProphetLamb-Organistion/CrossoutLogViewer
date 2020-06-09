using CrossoutLogView.Common;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.Database.Events;
using CrossoutLogView.GUI.WindowsAuxilary;
using CrossoutLogView.Log;
using CrossoutLogView.Statistics;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for LiveTracking.xaml
    /// </summary>
    public partial class LiveTrackingWindow
    {
        private readonly LiveTrackingWindowViewModel viewModel;
        private Game currentGame = new Game();
        private List<ILogEntry> gameLog;
        private bool directLaunch;

        private bool forceClose = false;

        public LiveTrackingWindow() : this(false) { }
        public LiveTrackingWindow(bool directLaunch)
        {
            this.directLaunch = directLaunch;
            Logging.WriteLine<LiveTrackingWindow>("Loading LiveTracking", true);
            InitializeComponent();
            DataContext = viewModel = new LiveTrackingWindowViewModel();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var controller = await this.ShowProgressAsync("Starting", "Preparing views.\r\nPlease stand by...", settings: new MetroDialogSettings
            {
                AnimateHide = false,
                AnimateShow = false,
                ColorScheme = MetroDialogOptions.ColorScheme
            });
            controller.SetIndeterminate();
            await Task.Delay(80); //ensure that the wait window loaded, before freezing
            if (directLaunch)
            {
                App.InitializeSession();
            }
            var uri = ImageProvider.GetMapImageUri("powerplant");
            MapImage.Source = new BitmapImage(uri);

            LogUploader.LogUploadEvent += OnLogUpload;
            await controller.CloseAsync();

            Logging.WriteLine<LiveTrackingWindow>("LiveTracking loaded in {TP}");
        }

        private void OnLogUpload(object sender, LogUploadEventArgs e)
        {

        }

        private void OpenMapImageClick(object sender, MouseButtonEventArgs e)
        {
            if (currentGame != null)
                ExplorerOpenFile.OpenFile(ImageProvider.GetMapImageUri("powerplant"));
        }

        #region Confim close
        protected override void OnClosing(CancelEventArgs e)
        {
            if (e.Cancel) return;
            if (!forceClose)
            {
                e.Cancel = true;
                Dispatcher.BeginInvoke(new Action(async delegate
                {
                    await ConfirmClose();
                }));
            }
            else
            {
                base.OnClosing(e);
            }
        }

        private async Task ConfirmClose()
        {
            var settings = new MetroDialogSettings
            {
                AnimateHide = false,
                AnimateShow = false,
                AffirmativeButtonText = "Laucher",
                NegativeButtonText = "Exit",
                FirstAuxiliaryButtonText = "Cancel",
                MaximumBodyHeight = 80,
                ColorScheme = MetroDialogOptions.ColorScheme
            };
            var result = await this.ShowMessageAsync(
                "Exit confirmation",
                "You mean to leave me, or just go back to the launcher?",
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                settings);
            switch (result)
            {
                case MessageDialogResult.Affirmative:
                    new LauncherWindow().Show();
                    forceClose = true;
                    break;
                case MessageDialogResult.FirstAuxiliary:
                    forceClose = false;
                    break;
                default:
                    forceClose = true;
                    break;
            }
            if (forceClose) Close();
        }
        #endregion
    }
}
