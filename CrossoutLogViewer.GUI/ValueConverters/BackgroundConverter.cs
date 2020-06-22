using CrossoutLogView.GUI.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace CrossoutLogView.GUI.ValueConverters
{
    public class BackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Brush) || targetType == typeof(object))
            {
                if (value is PlayerGameModel playerGame)
                    return playerGame.Won ? App.Current.Resources["TeamWon"] as Brush : !playerGame.Unfinished ? App.Current.Resources["TeamLost"] as Brush : default;
                if (value is GameModel game)
                    return game.Won ? App.Current.Resources["TeamWon"] as Brush : !game.Unfinished ? App.Current.Resources["TeamLost"] as Brush : default;
                if (value is RoundModel round)
                    return App.Current.Resources[round.Won ? "TeamWon" : "TeamLost"] as Brush;
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
