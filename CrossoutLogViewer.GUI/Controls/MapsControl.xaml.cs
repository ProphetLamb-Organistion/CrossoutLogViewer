using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.Statistics;
using MahApps.Metro.Controls;
using System;
using System.Collections;
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
    /// Interaction logic for MapsView.xaml
    /// </summary>
    public partial class MapsView : UserControl
    {
        private GameFilter filter = new GameFilter(GameMode.All);
        private MapModel selectedItem;

        public MapsView()
        {
            InitializeComponent();
            //hide map column
            PlayerGamesDataGrid.Columns.First(x => String.Equals(x.Header as string, "Map", StringComparison.InvariantCultureIgnoreCase)).Visibility = Visibility.Hidden;
        }


        private List<MapModel> _maps;
        public List<MapModel> Maps
        {
            get => _maps;
            set
            {
                value.Sort(new MapModelGamesPlayedDecending());
                ListBoxMaps.ItemsSource = _maps = value;
                ListBoxMaps.SelectedIndex = 0;
            }
        }

        private void ListBoxMaps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxMaps.SelectedItem is MapModel map)
            {
                if (selectedItem != null) map.StatDisplayMode = selectedItem.StatDisplayMode;
                UserOverview.DataContext = map;
                PlayerGamesDataGrid.ItemsSource = map.Games;
                RefreshGameFilter();
                selectedItem = map;
            }
        }

        private void GameListFilter_FilterChanged(object sender, GameFilterChangedEventArgs e)
        {
            filter = e.NewValue;
            RefreshGameFilter();
        }

        private void RefreshGameFilter()
        {
            if (PlayerGamesDataGrid.ItemsSource == null) return;
            var view = CollectionViewSource.GetDefaultView(PlayerGamesDataGrid.ItemsSource);
            view.Filter = filter.Filter;
            view.Refresh();
        }

        private void OpenUsersClick(object sender, RoutedEventArgs e)
        {
            PlayerGamesDataGrid.OpenAllGamesUsers();
        }
    }
}
