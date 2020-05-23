using CrossoutLogView.Common;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Log;
using CrossoutLogView.Statistics;

using System;
using System.Windows.Media;

namespace CrossoutLogView.GUI.Models
{

    public sealed class AssistModel : ViewModelBase
    {
        public AssistModel()
        {
            Parent = null;
            Object = new Assist();
        }
        public AssistModel(KillModel parent, Assist obj)
        {
            Parent = parent;
            Object = obj;
        }

        public override void UpdateCollections() { }

        public KillModel Parent { get; }

        public Assist Object { get; }


        private bool _isExpanded = false;
        public bool IsExpanded { get => _isExpanded; set => Set(ref _isExpanded, value); }

        public Brush DamageForeground => App.Current.Resources[Object.IsCriticalDamage ? "CriticalDamage" : "ArmorDamage"] as SolidColorBrush;

        public string DamageWithString => Object.IsCriticalDamage ? " criticaldamage with " : " damage with ";

        public string ListItemStringElapsed
        {
            get
            {
                if (Object.Elapsed == 0.0) return String.Empty;
                return String.Concat(Strings.CenterDotSeparator, Math.Round(Object.Elapsed), " sec ago");
            }
        }

        public string WeaponName => Object.Weapon.Name;

        public string Assistant => Object.Assistant;

        public Weapon Weapon => Object.Weapon;

        public double Elapsed => Object.Elapsed;

        public double Damage => Object.TotalDamage;

        public DamageFlag DamageFlags => Object.DamageFlags;

        public bool IsCriticalDamage => Object.IsCriticalDamage;
    }
}
