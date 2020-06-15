using CrossoutLogView.Common;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Events;
using CrossoutLogView.Database.Reflection;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrossoutLogView.Database.Data
{
    public static class DataProvider
    {
        private static ConcurrentDictionary<int, User> users = new ConcurrentDictionary<int, User>();
        private static SortedSet<int> usersCompleted = new SortedSet<int>();
        private static ConcurrentDictionary<int, long[]> userParticipationRowIds = new ConcurrentDictionary<int, long[]>();

        private static ConcurrentDictionary<long, Game> games = new ConcurrentDictionary<long, Game>();
        private static ConcurrentDictionary<DateTime, long> gamesStartRowIds = new ConcurrentDictionary<DateTime, long>();
        private static ConcurrentDictionary<long, long[]> gamesPlayerRowIds = new ConcurrentDictionary<long, long[]>();
        private static SortedSet<long> gamesCompleted = new SortedSet<long>();

        private static ConcurrentDictionary<string, WeaponGlobal> weapons = new ConcurrentDictionary<string, WeaponGlobal>();
        private static SortedSet<string> completedWeapons = new SortedSet<string>();

        private static ConcurrentDictionary<long, GameMap> gameMaps = new ConcurrentDictionary<long, GameMap>();
        private static ConcurrentDictionary<string, long> gameMapRowIds = new ConcurrentDictionary<string, long>();

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
                    weapons.Remove(wName);
                    completedWeapons.Remove(wName);
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
            if (userId == -1) return null;
            if (users.TryGetValue(userId, out var user)) return user;
            using var statCon = new StatisticsConnection();
            user = statCon.RequestUser(userId, TableRepresentation.Varialbe);
            var refs = statCon.RequestUserParticipationRowIds(userId);
            for (int i = 0; i < refs.Length; i++)
            {
                user.Participations.Add(GetGame(refs[i]));
            }
            user.Participations.Sort((x, y) => y.Start.CompareTo(x.Start));
            users.Add(userId, user);
            userParticipationRowIds.Add(userId, refs);
            return user;
        }

        public static bool CompleteUser(User user)
        {
            if (user == null || usersCompleted.Contains(user.UserID)) return false;
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
            if (rowId == -1) return null;
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
            if (game == null || !gamesStartRowIds.TryGetValue(game.Start, out var rowId) || gamesCompleted.Contains(rowId)) return false;
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
            if (String.IsNullOrEmpty(name)) return null;
            if (weapons.TryGetValue(name, out var weapon)) return weapon;
            using var statCon = new StatisticsConnection();
            weapon = statCon.RequestWeapon(name, TableRepresentation.All & ~TableRepresentation.ReferenceArray);
            weapons.Add(name, weapon);
            return weapon;
        }

        public static bool CompleteWeapon(WeaponGlobal weapon)
        {
            if (weapon == null) return false;
            if (completedWeapons.Contains(weapon.Name)) return false;
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
            completedWeapons.Add(weapon.Name);
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
                    gameMap = new GameMap(map, games);
                    AddMap(mapRowId, gameMap);
                }
                maps.Add(gameMap);
            }
            return maps;
        }

        public static GameMap GetMap(string mapName)
        {
            if (String.IsNullOrEmpty(mapName)) return null;
            if (gameMapRowIds.TryGetValue(mapName, out var mapRowId)) return gameMaps[mapRowId];
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
            var gameMap = new GameMap(map, games);
            AddMap(mapRowId, gameMap);
            return gameMap;
        }

        private static void AddMap(long mapRowId, GameMap gameMap)
        {
            if (mapRowId == -1 || gameMap == null) return;
            gameMaps.Add(mapRowId, gameMap);
            gameMapRowIds.Add(gameMap.Map.Name, mapRowId);
        }
    }
}
