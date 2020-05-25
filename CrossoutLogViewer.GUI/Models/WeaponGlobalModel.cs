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
    public class WeaponGlobalModel : ViewModelBase
    {
        private string _displayName;
        private IEnumerable<GameModel> _games;
        private IEnumerable<WeaponUserListModel> _weaponUsers;

        public WeaponGlobalModel(WeaponGlobal obj)
        {
            Object = obj;
            DisplayName = DisplayStringFactory.AssetName(Object.Name);
            UpdateCollections();
        }

        public WeaponGlobal Object { get; }

        public string DisplayName { get => _displayName; private set => Set(ref _displayName, value); }

        public double ArmorDamage { get => Object.ArmorDamage; }

        public double CriticalDamage { get => Object.CriticalDamage; }

        public double TotalDamage { get => Object.ArmorDamage + Object.CriticalDamage; }

        public int TotalUses { get => Object.Uses.Sum(); }

        public IEnumerable<GameModel> Games { get => _games; private set => Set(ref _games, value); }

        public IEnumerable<WeaponUserListModel> WeaponUsers { get => _weaponUsers; set => Set(ref _weaponUsers, value); }

        public override void UpdateCollections()
        {
            var games = new List<GameModel>();
            var weaponUsers = new List<WeaponUserListModel>();
            Task.WaitAll(
            Task.Run(delegate
            {
                foreach (var game in Object.Games)
                {
                    games.Add(new GameModel(game));
                }
            }),
            Task.Run(delegate
            {
                foreach (var user in Object.Users)
                {
                    weaponUsers.Add(new WeaponUserListModel(user, Object));
                }
            }));
            Games = games;
            WeaponUsers = weaponUsers;
        }
    }

    public class WeaponGlobalModelTotalUsesDescending : IComparer<WeaponGlobalModel>
    {
        int IComparer<WeaponGlobalModel>.Compare(WeaponGlobalModel x, WeaponGlobalModel y) => y.TotalUses.CompareTo(x.TotalUses);
    }
}
