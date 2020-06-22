using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Log;
using CrossoutLogView.Statistics;

using System;
using System.Globalization;
using System.Runtime.Versioning;
using System.Windows.Data;
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

        public string DamageWithString => Assist.IsCriticalDamage ? "" : "";

        public string WeaponName => DisplayStringFactory.AssetName(Assist.Weapon.Name);

        public string Assistant => Assist.Assistant;

        public Weapon Weapon => Assist.Weapon;

        public double Elapsed => Assist.Elapsed;

        public double Damage => Assist.TotalDamage;

        public DamageFlag DamageFlags => Assist.DamageFlags;

        public bool IsCriticalDamage => Assist.IsCriticalDamage;
    }

    public class CriticalDamageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AssistModel assist)
            {
                if (targetType == typeof(Brush))
                    return App.Current.Resources[assist.Assist.IsCriticalDamage ? "CriticalDamage" : "ArmorDamage"];
                if (targetType == typeof(string) || targetType == typeof(object))
                    return App.GetControlResource(assist.Assist.IsCriticalDamage ? "Assist_CritWith" : "Assist_DmgWith");
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
