using CrossoutLogView.Common;
using CrossoutLogView.Log;

using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossoutLogView.Statistics
{
    public class Game : IStatisticData
    {
        public DateTime Start;
        public DateTime End;
        public GameMode Mode;
        public string Mission;
        public Map Map;
        public byte WinningTeam;
        public List<Round> Rounds;
        public List<Player> Players;
        public List<Weapon> Weapons;
        public int MVP;
        public int RedMVP;

        public Game()
        {
            Start = DateTime.MinValue;
            End = DateTime.MinValue;
            Mode = GameMode.PvP;
            Mission = String.Empty;
            Map = default;
            WinningTeam = 0xff;
            Rounds = new List<Round>();
            Players = new List<Player>();
            Weapons = new List<Weapon>();
            MVP = RedMVP = -1;
        }

        public static Game Parse(IEnumerable<ILogEntry> gameLog)
        {
            if (gameLog == null) throw new ArgumentNullException(nameof(gameLog));
            var game = new Game();
            if (!(gameLog.FirstOrDefault(x => x is GameStart) is GameStart start))
                throw new MatchingLogEntryNotFoundException("Could not find 'finish' logentry in gameLog.", nameof(start));
            if (!(gameLog.FirstOrDefault(x => x is ActiveBattleStart) is ActiveBattleStart battleStart))
                throw new MatchingLogEntryNotFoundException("Could not find 'battle start' logentry in gameLog.", nameof(battleStart));
            if (!(gameLog.FirstOrDefault(x => x is GameFinish) is GameFinish end))
                throw new MatchingLogEntryNotFoundException("Could not find 'finish' logentry in gameLog.", nameof(end));
            game.Start = new DateTime(battleStart.TimeStamp);
            game.End = game.Start.AddSeconds(end.GameDuration);
            game.Mode = GetGameMode(start, out game.Mission);
            game.Map = new Map(start.MapDisplayName, 1);
            game.WinningTeam = end.Team;
            game.Weapons = Weapon.ParseWeapons(gameLog.Where(x => x is Damage).Cast<Damage>());
            game.Rounds = Round.ParseRounds(game, gameLog);
            game.Players = Player.ParsePlayers(gameLog);
            if (game.WinningTeam != 0xff)
            {
                game.Players.Sort(new PlayerScoreDescendingComparer());
                var mvp = game.Players.FirstOrDefault(x => x.Team == game.WinningTeam);
                var redMvp = game.Players.FirstOrDefault(x => x.Team != game.WinningTeam);
                if (mvp != null && redMvp != null)
                {
                    game.MVP = mvp.PlayerIndex;
                    // Only exisits if half or more of the MVPs score
                    if (redMvp.Score * 2 >= mvp.Score) game.RedMVP = redMvp.PlayerIndex;
                    else game.RedMVP = -1;
                }
            }
            return game;
        }

        private static GameMode GetGameMode(GameStart startGame, out string Mission)
        {
            if (startGame.GameMode.EndsWith(Strings.GameModeClanWars))
            {
                Mission = startGame.GameMode[0..^Strings.GameModeClanWars.Length];
                return GameMode.ClanWars;
            }
            if (startGame.GameMode.StartsWith(Strings.GameModeBrawl))
            {
                Mission = startGame.GameMode[Strings.GameModeBrawl.Length..^0];
                return GameMode.Brawl;
            }
            if (startGame.GameMode.StartsWith(Strings.GameModePvE))
            {
                Mission = startGame.GameMode[Strings.GameModePvE.Length..^0];
                return GameMode.PvE;
            }
            Mission = startGame.GameMode;
            return GameMode.PvP;
        }

        public override string ToString() => String.Concat(nameof(Game), " ", Start.Ticks, " ", End.Ticks, " ", Mode, " ", Mission);
    }

    internal class PlayerScoreDescendingComparer : IComparer<Player>
    {
        int IComparer<Player>.Compare(Player x, Player y) => y.Score.CompareTo(x.Score);
    }
}
