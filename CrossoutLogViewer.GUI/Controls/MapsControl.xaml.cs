using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.Statistics;

using MahApps.Metro.Controls;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MapsControl.xaml
    /// </summary>
    public partial class MapsControl
    {
        private GameFilter filter = new GameFilter(GameMode.All);
        private MapModel selectedItem;

        public MapsControl()
        {
            InitializeComponent();
            //hide map column
            PlayerGamesDataGrid.Columns.First(x => String.Equals(x.Header as string, "Map", StringComparison.InvariantCultureIgnoreCase)).Visibility = Visibility.Hidden;
        }


        private ObservableCollection<MapModel> _maps;
        public ObservableCollection<MapModel> Maps
        {
            get => _maps;
            set
            {
                if (value != null)
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
                try
                {
                    var uri = ImageProvider.GetMapImageUri(map.Object.Map.Name);
                    MapBackgroundImage.Source = new BitmapImage(uri);
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    Logging.WriteLine<MapsControl>(ex);
                }
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

        private void OpenMapImageClick(object sender, MouseButtonEventArgs e)
        {
            ExplorerOpenFile.OpenFile(ImageProvider.GetMapImageUri(selectedItem.Object.Map.Name));
        }
    }
}
