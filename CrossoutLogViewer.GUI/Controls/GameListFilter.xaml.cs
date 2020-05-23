using CrossoutLogView.GUI.Events;
using CrossoutLogView.Statistics;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for GameListFilter.xaml
    /// </summary>
    public partial class GameListFilter : UserControl
    {
        public event GameFilterChangedEventHandler FilterChanged;

        public GameListFilter()
        {
            InitializeComponent();
            ComboBoxGameMode.ItemsSource = Enum.GetValues(typeof(GameMode)).Cast<GameMode>();
            ComboBoxGameMode.SelectedItem = GameMode.All;
        }

        private bool freezeFilter = false; //protect from redundant calls
        private GameFilter _filter = new GameFilter(GameMode.All);
        public GameFilter Filter
        {
            get => _filter;
            set
            {
                if (_filter == value) return;
                if (!freezeFilter)
                {
                    freezeFilter = true;
                    if (value == default)
                    {
                        DateTimePickerStart.SelectedDateTime = null;
                        DateTimePickerEnd.SelectedDateTime = null;
                        ComboBoxGameMode.SelectedItem = GameMode.All;
                    }
                    else
                    {
                        if (value.StartLimit == default) DateTimePickerStart.SelectedDateTime = null;
                        else DateTimePickerStart.SelectedDateTime = value.StartLimit;
                        if (value.EndLimit == default) DateTimePickerEnd.SelectedDateTime = null;
                        else DateTimePickerEnd.SelectedDateTime = value.EndLimit;
                        ComboBoxGameMode.SelectedItem = value.GameModes;
                    }
                    freezeFilter = false;
                }
                FilterChanged?.Invoke(this, new GameFilterChangedEventArgs(_filter, _filter = value));
            }
        }
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(GameFilter), typeof(GameListFilter));

        public DateTime? StartLimit
        {
            get => DateTimePickerStart.SelectedDateTime;
            set
            {
                if (freezeFilter) return;
                if (_filter.StartLimit == value) return;
                freezeFilter = true;
                DateTimePickerStart.SelectedDateTime = value;
                Filter = new GameFilter(
                    _filter.GameModes,
                    value ?? default,
                    _filter.EndLimit);
                freezeFilter = false;
            }
        }
        public static readonly DependencyProperty StartLimitProperty = DependencyProperty.Register(nameof(StartLimit), typeof(DateTime?), typeof(GameListFilter));

        public DateTime? EndLimit
        {
            get => DateTimePickerEnd.SelectedDateTime;
            set
            {
                if (freezeFilter) return;
                if (_filter.EndLimit == value) return;
                freezeFilter = true;
                DateTimePickerEnd.SelectedDateTime = value;
                Filter = new GameFilter(
                    _filter.GameModes,
                   _filter.StartLimit,
                   value ?? default);
                freezeFilter = false;
            }
        }
        public static readonly DependencyProperty EndLimitProperty = DependencyProperty.Register(nameof(EndLimit), typeof(DateTime?), typeof(GameListFilter));

        public GameMode GameModes
        {
            get => (GameMode)(ComboBoxGameMode.SelectedItem ?? GameMode.All);
            set
            {
                if (freezeFilter) return;
                if (_filter.GameModes == value) return;
                freezeFilter = true;
                ComboBoxGameMode.SelectedItem = value;
                Filter = new GameFilter(
                    value,
                    _filter.StartLimit,
                    _filter.EndLimit);
                freezeFilter = false;
            }
        }
        public static readonly DependencyProperty GameModeProperty = DependencyProperty.Register(nameof(GameModes), typeof(GameMode), typeof(GameListFilter));
        
        public void ClearFilter() => ClearFilter(null, null);
        private void ClearFilter(object sender, RoutedEventArgs e)
        {
            Filter = new GameFilter(GameMode.All);
        }


        private void GameFilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GameModes = (GameMode)(sender as ComboBox).SelectedItem;
            e.Handled = true;
        }

        private void DateTimePickerStart_SelectedDateTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            var dtp = sender as DateTimePicker;
            if (dtp.Name == nameof(DateTimePickerStart))
            {
                StartLimit = dtp.SelectedDateTime;
                e.Handled = true;
            }
            else if (dtp.Name == nameof(DateTimePickerEnd))
            {
                EndLimit = dtp.SelectedDateTime;
                e.Handled = true;
            }
        }
    }
}
