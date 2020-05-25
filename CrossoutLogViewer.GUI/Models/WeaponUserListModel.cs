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
    public class WeaponUserListModel : ViewModelBase
    {
        private double _armorDamage;
        private double _critcalDamage;

        public WeaponUserListModel(User user, WeaponGlobal weapon)
        {
            User = user;
            Weapon = weapon;
            foreach (var g in Weapon.Games) DataProvider.CompleteGame(g);
            UpdateDamageData();
        }

        public User User { get; }
        public WeaponGlobal Weapon { get; }

        public string WeaponName => DisplayStringFactory.AssetName(Weapon.Name);

        public string UserName => User.Name;

        public double ArmorDamage { get => _armorDamage; set { Set(ref _armorDamage, value); OnPropertyChanged(nameof(TotalDamage)); } }

        public double CriticalDamage { get => _critcalDamage; set { Set(ref _critcalDamage, value); OnPropertyChanged(nameof(TotalDamage)); } }

        public double TotalDamage { get => _armorDamage + _critcalDamage; }

        public override void UpdateCollections() => UpdateDamageData();
        private void UpdateDamageData()
        {
            foreach (var weapon in Weapon.Games
                .SelectMany(g => g.Players)
                .Where(p => p.UserID == User.UserID)
                .SelectMany(p => p.Weapons)
                .Where(w => w.Name == Weapon.Name))
            {
                _armorDamage += weapon.ArmorDamage;
                _critcalDamage += weapon.CriticalDamage;
            }
            OnPropertyChanged(nameof(ArmorDamage));
            OnPropertyChanged(nameof(CriticalDamage));
            OnPropertyChanged(nameof(TotalDamage));
        }
    }
}
