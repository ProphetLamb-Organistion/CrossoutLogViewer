using CrossoutLogView.Common;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
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
    public partial class UserGamesControl
    {
        private UserModel _user;

        public UserGamesControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the <see cref="UserModel"/> used to generate the content of the <see cref="UserGamesControl"/>.
        /// </summary>
        public UserModel User
        {
            get => _user;
            set
            {
                if (value == null || ReferenceEquals(_user, value)) return;
                value.Participations.Sort(new PlayerGameCompositeModelStartTimeDescending());
                UserOverview.DataContext = _user = value;
                RefreshGamesFilter();
            }
        }

        private void RefreshGamesFilter(object sender = null, GameFilterChangedEventArgs e = null)
        {
            _user.FilterParticipations = GameListFilter.Filter;
            DataGridGames.ItemsSource = _user.ParticipationsFiltered;
        }

        private void OpenUsersClick(object sender, RoutedEventArgs e)
        {
            DataGridGames.OpenAllGamesUsers();
        }
    }
}
