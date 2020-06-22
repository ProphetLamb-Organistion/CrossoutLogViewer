using CrossoutLogView.Common;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Helpers;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace CrossoutLogView.GUI.Models
{
    public sealed class UserModel : StatDisplayViewModeBase
    {
        private int _gamesWon, _gamesLost, _gamesUnfinished;
        private double _winrate, _score, _kills, _assists, _deaths, _armorDamageDealt, _criticalDamageDealt, _armorDamageTaken, _criticalDamageTaken;
        private List<WeaponModel> _weapons;
        private ObservableCollection<PlayerGameModel> _participations;
        private ObservableCollection<PlayerGameModel> _participationsFiltered;
        private List<StripeModel> _stripes;

        public UserModel() : base()
        {
            User = new User();
            Participations = new ObservableCollection<PlayerGameModel>();
            Weapons = new List<WeaponModel>();
            Stripes = new List<StripeModel>();
        }

        public UserModel(User obj) : base()
        {
            User = obj ?? throw new ArgumentNullException(nameof(obj));
            var participations = new PlayerGameModel[User.Participations.Count];
            for (int i = 0; i < participations.Length; i++)
            {
                var gv = new GameModel(User.Participations[i]);
                var pv = new PlayerModel(gv, User.Participations[i].Players.FirstOrDefault(x => x.UserID == User.UserID));
                participations[i] = new PlayerGameModel(gv, pv);
            }
            Participations = new ObservableCollection<PlayerGameModel>(participations);
            ParticipationsFiltered = new ObservableCollection<PlayerGameModel>(participations);
            UpdateCollectionsSafe();
            UpdateProperties();
        }

        public User User { get; }

        private GameFilter _filterParticipations = Controls.GameListFilter.DefaultFilter;
        public GameFilter FilterParticipations
        {
            get => _filterParticipations;
            set
            {
                Set(ref _filterParticipations, value);
                ParticipationsFiltered.Filter(Participations, value.Filter);
                UpdateProperties();
            }
        }

        public ObservableCollection<PlayerGameModel> Participations { get => _participations; private set => Set(ref _participations, value); }
        public ObservableCollection<PlayerGameModel> ParticipationsFiltered { get => _participationsFiltered; private set => Set(ref _participationsFiltered, value); }
        public List<WeaponModel> Weapons { get => _weapons; private set => Set(ref _weapons, value); }
        public List<StripeModel> Stripes { get => _stripes; private set => Set(ref _stripes, value); }
        public int ParticipationCount => User.Participations.Count;
        public string Name => User.Name;
        public int UserID { get => User.UserID; }
        public int GamesWon { get => _gamesWon; set => Set(ref _gamesWon, value); }
        public int GamesLost { get => _gamesLost; set => Set(ref _gamesLost, value); }
        public int GamesUnfinished { get => _gamesUnfinished; set => Set(ref _gamesUnfinished, value); }
        public double Winrate { get => _winrate; set => Set(ref _winrate, value); }
        public double Score { get => _score; set => Set(ref _score, value); }
        public double Kills { get => _kills; set => Set(ref _kills, value); }
        public double Assists { get => _assists; set => Set(ref _assists, value); }
        public double Deaths { get => _deaths; set => Set(ref _deaths, value); }
        public double ArmorDamageDealt { get => _armorDamageDealt; set => Set(ref _armorDamageDealt, value); }
        public double CriticalDamageDealt { get => _criticalDamageDealt; set => Set(ref _criticalDamageDealt, value); }
        public double ArmorDamageTaken { get => _armorDamageTaken; set => Set(ref _armorDamageTaken, value); }
        public double CriticalDamageTaken { get => _criticalDamageTaken; set => Set(ref _criticalDamageTaken, value); }
        public double TotalDamageDealt { get => _armorDamageDealt + _criticalDamageDealt; }
        public double TotalDamageTaken { get => _armorDamageTaken + _criticalDamageTaken; }

        protected override void UpdateCollections()
        {
            OnPropertyChanged(nameof(Participations));
            User.Weapons.Sort((x, y) => (y.ArmorDamage + y.CriticalDamage).CompareTo(x.ArmorDamage + x.CriticalDamage));
            Weapons = User.Weapons.Select(x => new WeaponModel(this, x)).ToList();
            User.Stripes.Sort((x, y) => y.Ammount.CompareTo(x.Ammount));
            Stripes = User.Stripes.Select(x => new StripeModel(this, x)).ToList();
        }

        public override void UpdateProperties()
        {
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

    public sealed class UserModelParticipationCountDescending : IComparer<UserModel>
    {
        int IComparer<UserModel>.Compare(UserModel x, UserModel y) => y.ParticipationCount.CompareTo(x.ParticipationCount);
    }
}
