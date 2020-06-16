using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Helpers;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.GUI.WindowsAuxilary;

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CrossoutLogView.GUI.Navigation
{
    /// <summary>
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GamePage : ILogging
    {
        private bool isRoundKillerOver = false, isRoundVictimOver = false, isRoundAssistantOver = false;
        private readonly NavigationWindow nav;
        private readonly GameModel gameModel;
        public GamePage(NavigationWindow nav, GameModel gameViewModel)
        {
            if (gameViewModel is null)
                throw new ArgumentNullException(nameof(gameViewModel));
            this.nav = nav ?? throw new ArgumentNullException(nameof(nav));
            DataProvider.CompleteGame(gameViewModel.Game);
            gameViewModel.UpdateCollectionsSafe();
            InitializeComponent();
            DataContext = gameModel = gameViewModel;
            TreeViewRounds.ItemsSource = gameViewModel.Rounds.Select(x => new RoundModel(gameViewModel, x));
            gameViewModel.Players.Sort(new PlayerModelScoreDescending());
            ListBoxWon.ItemsSource = gameViewModel.Players.Where(x => gameViewModel.WinningTeam == x.Player.Team);
            ListBoxLost.ItemsSource = gameViewModel.Players.Where(x => gameViewModel.WinningTeam != x.Player.Team);
            var uri = ImageHelper.GetMapImageUri(gameModel.Map.Name);
            if (uri != null)
                MapImage.Source = new BitmapImage(uri);
        }

        private void RoundKillEnter(object sender, MouseEventArgs e) => isRoundKillerOver = true;
        private void RoundKillLeave(object sender, MouseEventArgs e) => isRoundKillerOver = false;
        private void RoundVictimEnter(object sender, MouseEventArgs e) => isRoundVictimOver = true;
        private void RoundVictimLeave(object sender, MouseEventArgs e) => isRoundVictimOver = false;
        private void RoundAssistantEnter(object sender, MouseEventArgs e) => isRoundAssistantOver = true;
        private void RoundAssistantLeave(object sender, MouseEventArgs e) => isRoundAssistantOver = false;

        private void ScoreOpenPlayer(object sender, MouseButtonEventArgs e)
        {
            var clickedItem = DataGridHelper.GetSourceElement<ListBoxItem>(e.OriginalSource);
            if (clickedItem != null && clickedItem.DataContext is PlayerModel player)
            {
                nav.Navigate(new PlayerPage(nav, player));
                e.Handled = true;
            }
        }

        private void RoundOpenPlayer(object sender, MouseButtonEventArgs e)
        {
            PlayerModel targetPlayer = null;
            if (TreeViewRounds.SelectedItem is KillModel kv)
            {
                if (isRoundKillerOver) targetPlayer = PlayerByName(kv.Killer);
                else if (isRoundVictimOver) targetPlayer = PlayerByName(kv.Victim);
                e.Handled = true;
            }
            else if (TreeViewRounds.SelectedItem is AssistModel av)
            {
                if (isRoundAssistantOver) targetPlayer = PlayerByName(av.Assistant);
                e.Handled = true;
            }
            if (targetPlayer != null)
            {
                nav.Navigate(new PlayerPage(nav, targetPlayer));
            }
        }

        private void OpenMVP(object sender, MouseButtonEventArgs e)
        {
            if (gameModel.MVP != null)
            {
                nav.Navigate(new PlayerPage(nav, gameModel.MVP));
                e.Handled = true;
            }
        }

        private void OpenRedMVP(object sender, MouseButtonEventArgs e)
        {
            if (gameModel.RedMVP != null)
            {
                nav.Navigate(new PlayerPage(nav, gameModel.RedMVP));
                e.Handled = true;
            }
        }

        private PlayerModel PlayerByName(string name)
        {
            if (String.IsNullOrEmpty(name)) return null;
            return gameModel.Players.FirstOrDefault(x => Common.Strings.NameEquals(x.Name, name));
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
