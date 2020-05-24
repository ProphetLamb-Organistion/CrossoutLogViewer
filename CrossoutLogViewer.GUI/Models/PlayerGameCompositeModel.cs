using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;

namespace CrossoutLogView.GUI.Models
{
    public sealed class PlayerGameCompositeModel : ViewModelBase
    {
        public PlayerGameCompositeModel()
        {
            Game = new GameModel();
            Player = new PlayerModel();
            Map = String.Empty;
        }

        public PlayerGameCompositeModel(GameModel game, PlayerModel player)
        {
            Game = game;
            Player = player;
            Map = DisplayStringFactory.MapName(Game.Object.Map.Name);
        }

        public override void UpdateCollections()
        {
            Game.UpdateCollections();
            Player.UpdateCollections();
        }

        public GameModel Game { get; }

        public PlayerModel Player { get; }

        public bool Unfinished => Game.Object.MVP == -1;

        public bool Won => Game.Object.WinningTeam == Player.Object.Team;

        public DateTime StartTime => Game.Object.Start;

        public string Map { get; }

        public GameMode Mode => Game.Object.Mode;

        public int Score => (int)Math.Round(Player.Object.Score);

        public int Kills => Player.Object.Kills;

        public int Assists => Player.Object.Assists;

        public int Deaths => Player.Object.Deaths;

        public double ArmorDamageDealt => Player.Object.ArmorDamageDealt;

        public double CriticalDamageDealt => Player.Object.CriticalDamageDealt;

        public double ArmorDamageTaken => Player.Object.ArmorDamageTaken;

        public double CriticalDamageTaken => Player.Object.CriticalDamageTaken;

        public double TotalDamageDealt => Player.Object.ArmorDamageDealt + Player.Object.CriticalDamageDealt;

        public double TotalDamageTaken => Player.Object.ArmorDamageTaken + Player.Object.CriticalDamageTaken;

        public Brush Background => Won ? App.Current.Resources["TeamWon"] as Brush : !Unfinished ? App.Current.Resources["TeamLost"] as Brush : default;
    }

    public class PlayerGameCompositeModelScoreDescending : IComparer<PlayerGameCompositeModel>
    {
        int IComparer<PlayerGameCompositeModel>.Compare(PlayerGameCompositeModel x, PlayerGameCompositeModel y) => y.Score.CompareTo(x.Score);
    }

    public class PlayerGameCompositeModelStartTimeDescending : IComparer<PlayerGameCompositeModel>
    {
        int IComparer<PlayerGameCompositeModel>.Compare(PlayerGameCompositeModel x, PlayerGameCompositeModel y) => y.StartTime.CompareTo(x.StartTime);
    }
}
