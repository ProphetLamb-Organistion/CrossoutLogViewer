using CrossoutLogView.Log;

using System;
using System.Collections.Generic;
using System.Linq;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.Statistics
{
    public class Player : PlayerBase, IStatisticData
    {
        public int PlayerIndex;
        public int PartyID;
        public bool IsBot;
        public byte Team;

        public Player() : base()
        {
            PlayerIndex = -1;
            PartyID = -1;
            IsBot = false;
            Team = 0xff;
        }

        public Player(PlayerLoad player) : base()
        {
            Name = player.PlayerNickName;
            PlayerIndex = player.PlayerNumber;
            UserID = player.UserID;
            PartyID = player.PartyID;
            IsBot = player.IsBot;
            Team = player.Team;
        }

        public static List<Player> ParsePlayers(IEnumerable<ILogEntry> gameLog)
        {
            var players = new List<Player>();
            Killing currentKill = null;
            foreach (var pll in gameLog.Where(x => x is PlayerLoad).Cast<PlayerLoad>())
            {
                players.Add(new Player(pll));
            }
            if (players.Count == 0) throw new PlayerNotFoundException("Log contains no players", nameof(gameLog));
            foreach (var logEntry in gameLog)
            {
                if (logEntry is Damage dmg)
                {
                    var attacker = players.Find(x => NameEquals(dmg.Attacker, x.Name));
                    var victim = players.Find(x => NameEquals(dmg.Victim, x.Name));
                    if (attacker != null && victim != null)
                    {
                        double criticalDamage, armorDamage;
                        if (dmg.IsCriticalDamage())
                        {
                            criticalDamage = dmg.DamageAmmount;
                            armorDamage = 0.0;
                        }
                        else
                        {
                            criticalDamage = 0.0;
                            armorDamage = dmg.DamageAmmount;
                        }
                        attacker.ArmorDamageDealt += armorDamage;
                        attacker.CriticalDamageDealt += criticalDamage;

                        var weaponName = TrimName(dmg.Weapon).ToString();
                        var weapon = attacker.Weapons.Find(x => NameEquals(weaponName, x.Name));
                        if (weapon == null) attacker.Weapons.Add(new Weapon(weaponName, criticalDamage, armorDamage));
                        else
                        {
                            weapon.ArmorDamage += armorDamage;
                            weapon.CriticalDamage += criticalDamage;
                        }
                        victim.ArmorDamageTaken += armorDamage;
                        victim.CriticalDamageTaken += criticalDamage;
                    }
                }
                else if (logEntry is Score score)
                {
                    var player = players.Find(x => x.PlayerIndex == score.PlayerNumber);
                    if (player != null) player.Score += score.ScoreAmmount;
                }
                else if (logEntry is Killing kill)
                {
                    var attacker = players.Find(x => NameEquals(kill.Killer, x.Name));
                    var victim = players.Find(x => NameEquals(kill.Victim, x.Name));
                    if (Kill.IsValidKill(attacker, victim))
                    {
                        attacker.Kills++;
                        victim.Deaths++;
                        currentKill = kill;
                    }
                }
                else if (currentKill != null && logEntry is KillAssist assist)
                {
                    var assistant = players.Find(x => NameEquals(x.Name, assist.Assistant));
                    if (assistant != null && !NameEquals(assistant.Name, currentKill.Killer)) assistant.Assists++;
                }
                else if (logEntry is Decal decal)
                {
                    var player = players.Find(x => x.PlayerIndex == decal.PlayerNumber);
                    var stripe = player.Stripes.Find(x => NameEquals(decal.StripeName, x.Name));
                    if (stripe != null) stripe.Ammount++;
                    else if (player != null) player.Stripes.Add(new Stripe(decal.StripeName, decal.AwardAmmount));
                }
            }
            //merge players from different rounds
            var playersDistinct = new List<Player>();
            foreach (IGrouping<int, Player> group in players.GroupBy(x => x.UserID))
            {
                Player distinct = null;
                foreach (var player in group)
                {
                    if (distinct == null) distinct = player;
                    else PlayerBase.Merge(distinct, player);
                }
                playersDistinct.Add(distinct);
            }
            return playersDistinct;
        }

        public override string ToString() => String.Concat(nameof(User), " ", Name, " ", UserID, " ", Team);
    }
}
