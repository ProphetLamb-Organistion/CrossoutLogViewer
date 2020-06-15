using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossoutLogView.GUI.Models
{
    public class MapModel : CollectionViewModel
    {
        public MapModel(GameMap map)
        {
            GameMap = map ?? throw new ArgumentNullException(nameof(map));
            UpdateCollectionsSafe();
            UpdateProperties();
            Name = DisplayStringFactory.MapName(map.Map.Name);
        }

        private DisplayMode _statDisplayMode;
        public DisplayMode StatDisplayMode
        {
            get => _statDisplayMode;
            set
            {
                Set(ref _statDisplayMode, value);
                UpdateProperties();
            }
        }

        public string GeneralGroup => StatDisplayMode == DisplayMode.Average ? "General (per battle)" : "General";

        public string DamageGroup => StatDisplayMode == DisplayMode.Average ? "Damage Dealt (per battle)" : "Damage Dealt";

        public GameMap GameMap { get; }

        public string Name { get; }

        private int _gamesPlayed;
        public int GamesPlayed { get => _gamesPlayed; set => Set(ref _gamesPlayed, value); }

        public IEnumerable<PlayerGameCompositeModel> Games { get; private set; }

        private PlayerGameCompositeModel _selectedItem;
        public PlayerGameCompositeModel SelectedItem { get => _selectedItem; set => Set(ref _selectedItem, value); }

        private double _winrate;
        public double Winrate { get => _winrate; set => Set(ref _winrate, value); }

        private int _gamesWon;
        public int GamesWon { get => _gamesWon; set => Set(ref _gamesWon, value); }

        private int _gamesLost;
        public int GamesLost { get => _gamesLost; set => Set(ref _gamesLost, value); }

        private int _gamesUnfinished;
        public int GamesUnfinished { get => _gamesUnfinished; set => Set(ref _gamesUnfinished, value); }

        private double _score;
        public double Score { get => _score; set => Set(ref _score, value); }

        private double _kills;
        public double Kills { get => _kills; set => Set(ref _kills, value); }

        private double _assists;
        public double Assists { get => _assists; set => Set(ref _assists, value); }

        private double _deaths;
        public double Deaths { get => _deaths; set => Set(ref _deaths, value); }

        private double _armorDamageDealt;
        public double ArmorDamageDealt { get => _armorDamageDealt; set => Set(ref _armorDamageDealt, value); }

        private double _criticalDamageDealt;
        public double CriticalDamageDealt { get => _criticalDamageDealt; set => Set(ref _criticalDamageDealt, value); }

        private double _armorDamageTaken;
        public double ArmorDamageTaken { get => _armorDamageTaken; set => Set(ref _armorDamageTaken, value); }

        private double _criticalDamageTaken;
        public double CriticalDamageTaken { get => _criticalDamageTaken; set => Set(ref _criticalDamageTaken, value); }

        public double TotalDamageDealt { get => _armorDamageDealt + _criticalDamageDealt; }

        public double TotalDamageTaken { get => _armorDamageTaken + _criticalDamageTaken; }

        protected override void UpdateCollections()
        {
            var games = new List<PlayerGameCompositeModel>();
            foreach (var g in GameMap.Games)
            {
                var game = new GameModel(g);
                var player = game.Players.FirstOrDefault(x => x.Player.UserID == Settings.Current.MyUserID);
                if (player != null)
                    games.Add(new PlayerGameCompositeModel(game, player));
            }
            Games = games;
        }

        public void UpdateProperties()
        {
            GamesPlayed = Games.Count();
            GamesWon = Games.Where(x => x.Won).Count();
            GamesLost = Games.Where(x => !x.Won && !x.Unfinished).Count();
            Winrate = GamesWon == 0 ? 0 : GamesLost == 0 ? 1 : GamesWon / (double)(GamesWon + GamesLost);
            GamesUnfinished = GamesPlayed - GamesWon - GamesLost;
            var mult = StatDisplayMode == DisplayMode.Average ? 1.0 / GamesPlayed : 1.0;
            Score = Games.Sum(x => x.Score) * mult;
            Kills = Games.Sum(x => x.Kills) * mult;
            Assists = Games.Sum(x => x.Assists) * mult;
            Deaths = Games.Sum(x => x.Kills) * mult;
            ArmorDamageDealt = Games.Sum(x => x.ArmorDamageDealt) * mult;
            CriticalDamageDealt = Games.Sum(x => x.ArmorDamageDealt) * mult;
            OnPropertyChanged(nameof(TotalDamageDealt));
            ArmorDamageTaken = Games.Sum(x => x.ArmorDamageDealt) * mult;
            CriticalDamageTaken = Games.Sum(x => x.ArmorDamageDealt) * mult;
            OnPropertyChanged(nameof(TotalDamageTaken));
        }
    }
    public sealed class MapModelGamesPlayedDecending : IComparer<MapModel>
    {
        int IComparer<MapModel>.Compare(MapModel x, MapModel y) => y.GamesPlayed.CompareTo(x.GamesPlayed);
    }
}
