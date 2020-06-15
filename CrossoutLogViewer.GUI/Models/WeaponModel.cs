using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Globalization;

namespace CrossoutLogView.GUI.Models
{
    public sealed class WeaponModel : ViewModelBase
    {
        public WeaponModel()
        {
            Parent = null;
            Weapon = new Weapon();
            Name = String.Empty;
        }

        public WeaponModel(object parent, Weapon obj)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Weapon = obj ?? throw new ArgumentNullException(nameof(obj));
            Name = DisplayStringFactory.AssetName(Weapon.Name);
        }

        public Weapon Weapon { get; }

        public object Parent { get; } // Either playerview or gameview

        public double TotalDamage => Weapon.ArmorDamage + Weapon.CriticalDamage;

        public string ListItemString => String.Concat(TotalDamage.ToString("0.##", CultureInfo.InvariantCulture.NumberFormat), Strings.CenterDotSeparator, Name);

        public string Name { get; }

        public double CriticalDamage => Weapon.CriticalDamage;

        public double ArmorDamage => Weapon.ArmorDamage;

        public int Uses => Weapon.Uses;
    }
}
