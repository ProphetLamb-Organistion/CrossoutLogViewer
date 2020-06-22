using CrossoutLogView.GUI.Core;
using CrossoutLogView.Log;
using CrossoutLogView.Statistics;

using NLog.Targets;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.Models
{
    public sealed class KillModel : CollectionViewModelBase
    {
        public KillModel()
        {
            Round = null;
            Kill = new Kill();
            Assists = new List<AssistModel>();
        }
        public KillModel(RoundModel round, Kill kill)
        {
            Round = round ?? throw new ArgumentNullException(nameof(round));
            Kill = kill ?? throw new ArgumentNullException(nameof(kill));
            UpdateCollectionsSafe();
        }

        protected override void UpdateCollections()
        {
            Kill.Assists.Sort((x, y) => x.Elapsed.CompareTo(y.Elapsed));
            Assists = Kill.Assists.Select(x => new AssistModel(this, x));
        }

        public RoundModel Round { get; }

        public Kill Kill { get; }

        private IEnumerable<AssistModel> _assists;
        public IEnumerable<AssistModel> Assists { get => _assists; private set => Set(ref _assists, value); }

        private bool _isExpanded = false;
        public bool IsExpanded { get => _isExpanded; set => Set(ref _isExpanded, value); }

        public double Time => Kill.Time;
        public string Killer => Kill.Killer;
        public string Victim => Kill.Victim;
    }

    public class DamageFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is KillModel kill)
            {
                if (targetType == typeof(Brush))
                {
                    return kill.Kill.Assists.Count == 0
                        ? new SolidColorBrush()
                        : (kill.Kill.Assists[0].DamageFlags & DamageFlag.SUICIDE) == DamageFlag.SUICIDE
                        ? App.Current.Resources["Suicide"] as SolidColorBrush
                        : (kill.Kill.Assists[0].DamageFlags & DamageFlag.SUICIDE_DESPAWN) == DamageFlag.SUICIDE_DESPAWN
                        ? App.Current.Resources["Despawn"] as SolidColorBrush
                        : default(Brush);
                }
                if (targetType == typeof(string) || targetType == typeof(object))
                {
                    if (kill.Kill.Assists.Count == 0 || (kill.Kill.Assists[0].DamageFlags & DamageFlag.SUICIDE_DESPAWN) == DamageFlag.SUICIDE_DESPAWN)
                        return App.GetControlResource("Kill_Despawn");
                    if ((kill.Kill.Assists[0].DamageFlags & DamageFlag.SUICIDE) == DamageFlag.SUICIDE)
                        return App.GetControlResource("Kill_Suicide");
                    return String.Empty;
                }
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class TimeDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((targetType == typeof(string) || targetType == typeof(object)) && value is KillModel kill)
                return String.Concat(TimeSpanStringFactory(kill.Kill.Time), CenterDotSeparator);
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
