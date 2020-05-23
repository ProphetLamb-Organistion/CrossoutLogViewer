using CrossoutLogView.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CrossoutLogView.Database.Events
{
    public delegate void InvalidateCachedDataEventHandler(object sender, InvalidateCachedDataEventArgs e);
    public sealed class InvalidateCachedDataEventArgs : EventArgs
    {
        public InvalidateCachedDataEventArgs(int gamesAdded, IEnumerable<int> usersChanged, IEnumerable<string> weaponsChanged)
        {
            GamesAdded = gamesAdded;
            UsersChanged = usersChanged;
            WeaponsChanged = weaponsChanged;
        }

        public int GamesAdded { get; }
        public IEnumerable<int> UsersChanged { get; }
        public IEnumerable<string> WeaponsChanged { get; }
    }
}
