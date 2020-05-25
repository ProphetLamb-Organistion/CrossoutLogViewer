using CrossoutLogView.Common;
using CrossoutLogView.Log;

using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossoutLogView.Statistics
{
    public class Round : IStatisticData
    {
        public DateTime Start;
        public DateTime End;
        public byte Winner;
        public List<Kill> Kills;

        public Round()
        {
            Start = End = DateTime.MinValue;
            Winner = 0;
            Kills = new List<Kill>();
        }

        public Round(DateTime start, DateTime end, byte winner, List<Kill> kills)
        {
            Start = start;
            End = end;
            Winner = winner;
            Kills = kills;
        }

        public bool TryAddKill(Game parent, Killing kill, IEnumerable<KillAssist> killAssists)
        {
            var dateTime = new DateTime(kill.TimeStamp);
            if (Start > dateTime || dateTime > End) return false;
            double killTime = (dateTime - Start).TotalSeconds;
            var assists = new List<Assist>();
            foreach (var ka in killAssists)
            {
                //obtain weapon
                var weaponName = Strings.TrimName(ka.Weapon).ToString();
                var weapon = parent.Weapons.Find(x => Strings.NameEquals(weaponName, x.Name));
                if (weapon == null) //create new weapon
                {
                    var criticalDamage = ka.IsCriticalDamage ? ka.TotalDamage : 0.0;
                    var armorDamage = ka.IsCriticalDamage ? ka.TotalDamage : 0.0;
                    weapon = new Weapon(weaponName, criticalDamage, armorDamage);
                }
                assists.Add(new Assist(ka.Assistant, weapon, ka.Elapsed, ka.TotalDamage, ka.DamageFlags));
            }
            Kills.Add(new Kill(killTime, kill.Killer, kill.Victim, assists));
            return true;
        }

        public static List<Round> ParseRounds(Game parent, IEnumerable<ILogEntry> gameLog)
        {
            var rounds = new List<Round>();
            var gameRounds = gameLog
                    .Where(x => x is GameRound && x.TimeStamp > parent.Start.Ticks && x.TimeStamp < parent.End.Ticks)
                    .Cast<GameRound>()
                    .OrderBy(x => x.RoundNumber);
            if (gameRounds.Count() == 0) rounds.Add(new Round(parent.Start, parent.End, parent.WinningTeam, new List<Kill>()));
            else
            {
                var previousStart = parent.Start;
                foreach (var gameRound in gameRounds)
                {
                    var thisStart = previousStart.AddSeconds(gameRound.RoundDuration);
                    rounds.Add(new Round(previousStart,
                        thisStart,
                        gameRound.Team,
                        new List<Kill>()));
                    previousStart = thisStart;
                }
                rounds.Add(new Round(previousStart, parent.End, parent.WinningTeam, new List<Kill>()));
            }
            var kills = gameLog.Where(x => x is Killing).Cast<Killing>();
            var assists = gameLog.Where(x => x is KillAssist).Cast<KillAssist>().GroupBy(x => x.TimeStamp);
            foreach (var assistKillGroup in assists)
            {
                var kill = kills.FirstOrDefault(x => x.TimeStamp == assistKillGroup.Key);
                if (kill == null) continue;
                foreach (var r in rounds)
                {
                    r.TryAddKill(parent, kill, assistKillGroup);
                }
            }
            return rounds;
        }

        public override string ToString() => String.Concat(nameof(Round), " ", Start, " ", End, " ", Kills?.Count);
    }
}
