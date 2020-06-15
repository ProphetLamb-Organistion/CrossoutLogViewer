using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;

namespace CrossoutLogView.GUI.Models
{
    public sealed class PlayerGameCompositeModel : CollectionViewModel
    {
        public PlayerGameCompositeModel()
        {
            Game = new GameModel();
            Player = new PlayerModel();
            Map = String.Empty;
        }

        public PlayerGameCompositeModel(GameModel game, PlayerModel player)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Map = DisplayStringFactory.MapName(Game.Game.Map.Name);
        }

        protected override void UpdateCollections()
        {
            Game.UpdateCollectionsSafe();
            Player.UpdateCollectionsSafe();
        }

        public GameModel Game { get; }

        public PlayerModel Player { get; }

        public bool Unfinished => Game.Game.MVP == -1;

        public bool Won => Game.Game.WinningTeam == Player.Player.Team;

        public DateTime StartTime => Game.Game.Start;

        public string Map { get; }

        public GameMode Mode => Game.Game.Mode;

        public int Score => (int)Math.Round(Player.Player.Score);

        public int Kills => Player.Player.Kills;

        public int Assists => Player.Player.Assists;

        public int Deaths => Player.Player.Deaths;

        public double ArmorDamageDealt => Player.Player.ArmorDamageDealt;

        public double CriticalDamageDealt => Player.Player.CriticalDamageDealt;

        public double ArmorDamageTaken => Player.Player.ArmorDamageTaken;

        public double CriticalDamageTaken => Player.Player.CriticalDamageTaken;

        public double TotalDamageDealt => Player.Player.ArmorDamageDealt + Player.Player.CriticalDamageDealt;

        public double TotalDamageTaken => Player.Player.ArmorDamageTaken + Player.Player.CriticalDamageTaken;

        public Brush Background => Won ? App.Current.Resources["TeamWon"] as Brush : !Unfinished ? App.Current.Resources["TeamLost"] as Brush : default;
    }

    public sealed class PlayerGameCompositeModelScoreDescending : IComparer<PlayerGameCompositeModel>
    {
        int IComparer<PlayerGameCompositeModel>.Compare(PlayerGameCompositeModel x, PlayerGameCompositeModel y) => y.Score.CompareTo(x.Score);
    }

    public sealed class PlayerGameCompositeModelStartTimeDescending : IComparer<PlayerGameCompositeModel>
    {
        int IComparer<PlayerGameCompositeModel>.Compare(PlayerGameCompositeModel x, PlayerGameCompositeModel y) => y.StartTime.CompareTo(x.StartTime);
    }
}
