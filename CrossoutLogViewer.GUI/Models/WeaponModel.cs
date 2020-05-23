using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;

namespace CrossoutLogView.GUI.Models
{
    public sealed class WeaponModel : ViewModelBase
    {
        public WeaponModel()
        {
            Parent = null;
            Object = new Weapon();
            Name = String.Empty;
        }

        public WeaponModel(object parent, Weapon obj)
        {
            Parent = parent;
            Object = obj;
            Name = DisplayStringFactory.AssetName(Object.Name);
        }

        public override void UpdateCollections() { }

        public Weapon Object { get; }

        public object Parent { get; } //either playerview or gameview

        public double TotalDamage => Object.ArmorDamage + Object.CriticalDamage;

        public string ListItemString => String.Concat(TotalDamage.ToString("0.##"), Strings.CenterDotSeparator, Name);

        public string Name { get; }

        public double CriticalDamage => Object.CriticalDamage;

        public double ArmorDamage => Object.ArmorDamage;

        public int Uses => Object.Uses;
    }
}
