using CrossoutLogView.Common;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Models;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrossoutLogView.GUI.Navigation
{
    /// <summary>
    /// Interaction logic for PlayerPage.xaml
    /// </summary>
    public partial class PlayerPage : Page
    {
        private readonly Frame frame;
        private readonly PlayerModel playerModel;
        public PlayerPage(Frame frame, PlayerModel playerViewModel)
        {
            this.frame = frame;
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
            if (playerModel.Object.IsBot) return;
            Logging.WriteLine<PlayerPage>("Navigate to user");
            frame.Navigate(new UserPage(frame, new UserModel(DataProvider.GetUser(playerModel.Object.UserID))));
        }
    }
}
