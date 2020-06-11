using CrossoutLogView.Common;
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
    public partial class GameListFilter
    {
        public event GameFilterChangedEventHandler FilterChanged;

        public GameListFilter()
        {
            InitializeComponent();
            ComboBoxGameMode.ItemsSource = Enum.GetValues(typeof(GameMode)).Cast<GameMode>();
            GameMode = GameMode.All;
        }

        /// <summary>
        /// Gets or sets the instance of <see cref="GameFilter"/> defining the setting of this <see cref="UserControl"/>.
        /// </summary>
        public GameFilter Filter
        {
            get => ObjToGameFilter(GetValue(FilterProperty));
            set => SetValue(FilterProperty, value);
        }
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(GameFilter), typeof(GameListFilter), new PropertyMetadata(OnFilterPropertyChanged));

        /// <summary>
        /// Gets or sets the lower bound of <see cref="DateTime"/> for <see cref="Game"/>s included in the filter.
        /// </summary>
        public DateTime? StartLimit
        {
            get => GetValue(StartLimitProperty) as DateTime?;
            set => SetValue(StartLimitProperty, value);
        }
        public static readonly DependencyProperty StartLimitProperty = DependencyProperty.Register(nameof(StartLimit), typeof(DateTime?), typeof(GameListFilter), new PropertyMetadata(OnStartLimitPropertyChanged));

        /// <summary>
        /// Gets or sets the upper bound of <see cref="DateTime"/> for <see cref="Game"/>s included in the filter.
        /// </summary>
        public DateTime? EndLimit
        {
            get => GetValue(EndLimitProperty) as DateTime?;
            set => SetValue(EndLimitProperty, value);
        }
        public static readonly DependencyProperty EndLimitProperty = DependencyProperty.Register(nameof(EndLimit), typeof(DateTime?), typeof(GameListFilter), new PropertyMetadata(OnEndLimitPropertyChanged));

        /// <summary>
        /// Gets or sets the enum specifing what kind of games are included in the filter.
        /// </summary>
        public GameMode GameMode
        {
            get => ObjToGameMode(GetValue(GameModeProperty));
            set => SetValue(GameModeProperty, value);
        }
        public static readonly DependencyProperty GameModeProperty = DependencyProperty.Register(nameof(GameMode), typeof(GameMode), typeof(GameListFilter), new PropertyMetadata(OnGameModePropertyChanged));

        private static void OnFilterPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is GameListFilter gl)
            {
                var newGameFilter = ObjToGameFilter(e.NewValue);
                gl.StartLimit = newGameFilter.StartLimit == default ? null : (newGameFilter.StartLimit as DateTime?);
                gl.EndLimit = newGameFilter.EndLimit == default ? null : (newGameFilter.EndLimit as DateTime?);
                gl.GameMode = newGameFilter.GameModes;
                gl.FilterChanged?.Invoke(gl, new GameFilterChangedEventArgs(ObjToGameFilter(e.OldValue), newGameFilter));
            }
        }

        private static void OnStartLimitPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && sender is GameListFilter gl)
            {
                gl.Filter = new GameFilter(
                    gl.Filter.GameModes,
                    (e.NewValue as DateTime?) ?? DateTime.MinValue,
                    gl.Filter.EndLimit);
            }
        }

        private static void OnEndLimitPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && sender is GameListFilter gl)
            {
                gl.Filter = new GameFilter(
                    gl.Filter.GameModes,
                    gl.Filter.StartLimit,
                    (e.NewValue as DateTime?) ?? DateTime.MinValue);
            }
        }

        private static void OnGameModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && sender is GameListFilter gl)
            {
                gl.Filter = new GameFilter(
                    ObjToGameMode(e.NewValue),
                    gl.Filter.StartLimit,
                    gl.Filter.EndLimit);
            }
        }

        private void ClearFilter(object sender, RoutedEventArgs e) => ClearFilter();
        public void ClearFilter()
        {
            SetValue(StartLimitProperty, null);
            SetValue(EndLimitProperty, null);
            SetValue(GameModeProperty, GameMode.All);
        }

        private void SetFilterNoonClick(object sender, RoutedEventArgs e) => SetFilterNoon();
        public void SetFilterNoon()
        {
            if (StartLimit.HasValue)
            {
                var start = StartLimit.Value;
                StartLimit = new DateTime(start.Year, start.Month, start.Day).AddHours(12);
            }
            else
            {
                StartLimit = DateTime.Today.AddHours(12.0);
            }
            EndLimit = StartLimit.Value.AddHours(5.0);
        }

        private void SetFilterAfternoonClick(object sender, RoutedEventArgs e) => SetFilterAfternoon();
        public void SetFilterAfternoon() 
        {
            if (StartLimit.HasValue)
            {
                var start = StartLimit.Value;
                StartLimit = new DateTime(start.Year, start.Month, start.Day).AddHours(18);
            }
            else
            {
                StartLimit = DateTime.Today.AddHours(18.0);
            }
            EndLimit = StartLimit.Value.AddHours(5.0);
        }

        private void SetFilterNightClick(object sender, RoutedEventArgs e) => SetFilterNight();
        public void SetFilterNight()
        {
            if (StartLimit.HasValue)
            {
                var start = StartLimit.Value;
                StartLimit = new DateTime(start.Year, start.Month, start.Day);
            }
            else
            {
                StartLimit = DateTime.Today;
            }
            EndLimit = StartLimit.Value.AddHours(5.0);
        }

        private void SetFilterAllDayClick(object sender, RoutedEventArgs e) => SetFilterAllDay();
        public void SetFilterAllDay()
        {
            if (StartLimit.HasValue)
            {
                var start = StartLimit.Value;
                StartLimit = new DateTime(start.Year, start.Month, start.Day);
            }
            else
            {
                StartLimit = DateTime.Today;
            }
            EndLimit = StartLimit.Value.AddDays(1.0).AddSeconds(-1.0);
        }

        private void SetFilterWeekClick(object sender, RoutedEventArgs e) => SetFilterWeek();
        public void SetFilterWeek()
        {
            StartLimit = DateTime.Now.StartOfWeek();
            EndLimit = StartLimit.Value.AddDays(7.0).AddSeconds(-1.0);
        }

        private void SetFilterMonthClick(object sender, RoutedEventArgs e) => SetFilterMonth();
        public void SetFilterMonth()
        {
            StartLimit = DateTime.Now.StartOfMonth();
            EndLimit = DateTime.Now.EndOfMonth();
        }

        private void OpenCustomFilterPopupClick(object sender, RoutedEventArgs e)
        {
            PopupCustomTimeFilter.IsOpen = true;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            if (StartLimit.HasValue)
            {
                StartLimit = DateTime.Now.Date.AddDays(e.NewValue).Add(StartLimit.Value.TimeOfDay);
            }
            else
            {
                StartLimit = DateTime.Now.Date;
            }
            if (EndLimit.HasValue)
            {
                EndLimit = DateTime.Now.Date.AddDays(e.NewValue).Add(EndLimit.Value.TimeOfDay);
            }
            else
            {
                EndLimit = DateTime.Now.Date.AddDays(1.0).AddSeconds(-1.0);
            }
        }

        private static GameFilter ObjToGameFilter(object obj)
        {
            return (GameFilter)(obj ?? new GameFilter(GameMode.All));
        }
        private static GameMode ObjToGameMode(object obj)
        {
            return (GameMode)(obj ?? GameMode.All);
        }
    }
}
