
using System;
using System.Collections.Generic;

namespace CrossoutLogView.Database.Events
{
    public delegate void InvalidateCachedDataEventHandler(object sender, InvalidateCachedDataEventArgs e);
    public sealed class InvalidateCachedDataEventArgs : EventArgs
    {
        public InvalidateCachedDataEventArgs(IEnumerable<int> usersChanged, IEnumerable<string> weaponsChanged, IEnumerable<string> mapsPlayed)
        {
            UsersChanged = usersChanged;
            WeaponsChanged = weaponsChanged;
            MapsPlayed = mapsPlayed;
        }

        public IEnumerable<int> UsersChanged { get; }
        public IEnumerable<string> WeaponsChanged { get; }
        public IEnumerable<string> MapsPlayed { get; }
    }
}
