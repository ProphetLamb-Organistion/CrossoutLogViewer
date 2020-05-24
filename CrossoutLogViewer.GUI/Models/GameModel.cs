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
    public sealed class GameModel : ViewModelBase
    {
        public GameModel()
        {
            Object = new Game();
        }

        public GameModel(Game obj)
        {
            Object = obj;
            UpdateCollections();
        }

        public override void UpdateCollections()
        {
            Players = Object.Players.Select(x => new PlayerModel(this, x)).ToList();
            Weapons = Object.Weapons.Select(x => new WeaponModel(this, x)).ToList();
            OnPropertyChanged(nameof(Rounds));
        }

        public Game Object { get; }

        private PlayerModel _MVP = null;
        public PlayerModel MVP
        {
            get
            {
                if (_MVP == null && Object.MVP != -1)
                {
                    Set(ref _MVP, Players.First(x => x.Object.PlayerIndex == Object.MVP));
                }
                return _MVP;
            }
        }

        public string MVPName => MVP == null ? String.Empty : "MVP: " + MVP.Object.Name;

        public PlayerModel _RedMVP = null;
        public PlayerModel RedMVP
        {
            get
            {
                if (_RedMVP == null && Object.RedMVP != -1)
                {
                    Set(ref _RedMVP, Players.First(x => x.Object.PlayerIndex == Object.RedMVP));
                }
                return _RedMVP;
            }
        }

        public string RedMVPName => RedMVP == null ? String.Empty : "Unyielding: " + RedMVP.Object.Name;

        public string Team1String => Object.MVP == -1 ? "Team 1" : "Winning Team";

        public string Team2String => Object.MVP == -1 ? "Team 2" : "Loosing Team";

        public Visibility MVPVisible => Object.MVP != -1 ? Visibility.Visible : Visibility.Hidden;

        public Visibility RedMVPVisible => Object.RedMVP != -1 ? Visibility.Visible : Visibility.Hidden;

        public Visibility UnfinishedVisible => Object.MVP == -1 ? Visibility.Visible : Visibility.Hidden;

        public string Title => String.Concat(Object.Start.ToString("t"), " - ", Object.End.ToString("t"),
                CenterDotSeparator, Object.Map.Name, CenterDotSeparator, Object.Mission, CenterDotSeparator, Object.Mode);

        public string Duration => "Duration: " + (Start - End).ToString(@"mm\:ss");

        private List<PlayerModel> _players;
        public List<PlayerModel> Players { get => _players; private set => Set(ref _players, value); }

        private IEnumerable<WeaponModel> _weapons;
        public IEnumerable<WeaponModel> Weapons { get => _weapons; private set => Set(ref _weapons, value); }

        public DateTime Start => Object.Start;

        public DateTime End => Object.End;

        public GameMode Mode => Object.Mode;

        public string Mission => Object.Mission;

        public Map Map => Object.Map;

        public byte WinningTeam => Object.WinningTeam;

        public List<Round> Rounds => Object.Rounds;

        public bool Won => _players.FirstOrDefault(x => x.UserID == Settings.Current.MyUserID).Won;
        public bool Unfinished => Object.MVP == -1;

        public Brush Background => Won ? App.Current.Resources["TeamWon"] as Brush : !Unfinished ? App.Current.Resources["TeamLost"] as Brush : default;
    }
}
