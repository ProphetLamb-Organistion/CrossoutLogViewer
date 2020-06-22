using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.ValueConverters
{
    public class TitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string) || targetType == typeof(object))
            {
                if (value is UserModel user)
                    return String.Concat(user.User.Name, " (", user.User.UserID, ")");
                if (value is PlayerModel player)
                    return String.Concat(player.Player.Name, " (", player.Player.IsBot ? "Bot" : player.Player.UserID.ToString(CultureInfo.CurrentUICulture.NumberFormat), ")");
                if (value is GameModel game)
                    return String.Concat(DateTimeStringFactory(game.Game.Start), " - ", DateTimeStringFactory(game.Game.End), CenterDotSeparator, DisplayStringFactory.MapName(game.Game.Map.Name), CenterDotSeparator, game.Game.Mission, CenterDotSeparator, game.Game.Mode);
                if (value is string title)
                    return title + " (" + Settings.Current.MyName + ")";
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((targetType == typeof(string) || targetType == typeof(object)) && value is string str)
                return str.Split(" (")[0];
            throw new NotSupportedException();
        }
    }
}
