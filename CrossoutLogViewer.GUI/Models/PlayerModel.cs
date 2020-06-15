using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.Models
{
    public sealed class PlayerModel : CollectionViewModel
    {
        public PlayerModel()
        {
            Player = new Player();
            Parent = new GameModel();
            Weapons = new List<WeaponModel>();
            Stripes = new List<StripeModel>();
        }

        public PlayerModel(GameModel parent, Player obj)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Player = obj ?? throw new ArgumentNullException(nameof(obj));
            UpdateCollectionsSafe();
        }

        protected override void UpdateCollections()
        {
            if (Player == null) return;
            Player.Weapons.Sort((x, y) => (y.ArmorDamage + y.CriticalDamage).CompareTo(x.ArmorDamage + x.CriticalDamage));
            Weapons = Player.Weapons.Select(x => new WeaponModel(this, x));
            Player.Stripes.Sort((x, y) => y.Ammount.CompareTo(x.Ammount));
            Stripes = Player.Stripes.Select(x => new StripeModel(this, x));
        }

        public bool Won { get => Parent.WinningTeam == Player.Team; }

        public GameModel Parent { get; }

        public Player Player { get; }

        public string Title => String.Concat(Player.Name, " (", Player.IsBot ? "Bot" : Player.UserID.ToString(CultureInfo.CurrentUICulture.NumberFormat), ")");

        public string ListItemString => String.Concat(Player.Score, CenterDotSeparator, Title);

        private IEnumerable<WeaponModel> _weapons;
        public IEnumerable<WeaponModel> Weapons { get => _weapons; private set => Set(ref _weapons, value); }

        private IEnumerable<StripeModel> _stripes;
        public IEnumerable<StripeModel> Stripes { get => _stripes; private set => Set(ref _stripes, value); }

        public string Name => Player.Name;

        public int UserID => Player.UserID;

        public double Score => Player.Score;

        public double ArmorDamageDealt => Player.ArmorDamageDealt;

        public double CriticalDamageDealt => Player.CriticalDamageDealt;

        public double ArmorDamageTaken => Player.ArmorDamageTaken;

        public double CriticalDamageTaken => Player.CriticalDamageTaken;

        public double TotalDamageDealt => Player.ArmorDamageDealt + Player.CriticalDamageDealt;

        public double TotalDamageTaken => Player.ArmorDamageTaken + Player.CriticalDamageTaken;

        public int Kills => Player.Kills;

        public int Assists => Player.Assists;

        public int Deaths => Player.Deaths;

        public byte Team => Player.Team;
    }

    public sealed class PlayerModelScoreDescending : IComparer<PlayerModel>
    {
        int IComparer<PlayerModel>.Compare(PlayerModel x, PlayerModel y) => y.Score.CompareTo(x.Score);
    }
}
