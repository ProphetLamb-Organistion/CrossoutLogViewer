using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Helpers;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.Statistics;

using MahApps.Metro.Controls;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// Interaction logic for UserGamesControl.xaml
    /// </summary>
    public partial class UserGamesControl : ILogging
    {
        public event OpenModelViewerEventHandler OpenViewModel;

        public UserGamesControl()
        {
            InitializeComponent();
            var scroller = (Content as Grid).FindChild<ScrollableHeaderedControl>();
            var header = scroller.HeaderContent as Grid;
            DataGridGames = scroller.Content as PlayerGamesDataGrid;
            GameListFilter = header.FindChild<GameListFilter>();
            GamesChart = header.FindChild<Expander>().Content as PlayerGamesChart;
            GamesChart.OpenViewModel += (s, e) => OpenViewModel?.Invoke(s, e);
        }

        public PlayerGamesDataGrid DataGridGames { get; }

        public GameListFilter GameListFilter { get; }

        public PlayerGamesChart GamesChart { get; }


        /// <summary>
        /// Gets or sets the <see cref="UserModel"/> used to generate the content of the <see cref="UserGamesControl"/>.
        /// </summary>
        public UserModel User { get => GetValue(UserProperty) as UserModel; set => SetValue(UserProperty, value); }
        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserModel), typeof(UserGamesControl), new PropertyMetadata(OnUserPropertyChanged));

        private static void OnUserPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is UserGamesControl cntr && e.NewValue is UserModel newValue)
            {
                if (newValue.Participations != null)
                    newValue.Participations.Sort(new PlayerGameModelStartTimeDescending());
                cntr.DataContext = newValue;
                cntr.RefreshGamesFilter();
            }
        }

        private void RefreshGamesFilter(object sender, ValueChangedEventArgs<GameFilter> e) => RefreshGamesFilter();
        private void RefreshGamesFilter()
        {
            if (User != null)
            {
                User.FilterParticipations = GameListFilter.Filter;
                GamesChart.ItemsSource = User.ParticipationsFiltered;
                DataGridGames.ItemsSource = User.ParticipationsFiltered;
            }
        }

        private void OpenUsersClick(object sender, RoutedEventArgs e)
        {
            DataGridGames.OpenAllGamesUsers();
        }

        private void MultiFlagComboBox_SelectedValueChanged(object sender, RoutedPropertyChangedEventArgs<NamedEnum> e)
        {
            GamesChart.Dimensions = (Dimensions)e.NewValue.Value;
        }

        private void UserGameControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshGamesFilter();
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
