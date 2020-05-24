using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.Models
{
    public sealed class KillModel : ViewModelBase
    {
        public KillModel()
        {
            Parent = null;
            Object = new Kill();
            Assists = new List<AssistModel>();
        }
        public KillModel(RoundModel parent, Kill obj)
        {
            Parent = parent;
            Object = obj;
            UpdateCollections();
        }

        public override void UpdateCollections()
        {
            Object.Assists.Sort((x, y) => x.Elapsed.CompareTo(y.Elapsed));
            Assists = Object.Assists.Select(x => new AssistModel(this, x));
        }

        public RoundModel Parent { get; }

        public Kill Object { get; }

        private IEnumerable<AssistModel> _assists;
        public IEnumerable<AssistModel> Assists { get => _assists; private set => Set(ref _assists, value); }

        private bool _isExpanded = false;
        public bool IsExpanded { get => _isExpanded; set => Set(ref _isExpanded, value); }

        public string TimeDisplayString => String.Concat(TimeSpan.FromSeconds(Object.Time).ToString(@"mm\:ss"), CenterDotSeparator);

        public string DamageFlagInfo
        {
            get
            {
                if (Object.Assists.Count == 0) return " despawned";
                if ((Object.Assists[0].DamageFlags & Log.DamageFlag.SUICIDE) == Log.DamageFlag.SUICIDE)
                    return " suicided";
                if ((Object.Assists[0].DamageFlags & Log.DamageFlag.SUICIDE_DESPAWN) == Log.DamageFlag.SUICIDE_DESPAWN)
                    return " despawned";
                return String.Empty;
            }
        }

        public SolidColorBrush DamageFlagInfoBrush => Object.Assists.Count == 0
            ? new SolidColorBrush()
            : (Object.Assists[0].DamageFlags & Log.DamageFlag.SUICIDE) == Log.DamageFlag.SUICIDE
            ? App.Current.Resources["Suicide"] as SolidColorBrush
            : (Object.Assists[0].DamageFlags & Log.DamageFlag.SUICIDE_DESPAWN) == Log.DamageFlag.SUICIDE_DESPAWN
            ? App.Current.Resources["Despawn"] as SolidColorBrush
            : new SolidColorBrush();

        public double Time => Object.Time;
        public string Killer => Object.Killer;
        public string Victim => Object.Victim;
    }
}
