using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.GUI.Models
{
    public class WeaponUserListModel : ViewModelBase
    {
        public WeaponUserListModel(Weapon weapon, User user)
        {
            Weapon = weapon;
            User = user;
        }

        public Weapon Weapon { get; }
        public User User { get; }

        public string DisplayName { get => DisplayStringFactory.AssetName(Weapon.Name); }

        public string UserName { get => User.Name; }

        public string ArmorDamage { get => Weapon.ArmorDamage.ToString("0.##"); }

        public string CriticalDamage { get => Weapon.CriticalDamage.ToString("0.##"); }

        public string TotalDamage { get => (Weapon.ArmorDamage + Weapon.CriticalDamage).ToString("0.##"); }

        public int Uses => Weapon.Uses;

        public override void UpdateCollections() { }
    }
}
