using CrossoutLogView.Common;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Events;
using CrossoutLogView.Database.Reflection;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossoutLogView.Database.Data
{
    public static class DataProvider
    {
        private static Dictionary<int, User> users = new Dictionary<int, User>();
        private static SortedSet<int> usersCompleted = new SortedSet<int>();
        private static Dictionary<int, long[]> userParticipationRowIds = new Dictionary<int, long[]>();

        private static Dictionary<long, Game> games = new Dictionary<long, Game>();
        private static Dictionary<DateTime, long> gamesStartRowIds = new Dictionary<DateTime, long>();
        private static Dictionary<long, long[]> gamesPlayerRowIds = new Dictionary<long, long[]>();
        private static SortedSet<long> gamesCompleted = new SortedSet<long>();

        private static Dictionary<int, WeaponGlobal> weapons = new Dictionary<int, WeaponGlobal>();
        private static SortedSet<int> completedWeapons = new SortedSet<int>();

        private static Dictionary<long, GameMap> gameMaps = new Dictionary<long, GameMap>();
        private static Dictionary<string, long> gameMapRowIds = new Dictionary<string, long>();

        public static InvalidateCachedDataEventHandler InvalidateCachedData;

        static DataProvider()
        {
            StatisticsUploader.InvalidateCachedData += OnInvalidateCachedData;
        }

        private static void OnInvalidateCachedData(object sender, InvalidateCachedDataEventArgs e)
        {
            Task.WaitAll(
            Task.Run(delegate
            {
                foreach (var uid in e.UsersChanged)
                {
                    users.Remove(uid);
                    usersCompleted.Remove(uid);
                    userParticipationRowIds.Remove(uid);
                }
            }),
            Task.Run(delegate
            {
                foreach (var wName in e.WeaponsChanged)
                {
                    var nHash = wName.GetHashCode();
                    weapons.Remove(nHash);
                    completedWeapons.Remove(nHash);
                }
            }),
            Task.Run(delegate
            {
                foreach (var mName in e.MapsPlayed)
                {
                    if (gameMapRowIds.TryGetValue(mName, out var mapRowId))
                    {
                        gameMapRowIds.Remove(mName);
                        gameMaps.Remove(mapRowId);
                    }
                }
            }));
            InvalidateCachedData?.Invoke(sender, e);
        }

        public static IEnumerable<User> GetUsers()
        {
            using var statCon = new StatisticsConnection();
            var userIds = statCon.RequestUserIDs();
            statCon.Dispose();
            foreach (var userId in userIds)
            {
                yield return GetUser(userId);
            }
        }

        public static User GetUser(int userId)
        {
            if (users.TryGetValue(userId, out var user)) return user;
            using var statCon = new StatisticsConnection();
            user = statCon.RequestUser(userId, TableRepresentation.Varialbe);
            var refs = statCon.RequestUserParticipationRowIds(userId);
            for (int i = 0; i < refs.Length; i++)
            {
                user.Participations.Add(GetGame(refs[i]));
            }
            user.Participations.Sort((x, y) => y.Start.CompareTo(x.Start));
            statCon.Close();
            users.Add(userId, user);
            userParticipationRowIds.Add(userId, refs);
            return user;
        }

        public static bool CompleteUser(User user)
        {
            if (usersCompleted.Contains(user.UserID)) return false;
            using var statCon = new StatisticsConnection();
            Task.WaitAll(
            Task.Run(delegate
            {
                user.Weapons = statCon.RequestUserWeapons(user.UserID);
            }),
            Task.Run(delegate
            {
                user.Stripes = statCon.RequestUserStripes(user.UserID);
            }));
            usersCompleted.Add(user.UserID);
            statCon.Close();
            return true;
        }

        public static IEnumerable<Game> GetGames(IEnumerable<long> rowIds)
        {
            foreach (var rowId in rowIds)
            {
                yield return GetGame(rowId);
            }
        }

        public static Game GetGame(long rowId)
        {
            if (games.TryGetValue(rowId, out var game)) return game;
            long[] playerRowIds;
            using (var statCon = new StatisticsConnection())
            {
                game = statCon.RequestGame(rowId, TableRepresentation.Varialbe);
                playerRowIds = statCon.RequestGamePlayerRowIds(rowId);
                game.Players = statCon.RequestGamePlayers(playerRowIds, TableRepresentation.Varialbe);
            }
            games.Add(rowId, game);
            gamesStartRowIds.Add(game.Start, rowId);
            gamesPlayerRowIds.Add(rowId, playerRowIds);
            return game;
        }

        public static bool CompleteGame(Game game)
        {
            if (!gamesStartRowIds.TryGetValue(game.Start, out var rowId)) throw new ArgumentOutOfRangeException(nameof(game.Start));
            if (gamesCompleted.Contains(rowId)) return false;
            using var statCon = new StatisticsConnection();
            Task.WaitAll(
            Task.Run(delegate
            {
                var playerRowIds = statCon.RequestGamePlayerRowIds(rowId);
                var complement = statCon.RequestGamePlayers(playerRowIds, TableRepresentation.Array);
                for (int i = 0; i < playerRowIds.Length; i++)
                {
                    foreach (var vi in VariableInfo
                        .FromType(typeof(Player))
                        .Where(x => (TableRepresentation.Array & Serialization.GetTableRepresentation(x.VariableType, IStatisticData.Implementations)) == Serialization.GetTableRepresentation(x.VariableType, IStatisticData.Implementations)))
                    {
                        vi.SetValue(game.Players[i], vi.GetValue(complement[i]));
                    }
                }
            }),
            Task.Run(delegate
            {
                game.Rounds = statCon.RequestGameRounds(rowId);
            }),
            Task.Run(delegate
            {
                game.Weapons = statCon.RequestGameWeapons(rowId);
            }));
            gamesCompleted.Add(rowId);
            return true;
        }

        public static List<WeaponGlobal> GetWeapons()
        {
            var weapons = new List<WeaponGlobal>();
            using var statCon = new StatisticsConnection();
            foreach (var name in statCon.RequestWeaponNames())
            {
                weapons.Add(GetWeapon(name));
            }
            return weapons;
        }

        public static WeaponGlobal GetWeapon(string name)
        {
            var hash = name.GetHashCode();
            if (weapons.TryGetValue(hash, out var weapon)) return weapon;
            using var statCon = new StatisticsConnection();
            weapons.Add(hash, weapon = statCon.RequestWeapon(name, TableRepresentation.All & ~TableRepresentation.ReferenceArray));
            return weapon;
        }

        public static bool CompleteWeapon(WeaponGlobal weapon)
        {
            var nHash = weapon.Name.GetHashCode();
            if (completedWeapons.Contains(nHash)) return false;
            using var statCon = new StatisticsConnection();
            Task.WaitAll(
            Task.Run(delegate
            {
                foreach (var uid in statCon.RequestWeaponUserIDs(weapon.Name))
                {
                    weapon.Users.Add(GetUser(uid));
                }
            }),
            Task.Run(delegate
            {
                foreach (var rid in statCon.RequestWeaponGamesRowIDs(weapon.Name))
                {
                    weapon.Games.Add(GetGame(rid));
                }
            }));
            completedWeapons.Add(nHash);
            return true;
        }

        public static List<GameMap> GetMaps()
        {
            using var statCon = new StatisticsConnection();
            var maps = new List<GameMap>();
            foreach (var mapRowId in statCon.RequestMapRowIds())
            {
                if (!gameMaps.TryGetValue(mapRowId, out var gameMap))
                {
                    var map = statCon.RequestMap(mapRowId);
                    var games = GetGames(statCon.RequestMapGameRowIds(mapRowId)).OrderByDescending(x => x.Start);
                    AddMap(mapRowId, gameMap = new GameMap(map, games));
                }
                maps.Add(gameMap);
            }
            return maps;
        }

        public static GameMap GetMap(string mapName)
        {
            GameMap gameMap;
            if (gameMapRowIds.TryGetValue(mapName, out var mapRowId))
            {
                gameMap = gameMaps[mapRowId];
            }
            else
            {
                using var statCon = new StatisticsConnection();
                mapRowId = statCon.RequestMapRowId(mapName);
                Map map = default;
                IEnumerable<Game> games = default;
                Task.WaitAll(
                Task.Run(delegate
                {
                    map = statCon.RequestMap(mapRowId);
                }),
                Task.Run(delegate
                {
                     games = GetGames(statCon.RequestMapGameRowIds(mapRowId)).OrderByDescending(x => x.Start);
                }));
                AddMap(mapRowId, gameMap = new GameMap(map, games));
            }
            return gameMap;
        }

        private static void AddMap(long mapRowId, GameMap gameMap)
        {
            gameMaps.Add(mapRowId, gameMap);
            gameMapRowIds.Add(gameMap.Map.Name, mapRowId);
        }
    }
}
