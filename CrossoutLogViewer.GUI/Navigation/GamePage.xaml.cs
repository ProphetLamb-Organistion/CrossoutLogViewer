using CrossoutLogView.Common;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrossoutLogView.GUI.Navigation
{
    /// <summary>
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        private bool isRoundKillerOver = false, isRoundVictimOver = false, isRoundAssistantOver = false;
        private readonly Frame frame;
        private readonly GameModel gameModel;
        public GamePage(Frame frame, GameModel gameViewModel)
        {
            this.frame = frame;
            Database.Data.DataProvider.CompleteGame(gameViewModel.Object);
            gameViewModel.UpdateCollections();
            InitializeComponent();
            DataContext = gameModel = gameViewModel;
            TreeViewRounds.ItemsSource = gameViewModel.Rounds.Select(x => new RoundModel(gameViewModel, x));
            gameViewModel.Players.Sort(new PlayerModelScoreDescending());
            ListBoxWon.ItemsSource = gameViewModel.Players.Where(x => gameViewModel.WinningTeam == x.Object.Team);
            ListBoxLost.ItemsSource = gameViewModel.Players.Where(x => gameViewModel.WinningTeam != x.Object.Team);
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
                Logging.WriteLine<GamePage>("Navigate to player.");
                frame.Navigate(new PlayerPage(frame, player));
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
                frame.Navigate(new PlayerPage(frame, targetPlayer));
            }
        }

        private void OpenMVP(object sender, MouseButtonEventArgs e)
        {
            if (gameModel.MVP != null)
            {
                Logging.WriteLine<GamePage>("Navigate to player.");
                frame.Navigate(new PlayerPage(frame, gameModel.MVP));
                e.Handled = true;
            }
        }
        private void OpenRedMVP(object sender, MouseButtonEventArgs e)
        {
            if (gameModel.RedMVP != null)
            {
                Logging.WriteLine<GamePage>("Navigate to player.");
                frame.Navigate(new PlayerPage(frame, gameModel.RedMVP));
                e.Handled = true;
            }
        }

        private PlayerModel PlayerByName(string name)
        {
            if (String.IsNullOrEmpty(name)) return null;
            return gameModel.Players.FirstOrDefault(x => Common.Strings.NameEquals(x.Name, name));
        }
    }
}
