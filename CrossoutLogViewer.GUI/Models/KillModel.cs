using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.Models
{
    public sealed class KillModel : CollectionViewModel
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

        public string TimeDisplayString => String.Concat(TimeSpanStringFactory(Kill.Time), CenterDotSeparator);

        public string DamageFlagInfo
        {
            get
            {
                if (Kill.Assists.Count == 0) return " despawned";
                if ((Kill.Assists[0].DamageFlags & Log.DamageFlag.SUICIDE) == Log.DamageFlag.SUICIDE)
                    return " suicided";
                if ((Kill.Assists[0].DamageFlags & Log.DamageFlag.SUICIDE_DESPAWN) == Log.DamageFlag.SUICIDE_DESPAWN)
                    return " despawned";
                return String.Empty;
            }
        }

        public SolidColorBrush DamageFlagInfoBrush => Kill.Assists.Count == 0
            ? new SolidColorBrush()
            : (Kill.Assists[0].DamageFlags & Log.DamageFlag.SUICIDE) == Log.DamageFlag.SUICIDE
            ? App.Current.Resources["Suicide"] as SolidColorBrush
            : (Kill.Assists[0].DamageFlags & Log.DamageFlag.SUICIDE_DESPAWN) == Log.DamageFlag.SUICIDE_DESPAWN
            ? App.Current.Resources["Despawn"] as SolidColorBrush
            : new SolidColorBrush();

        public double Time => Kill.Time;
        public string Killer => Kill.Killer;
        public string Victim => Kill.Victim;
    }
}
