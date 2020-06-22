using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.Models
{
    public sealed class GameModel : CollectionViewModelBase
    {
        public GameModel()
        {
            Game = new Game();
        }

        public GameModel(Game game)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));
            UpdateCollectionsSafe();
        }

        protected override void UpdateCollections()
        {
            if (Game == null) return;
            Players = Game.Players.Select(x => new PlayerModel(this, x)).ToList();
            Weapons = Game.Weapons.Select(x => new WeaponModel(this, x)).ToList();
            OnPropertyChanged(nameof(Rounds));
        }

        public Game Game { get; }

        private PlayerModel _MVP = null;
        public PlayerModel MVP
        {
            get
            {
                if (_MVP == null && Game.MVP != -1)
                {
                    Set(ref _MVP, Players.FirstOrDefault(x => x.Player.PlayerIndex == Game.MVP));
                }
                return _MVP;
            }
        }

        public string MVPName => MVP == null ? String.Empty : App.GetControlResource("Game_Mvp") + MVP.Player.Name;

        private PlayerModel _RedMVP = null;
        public PlayerModel RedMVP
        {
            get
            {
                if (_RedMVP == null && Game.RedMVP != -1)
                {
                    Set(ref _RedMVP, Players.FirstOrDefault(x => x.Player.PlayerIndex == Game.RedMVP));
                }
                return _RedMVP;
            }
        }

        public string RedMVPName => RedMVP == null ? String.Empty : App.GetControlResource("Game_RedMvp") + RedMVP.Player.Name;

        public string Team1String => Game.MVP == -1 ? App.GetControlResource("Game_Team1") : App.GetControlResource("Game_TeamWin");

        public string Team2String => Game.MVP == -1 ? App.GetControlResource("Game_Team2") : App.GetControlResource("Game_TeamLoose");

        public Visibility MVPVisible => Game.MVP != -1 ? Visibility.Visible : Visibility.Hidden;

        public Visibility RedMVPVisible => Game.RedMVP != -1 ? Visibility.Visible : Visibility.Hidden;

        public Visibility UnfinishedVisible => Game.MVP == -1 ? Visibility.Visible : Visibility.Hidden;

        public string Duration => App.GetControlResource("Game_Duration") + TimeSpanStringFactory(Start - End);

        private List<PlayerModel> _players;
        public List<PlayerModel> Players { get => _players; private set => Set(ref _players, value); }

        private IEnumerable<WeaponModel> _weapons;
        public IEnumerable<WeaponModel> Weapons { get => _weapons; private set => Set(ref _weapons, value); }

        public DateTime Start => Game.Start;

        public DateTime End => Game.End;

        public GameMode Mode => Game.Mode;

        public string Mission => Game.Mission;

        public Map Map => Game.Map;

        public byte WinningTeam => Game.WinningTeam;

        public List<Round> Rounds => Game.Rounds;

        public bool Won => _players.FirstOrDefault(x => x.UserID == Settings.Current.MyUserID).Won;
        public bool Unfinished => Game.MVP == -1;
    }
}
