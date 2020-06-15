using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
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
            Kill = null;
            Assist = new Assist();
        }
        public AssistModel(KillModel kill, Assist assist)
        {
            Kill = kill ?? throw new ArgumentNullException(nameof(kill));
            Assist = assist ?? throw new ArgumentNullException(nameof(assist));
        }

        public KillModel Kill { get; }

        public Assist Assist { get; }


        private bool _isExpanded = false;
        public bool IsExpanded { get => _isExpanded; set => Set(ref _isExpanded, value); }

        public Brush DamageForeground => App.Current.Resources[Assist.IsCriticalDamage ? "CriticalDamage" : "ArmorDamage"] as SolidColorBrush;

        public string DamageWithString => Assist.IsCriticalDamage ? " criticaldamage with " : " damage with ";

        public string ListItemStringElapsed
        {
            get
            {
                if (Assist.Elapsed == 0.0) return String.Empty;
                return String.Concat(Strings.CenterDotSeparator, Math.Round(Assist.Elapsed), " sec ago");
            }
        }

        public string WeaponName => DisplayStringFactory.AssetName(Assist.Weapon.Name);

        public string Assistant => Assist.Assistant;

        public Weapon Weapon => Assist.Weapon;

        public double Elapsed => Assist.Elapsed;

        public double Damage => Assist.TotalDamage;

        public DamageFlag DamageFlags => Assist.DamageFlags;

        public bool IsCriticalDamage => Assist.IsCriticalDamage;
    }
}
