using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossoutLogView.GUI.Models
{
    public class WeaponGlobalModel : ViewModelBase
    {
        public WeaponGlobalModel(WeaponGlobal obj)
        {
            Object = obj;
            DisplayName = DisplayStringFactory.AssetName(Object.Name);
            UpdateCollections();
        }

        public WeaponGlobal Object { get; }

        private string _displayName;
        public string DisplayName { get => _displayName; private set => Set(ref _displayName, value); }

        public double ArmorDamage { get => Object.ArmorDamage; }

        public double CriticalDamage { get => Object.CriticalDamage; }

        public double TotalDamage { get => Object.ArmorDamage + Object.CriticalDamage; }

        public int TotalUses { get => Object.Uses.Sum(); }


        private IEnumerable<WeaponUserListModel> _users;
        public IEnumerable<WeaponUserListModel> Users { get => _users; private set => Set(ref _users, value); }

        public override void UpdateCollections()
        {
            var count = Object.Users.Count;
            var users = new List<WeaponUserListModel>();
            for (int i = 0; i < count; i++)
            {
                var user = Object.Users[i];
                DataProvider.CompleteUser(user.UserID);
                var weapon = user.Weapons.Find(x => x.Name == Object.Name);
                if (weapon == null)
                {
                    Logging.WriteLine<WeaponGlobalModel>(new WeaponNotFoundException(String.Format("Weapon '{0}' for user '{1}' {2} not found.",
                        Object.Name, user.Name, user.UserID)));
                }
                else users.Add(new WeaponUserListModel(weapon, user));
            }
            Users = users;
        }
    }

    public class WeaponGlobalModelTotalUsesDescending : IComparer<WeaponGlobalModel>
    {
        int IComparer<WeaponGlobalModel>.Compare(WeaponGlobalModel x, WeaponGlobalModel y) => y.TotalUses.CompareTo(x.TotalUses);
    }
}
