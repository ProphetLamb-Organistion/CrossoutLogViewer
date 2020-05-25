using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.Statistics;

using MahApps.Metro.Controls;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CrossoutLogView.GUI.Navigation
{
    /// <summary>
    /// Interaction logic for UserPage.xaml
    /// </summary>
    public partial class UserPage
    {
        private readonly Frame frame;
        public UserPage(Frame frame, UserModel userViewModel)
        {
            this.frame = frame;
            DataProvider.CompleteUser(userViewModel.Object);
            userViewModel.UpdateCollections();
            InitializeComponent();
            DataContext = UserGamesViewGames.User = userViewModel;
            ListBoxWeapons.ItemsSource = userViewModel.Weapons;
            ListBoxStripes.ItemsSource = userViewModel.Stripes;
            UserGamesViewGames.DataGridGames.OpenViewModel += OpenViewModelClick;
        }

        private void OpenViewModelClick(object sender, OpenModelViewerEventArgs e)
        {
            if (e.ViewModel is PlayerGameCompositeModel pgc)
            {
                Logging.WriteLine<UserPage>("Navigate to game");
                frame.Navigate(new GamePage(frame, pgc.Game));
            }
            else if (e.ViewModel is UserListModel ul)
            {
                Logging.WriteLine<UserPage>("Navigate to userlist");
                frame.Navigate(new UserListPage(frame, ul));
            }
        }
    }
}
