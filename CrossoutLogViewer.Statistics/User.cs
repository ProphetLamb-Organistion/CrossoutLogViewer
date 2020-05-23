using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossoutLogView.Statistics
{
    public class User : PlayerBase, IStatisticData, ICloneable
    {
        public List<Game> Participations;

        public User() : base()
        {
            Participations = new List<Game>();
        }
        public User(string name, int userId) : base()
        {
            Name = name;
            UserID = userId;
            Participations = new List<Game>();
        }

        public User(string name, int userID, double score,
            double armorDamageDealt, double criticalDamageDealt, double armorDamageTaken, double criticalDamageTaken,
            int kills, int assists, int deaths,
            List<Weapon> weapons, List<Stripe> stripes, List<Game> participations) : this(name, userID)
        {
            Score = score;
            ArmorDamageDealt = armorDamageDealt;
            CriticalDamageDealt = criticalDamageDealt;
            ArmorDamageTaken = armorDamageTaken;
            CriticalDamageTaken = criticalDamageTaken;
            Kills = kills;
            Assists = assists;
            Deaths = deaths;
            Weapons = weapons;
            Stripes = stripes;
            Participations = participations;
        }

        public static List<User> ParseUsers(IEnumerable<Game> games)
        {
            var users = new List<User>();
            foreach (var game in games)
            {
                foreach (var player in game.Players.Where(x => !x.IsBot))
                {
                    var user = users.Find(x => x.UserID == player.UserID);
                    if (user == null) users.Add(user = new User(player.Name, player.UserID));
                    Merge(user, player);
                    if (!user.Participations.Any(x => x.Start == game.Start)) //duplicates
                        user.Participations.Add(game);
                }
            }
            return users;
        }

        object ICloneable.Clone() => Clone();
        public User Clone() => new User(Name, UserID, Score,
            ArmorDamageDealt, CriticalDamageDealt, ArmorDamageTaken, CriticalDamageTaken,
            Kills, Assists, Deaths,
            Weapons, Stripes, Participations);

        public override string ToString() => String.Concat(nameof(User), " ", Name, " ", UserID, " ", Participations?.Count);
    }
}
