using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Helpers;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public partial class PlayerGamesDataGrid : ILogging
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

        public void OpenSelectedGame()
        {
            if (SelectedItems[0] is PlayerGameModel pgc)
                OpenViewModel?.Invoke(pgc, new OpenModelViewerEventArgs(pgc));
        }

        public void OpenSelectedGamesUsers()
        {
            if (SelectedItems == null || SelectedItems.Count == 0) return;
            var users = new ObservableCollection<UserModel>(User.Parse(SelectedItems
                .Cast<PlayerGameModel>()
                .Select(x => x.Game.Game))
                .Select(x => new UserModel(x)));
            OpenViewModel?.Invoke(this, new OpenModelViewerEventArgs(new UserListModel(users)));
        }

        public async Task OpenAllGamesUsers()
        {
            var items = Dispatcher.InvokeAsync(() => CollectionViewSource.GetDefaultView(ItemsSource)); //items in collectionview respect the current filter
            var games = new List<Game>();
            Parallel.ForEach((await items).Cast<PlayerGameModel>(), delegate (PlayerGameModel item)
            {
                games.Add(item.Game.Game);
            });
            var users = await Task.Run(() => User.Parse(games));
            var models = new UserModel[users.Count];
            Parallel.For(0, users.Count, delegate (int index)
            {
                models[index] = new UserModel(users[index]);
            });
            OpenViewModel?.Invoke(this, new OpenModelViewerEventArgs(new UserListModel(new ObservableCollection<UserModel>(models))));
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

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
