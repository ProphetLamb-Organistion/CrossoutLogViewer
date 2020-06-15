using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.GUI.WindowsAuxilary;
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
        private readonly NavigationWindow nav;
        private readonly UserModel model;
        public UserPage(NavigationWindow nav, UserModel userViewModel)
        {
            if (userViewModel is null)
                throw new ArgumentNullException(nameof(userViewModel));
            this.nav = nav ?? throw new ArgumentNullException(nameof(nav));
            DataProvider.CompleteUser(userViewModel.User);
            model = userViewModel;
            InitializeComponent();
            UserGamesViewGames.DataGridGames.OpenViewModel += OpenViewModelClick;
            UserGamesViewGames.OpenViewModel += OpenViewModelClick;
        }

        private void OpenViewModelClick(object sender, OpenModelViewerEventArgs e)
        {
            if (e.ViewModel is PlayerGameCompositeModel pgc)
            {
                nav.Navigate(new GamePage(nav, pgc.Game));
            }
            else if (e.ViewModel is UserListModel ul)
            {
                nav.Navigate(new UserListPage(nav, ul));
            }
            else if (e.ViewModel is GameModel gm)
            {
                nav.Navigate(new GamePage(nav, gm));
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            model.UpdateCollectionsSafe();
            DataContext = UserGamesViewGames.User = model;
            ListBoxWeapons.ItemsSource = model.Weapons;
            ListBoxStripes.ItemsSource = model.Stripes;
        }
    }
}
