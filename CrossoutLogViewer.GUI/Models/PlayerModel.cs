using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.Models
{
    public sealed class PlayerModel : ViewModelBase
    {
        public PlayerModel()
        {
            Object = new Player();
            Parent = new GameModel();
            Weapons = new List<WeaponModel>();
            Stripes = new List<StripeModel>();
        }

        public PlayerModel(GameModel parent, Player obj)
        {
            Parent = parent;
            Object = obj;
            UpdateCollections();
        }

        public override void UpdateCollections()
        {
            Object.Weapons.Sort((x, y) => (y.ArmorDamage + y.CriticalDamage).CompareTo(x.ArmorDamage + x.CriticalDamage));
            Weapons = Object.Weapons.Select(x => new WeaponModel(this, x));
            Object.Stripes.Sort((x, y) => y.Ammount.CompareTo(x.Ammount));
            Stripes = Object.Stripes.Select(x => new StripeModel(this, x));
        }

        public bool Won { get => Parent.WinningTeam == Object.Team; }

        public GameModel Parent { get; }

        public Player Object { get; }

        public string Title => String.Concat(Object.Name, " (", Object.IsBot ? "Bot" : Object.UserID.ToString(), ")");

        public string ListItemString => String.Concat(Object.Score, CenterDotSeparator, Title);

        private IEnumerable<WeaponModel> _weapons;
        public IEnumerable<WeaponModel> Weapons { get => _weapons; private set => Set(ref _weapons, value); }

        private IEnumerable<StripeModel> _stripes;
        public IEnumerable<StripeModel> Stripes { get => _stripes; private set => Set(ref _stripes, value); }

        public string Name => Object.Name;

        public int UserID => Object.UserID;

        public double Score => Object.Score;

        public double ArmorDamageDealt => Object.ArmorDamageDealt;

        public double CriticalDamageDealt => Object.CriticalDamageDealt;

        public double ArmorDamageTaken => Object.ArmorDamageTaken;

        public double CriticalDamageTaken => Object.CriticalDamageTaken;

        public double TotalDamageDealt => Object.ArmorDamageDealt + Object.CriticalDamageDealt;

        public double TotalDamageTaken => Object.ArmorDamageTaken + Object.CriticalDamageTaken;

        public int Kills => Object.Kills;

        public int Assists => Object.Assists;

        public int Deaths => Object.Deaths;

        public byte Team => Object.Team;
    }

    public class PlayerModelScoreDescending : IComparer<PlayerModel>
    {
        int IComparer<PlayerModel>.Compare(PlayerModel x, PlayerModel y) => y.Score.CompareTo(x.Score);
    }
}
