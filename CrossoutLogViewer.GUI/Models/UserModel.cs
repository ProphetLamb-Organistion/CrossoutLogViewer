using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CrossoutLogView.GUI.Models
{
    public enum DisplayMode { Average, Total }

    public sealed class UserModel : ViewModelBase
    {
        private DisplayMode _statDisplayMode = DisplayMode.Average;

        public UserModel()
        {
            Object = new User();
            Participations = new ObservableCollection<PlayerGameCompositeModel>();
            Weapons = new List<WeaponModel>();
            Stripes = new List<StripeModel>();
        }
        public UserModel(User obj)
        {
            Object = obj;
            var participations = new PlayerGameCompositeModel[Object.Participations.Count];
            for (int i = 0; i < participations.Length; i++)
            {
                var gv = new GameModel(Object.Participations[i]);
                var pv = new PlayerModel(gv, Object.Participations[i].Players.First(x => x.UserID == Object.UserID));
                participations[i] = new PlayerGameCompositeModel(gv, pv);
            }
            ParticipationsFiltered = Participations = new ObservableCollection<PlayerGameCompositeModel>(participations);
            UpdateCollections();
            UpdateProperties();
        }

        public User Object { get; }

        private GameFilter _filterParticipations = new GameFilter(GameMode.All);
        public GameFilter FilterParticipations
        {
            get => _filterParticipations;
            set
            {
                Set(ref _filterParticipations, value);
                ParticipationsFiltered = _participations.Where(x => value.Filter(x));
                UpdateProperties();
            }
        }

        public string Title => String.Concat(Object.Name, " (", Object.UserID, ")");

        public DisplayMode StatDisplayMode
        {
            get => _statDisplayMode;
            set
            {
                Set(ref _statDisplayMode, value);
                UpdateProperties();
            }
        }

        private ObservableCollection<PlayerGameCompositeModel> _participations;
        public ObservableCollection<PlayerGameCompositeModel> Participations { get => _participations; private set => Set(ref _participations, value); }

        private IEnumerable<PlayerGameCompositeModel> _participationsFiltered;
        public IEnumerable<PlayerGameCompositeModel> ParticipationsFiltered { get => _participationsFiltered; private set => Set(ref _participationsFiltered, value); }

        private List<WeaponModel> _weapons;
        public List<WeaponModel> Weapons { get => _weapons; private set => Set(ref _weapons, value); }

        private List<StripeModel> _stripes;
        public List<StripeModel> Stripes { get => _stripes; private set => Set(ref _stripes, value); }

        public int ParticipationCount => Object.Participations.Count;

        public string Name => Object.Name;

        public string GeneralGroup => StatDisplayMode == DisplayMode.Average ? "General (per battle)" : "General";

        public string DamageGroup => StatDisplayMode == DisplayMode.Average ? "Damage Dealt (per battle)" : "Damage Dealt";

        public int UserID { get => Object.UserID; }

        private int _gamesWon;
        public int GamesWon { get => _gamesWon; set => Set(ref _gamesWon, value); }

        private int _gamesLost;
        public int GamesLost { get => _gamesLost; set => Set(ref _gamesLost, value); }

        private int _gamesUnfinished;
        public int GamesUnfinished { get => _gamesUnfinished; set => Set(ref _gamesUnfinished, value); }

        private double _winrate;
        public double Winrate { get => _winrate; set => Set(ref _winrate, value); }

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

        public override void UpdateCollections()
        {
            OnPropertyChanged(nameof(Participations));
            Object.Weapons.Sort((x, y) => (y.ArmorDamage + y.CriticalDamage).CompareTo(x.ArmorDamage + x.CriticalDamage));
            Weapons = Object.Weapons.Select(x => new WeaponModel(this, x)).ToList();
            Object.Stripes.Sort((x, y) => y.Ammount.CompareTo(x.Ammount));
            Stripes = Object.Stripes.Select(x => new StripeModel(this, x)).ToList();
        }

        public void UpdateProperties()
        {
            OnPropertyChanged(nameof(GeneralGroup));
            OnPropertyChanged(nameof(DamageGroup));
            GamesWon = ParticipationsFiltered.Where(x => x.Won).Count();
            GamesLost = ParticipationsFiltered.Where(x => !x.Won && !x.Unfinished).Count();
            var count = ParticipationsFiltered.Count();
            GamesUnfinished = count - GamesWon - GamesLost;
            Winrate = GamesWon == 0 ? 0.0 : GamesLost == 0 ? 1.0 : GamesWon / (double)(GamesWon + GamesLost);
            var mult = StatDisplayMode == DisplayMode.Average ? 1.0 / count : 1.0;
            Score = ParticipationsFiltered.Sum(x => x.Score) * mult;
            Kills = ParticipationsFiltered.Sum(x => x.Kills) * mult;
            Assists = ParticipationsFiltered.Sum(x => x.Assists) * mult;
            Deaths = ParticipationsFiltered.Sum(x => x.Deaths) * mult;
            ArmorDamageDealt = ParticipationsFiltered.Sum(x => x.ArmorDamageDealt) * mult;
            CriticalDamageDealt = ParticipationsFiltered.Sum(x => x.CriticalDamageDealt) * mult;
            OnPropertyChanged(nameof(TotalDamageDealt));
            ArmorDamageTaken = ParticipationsFiltered.Sum(x => x.ArmorDamageTaken) * mult;
            CriticalDamageTaken = ParticipationsFiltered.Sum(x => x.CriticalDamageTaken) * mult;
            OnPropertyChanged(nameof(TotalDamageTaken));
        }
    }

    public class UserModelParticipationCountDescending : IComparer<UserModel>
    {
        int IComparer<UserModel>.Compare(UserModel x, UserModel y) => y.ParticipationCount.CompareTo(x.ParticipationCount);
    }
}
