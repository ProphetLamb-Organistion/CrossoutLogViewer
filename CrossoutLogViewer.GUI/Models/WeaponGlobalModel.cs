using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossoutLogView.GUI.Models
{
    public class WeaponGlobalModel : CollectionViewModel
    {
        private string _displayName;
        private IEnumerable<GameModel> _games;
        private IEnumerable<WeaponUserListModel> _weaponUsers;

        public WeaponGlobalModel(WeaponGlobal obj)
        {
            Weapon = obj ?? throw new ArgumentNullException(nameof(obj));
            DisplayName = DisplayStringFactory.AssetName(Weapon.Name);
            UpdateCollectionsSafe();
        }

        public WeaponGlobal Weapon { get; }

        public string DisplayName { get => _displayName; private set => Set(ref _displayName, value); }

        public double ArmorDamage { get => Weapon.ArmorDamage; }

        public double CriticalDamage { get => Weapon.CriticalDamage; }

        public double TotalDamage { get => Weapon.ArmorDamage + Weapon.CriticalDamage; }

        public int TotalUses { get => Weapon.Uses.Sum(); }

        public IEnumerable<GameModel> Games { get => _games; private set => Set(ref _games, value); }

        public IEnumerable<WeaponUserListModel> WeaponUsers { get => _weaponUsers; set => Set(ref _weaponUsers, value); }

        protected override void UpdateCollections()
        {
            var games = new List<GameModel>();
            var weaponUsers = new List<WeaponUserListModel>();
            Task.WaitAll(
            Task.Run(delegate
            {
                foreach (var game in Weapon.Games)
                {
                    games.Add(new GameModel(game));
                }
            }),
            Task.Run(delegate
            {
                foreach (var user in Weapon.Users)
                {
                    weaponUsers.Add(new WeaponUserListModel(user, Weapon));
                }
            }));
            Games = games;
            WeaponUsers = weaponUsers;
        }
    }

    public sealed class WeaponGlobalModelTotalUsesDescending : IComparer<WeaponGlobalModel>
    {
        int IComparer<WeaponGlobalModel>.Compare(WeaponGlobalModel x, WeaponGlobalModel y) => y.TotalUses.CompareTo(x.TotalUses);
    }
}
