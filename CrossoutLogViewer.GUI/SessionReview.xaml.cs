using CrossoutLogView.Common;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.Database.Events;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.GUI.WindowsAuxilary;
using CrossoutLogView.Log;
using CrossoutLogView.Statistics;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CrossoutLogView.GUI
{
    /// <summary>
    /// Interaction logic for SessionReview.xaml
    /// </summary>
    public partial class SessionReview : ILogging
    {
        public SessionReview()
        {
            if (Settings.Current.StartupMaximized)
                WindowState = WindowState.Maximized;
            InitializeComponent();
            DataContext = new WindowViewModelBase();
        }


        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
                Settings.Current.StartupMaximized = WindowState == WindowState.Maximized;
        }

        private void Button_Popup_Click(object sender, RoutedEventArgs e)
        {
            Popup_SelectSession.IsOpen = true;
        }

        private void SessionCalendar_SessionClick(object sender, SessionClickEventArgs e)
        {
            var games = DataProvider.GetGames(e.Day.Date, e.Day.Date.AddDays(1).AddMilliseconds(-1)).Where(x => x.Mode == GameMode.ClanWars);
            UserListControl.ItemsSource = new ObservableCollection<UserModel>(User.ParseUsers(games).Select(x => new UserModel(x)));
        }

        private void UserListControl_OpenViewModel(object sender, OpenModelViewerEventArgs e)
        {
            new NavigationWindow(e.ViewModel).Show();
        }

        #region Confim close
        private bool forceClose = false;

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

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
