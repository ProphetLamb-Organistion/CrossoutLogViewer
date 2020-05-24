using CrossoutLogView.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CrossoutLogView.Statistics
{
    public class WeaponGlobal : WeaponBase, IStatisticData
    {
        public List<User> Users;
        public List<int> Uses;
        public List<Game> Games;

        public WeaponGlobal() : base()
        {
            Users = new List<User>();
            Uses = new List<int>();
            Games = new List<Game>();
        }

        public WeaponGlobal(WeaponBase weapon) : this()
        {
            Name = weapon.Name;
            ArmorDamage = weapon.ArmorDamage;
            CriticalDamage = weapon.CriticalDamage;
        }

        public void Merge(WeaponGlobal other)
        {
            for (int i = 0; i < other.Users.Count; i++)
            {
                var otherUser = other.Users[i];
                var userIndex = Users.FindIndex(x => x.UserID == otherUser.UserID);
                if (userIndex == -1)
                {
                    userIndex = Users.Count;
                    Users.Add(otherUser);
                    Uses.Add(other.Uses[i]);
                }
                else Uses[userIndex] += other.Uses[i];
                ArmorDamage += other.ArmorDamage;
                CriticalDamage += other.CriticalDamage;
            }
        }

        public static List<WeaponGlobal> ParseWeapons(List<Game> games)
        {
            var weapons = new List<WeaponGlobal>();
            foreach (var g in games)
            {
                foreach (var p in g.Players)
                {
                    foreach (var w in p.Weapons)
                    {
                        var weapon = weapons.Find(x => Strings.NameEquals(x.Name, w.Name));
                        if (weapon == null) weapons.Add(weapon = new WeaponGlobal(w));
                        var userIndex = weapon.Users.FindIndex(x => x.UserID == p.UserID);
                        if (userIndex == -1)
                        {
                            weapon.Users.Add(new User(p.Name, p.UserID));
                            weapon.Uses.Add(1);
                        }
                        else weapon.Uses[userIndex] += 1;
                        weapon.ArmorDamage += w.ArmorDamage;
                        weapon.CriticalDamage += w.CriticalDamage;
                        if (!weapon.Games.Any(x => x.Start == g.Start))
                            weapon.Games.Add(g);
                    }
                }
            }
            return weapons;
        }

        public override string ToString() => String.Concat(nameof(WeaponGlobal), " ", Name, " ", ArmorDamage + CriticalDamage, " ", Users?.Count);

        public override int GetHashCode() => Name.GetHashCode();
    }
}
