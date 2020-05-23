using CrossoutLogView.GUI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CrossoutLogView.GUI.Events
{
    public delegate void WeaponFilterChangedEventHandler(object sender, WeaponFilterChangedEventArgs e);
    public sealed class WeaponFilterChangedEventArgs : EventArgs
    {
        public WeaponFilterChangedEventArgs(WeaponFilter oldValue, WeaponFilter newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public WeaponFilter OldValue { get; }
        public WeaponFilter NewValue { get; }
    }

    public readonly struct WeaponFilter : IEquatable<WeaponFilter>
    {
        public readonly string WeaponName;
        public readonly string UserName;

        public WeaponFilter(string weaponName, string userName)
        {
            WeaponName = weaponName;
            UserName = userName;
        }

        public bool Filter(object obj)
        {
            if (!(obj is WeaponGlobalModel weapon)) return false;
            if (!String.IsNullOrEmpty(WeaponName))
            {
                foreach (var part in WeaponName.TrimEnd().Split(' ', '-', '_'))
                {
                    if (!weapon.DisplayName.Contains(part, StringComparison.InvariantCultureIgnoreCase)) return false;
                }
            }
            if (!String.IsNullOrEmpty(UserName))
            {
                foreach (var part in UserName.TrimEnd().Split(' ', '-', '_'))
                {
                    if (!weapon.Users.Any(x => x.UserName.Contains(part, StringComparison.InvariantCultureIgnoreCase))) return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj) => obj is WeaponFilter filter && Equals(filter);
        public bool Equals([AllowNull] WeaponFilter other) => WeaponName == other.WeaponName && UserName == other.UserName;
        public override int GetHashCode() => HashCode.Combine(WeaponName, UserName);

        public static bool operator ==(WeaponFilter left, WeaponFilter right) => left.Equals(right);
        public static bool operator !=(WeaponFilter left, WeaponFilter right) => !(left == right);
    }
}
