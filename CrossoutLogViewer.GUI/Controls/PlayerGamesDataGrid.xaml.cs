using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrossoutLogView.GUI.Controls
{
    /// <summary>
    /// Interaction logic for PlayerGamesDataGrid.xaml
    /// </summary>
    public partial class PlayerGamesDataGrid
    {
        public event OpenModelViewerEventHandler OpenViewModel;

        public PlayerGamesDataGrid()
        {
            InitializeComponent();
            foreach (var column in Columns)
            {
                column.CanUserSort = true;
                column.IsReadOnly = true;
            }
        }

        private void OnOpenViewModel(object sender, MouseButtonEventArgs e)
        {
            if (DataGridHelper.GetSourceCellElement(e) is DataGridCell)
            {
                if (SelectedItems.Count == 1)
                {
                    OpenSelectedGame();
                }
                else if (SelectedItems.Count > 1)
                {
                    OpenSelectedGamesUsers();
                }
            }
        }

        private void OpenGameClick(object sender, RoutedEventArgs e)
        {
            OpenSelectedGame();
        }

        private void OpenGameUsersClick(object sender, RoutedEventArgs e)
        {
            OpenSelectedGamesUsers();
        }

        public void OpenSelectedGame()
        {
            if (SelectedItems[0] is PlayerGameCompositeModel pgc)
                OpenViewModel?.Invoke(pgc, new OpenModelViewerEventArgs(pgc));
        }

        public void OpenSelectedGamesUsers()
        {
            if (SelectedItems == null || SelectedItems.Count == 0) return;
            var users = User.ParseUsers(SelectedItems
                .Cast<PlayerGameCompositeModel>()
                .Select(x => x.Game.Object))
                .Select(x => new UserModel(x))
                .ToList();
            OpenViewModel?.Invoke(this, new OpenModelViewerEventArgs(new UserListModel(users)));
        }

        public void OpenAllGamesUsers()
        {
            if (ItemsSource == null) return;
            var games = new List<Game>();
            foreach (var item in CollectionViewSource.GetDefaultView(ItemsSource)) //items in collectionview respect filter
            {
                if (item is PlayerGameCompositeModel pgm) games.Add(pgm.Game.Object);
            }
            var users = new List<UserModel>();
            foreach (var user in User.ParseUsers(games))
            {
                users.Add(new UserModel(user));
            }
            OpenViewModel?.Invoke(this, new OpenModelViewerEventArgs(new UserListModel(users)));
        }
    }
}
