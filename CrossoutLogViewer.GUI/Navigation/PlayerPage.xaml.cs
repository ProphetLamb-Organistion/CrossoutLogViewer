using CrossoutLogView.Common;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.GUI.WindowsAuxilary;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrossoutLogView.GUI.Navigation
{
    /// <summary>
    /// Interaction logic for PlayerPage.xaml
    /// </summary>
    public partial class PlayerPage : ILogging
    {
        private readonly NavigationWindow nav;
        private readonly PlayerModel playerModel;
        public PlayerPage(NavigationWindow nav, PlayerModel playerViewModel)
        {
            this.nav = nav;
            InitializeComponent();
            DataContext = playerModel = playerViewModel;
            DataGridWeapons.ItemsSource = playerViewModel.Weapons;
            ItemsControlStripes.ItemsSource = playerViewModel.Stripes;
            foreach (var column in DataGridWeapons.Columns)
            {
                column.CanUserSort = true;
                column.IsReadOnly = true;
            }
        }

        private void OpenUser(object sender, MouseButtonEventArgs e)
        {
            if (playerModel.Player.IsBot) return;
            nav.Navigate(new UserPage(nav, new UserModel(DataProvider.GetUser(playerModel.Player.UserID))));
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
