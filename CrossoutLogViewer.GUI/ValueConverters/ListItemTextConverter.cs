using CrossoutLogView.GUI.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.ValueConverters
{
    public class ListItemTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string) || targetType == typeof(object))
            {
                if (value is StripeModel stripe)
                {
                    return String.Concat(stripe.Stripe.Ammount, CenterDotSeparator, stripe.Stripe.Name);
                }
                if (value is RoundModel round)
                {
                    return new StringBuilder()
                        .Append(App.GetSharedResource("Round"))
                        .Append(round.RoundNumber)
                        .Append(CenterDotSeparator)
                        .Append(App.GetSharedResource("From"))
                        .Append(TimeSpanStringFactory(round.Round.Start - round.Game.Start))
                        .Append(App.GetSharedResource("To"))
                        .Append(TimeSpanStringFactory(round.Round.End - round.Game.Start))
                        .Append(CenterDotSeparator)
                        .Append(App.GetSharedResource("Duration"))
                        .Append(TimeSpanStringFactory(round.Round.End - round.Round.Start))
                        .Append(CenterDotSeparator)
                        .Append(round.Round.Kills.Count)
                        .Append(App.GetSharedResource("Kills"))
                        .ToString();
                }
                if (value is AssistModel assist)
                {
                    if (assist.Assist.Elapsed == 0.0) return String.Empty;
                    return String.Concat(CenterDotSeparator, Math.Round(assist.Assist.Elapsed), App.GetSharedResource("SecAgo"));
                }
                if (value is PlayerModel player)
                {
                    return String.Concat(player.Player.Score, CenterDotSeparator, new TitleConverter().Convert(player, typeof(string), null, CultureInfo.CurrentUICulture));
                }
                if (value is WeaponModel weapon)
                {
                    return String.Concat(weapon.TotalDamage.ToString("0.##", CultureInfo.CurrentUICulture.NumberFormat), CenterDotSeparator, weapon.Name);
                }
            }
            throw new NotSupportedException();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
