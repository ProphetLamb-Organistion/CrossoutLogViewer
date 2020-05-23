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

namespace CrossoutLogView.Database.Data
{
    public static class DataProvider
    {
        private static Dictionary<int, User> users = new Dictionary<int, User>();
        private static SortedSet<int> usersCompleted = new SortedSet<int>();
        private static Dictionary<int, long[]> userParticipationRowIds = new Dictionary<int, long[]>();
        private static Dictionary<long, Game> games = new Dictionary<long, Game>();
        private static Dictionary<long, long[]> gamesPlayerRowIds = new Dictionary<long, long[]>();
        private static SortedSet<long> gamesCompleted = new SortedSet<long>();
        private static Dictionary<int, WeaponGlobal> weapons = new Dictionary<int, WeaponGlobal>();
        private static Dictionary<string, GameMap> maps = new Dictionary<string, GameMap>();
        
        static DataProvider()
        {
            StatisticsUploader.InvalidateCachedData += OnInvalidateCachedData;
        }

        private static void OnInvalidateCachedData(object sender, InvalidateCachedDataEventArgs e)
        {
            foreach (var uid in e.UsersChanged)
            {
                users.Remove(uid);
                usersCompleted.Remove(uid);
                userParticipationRowIds.Remove(uid);
            }
            foreach (var wName in e.WeaponsChanged)
            {
                weapons.Remove(wName.GetHashCode());
            }
        }

        public static IEnumerable<User> GetUsers()
        {
            using var statCon = new StatisticsConnection();
            statCon.Open();
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
            statCon.Open();
            user = statCon.RequestUser(userId, TableRepresentation.Varialbe);
            var refs = statCon.RequestUserParticipationRowIds(userId);
            user.Participations = new List<Game>();
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

        public static void CompleteUser(int userId)
        {
            if (usersCompleted.Contains(userId)) return;
            var user = GetUser(userId);
            using var statCon = new StatisticsConnection();
            statCon.Open();
            user.Weapons = statCon.RequestUserWeapons(userId);
            user.Stripes = statCon.RequestUserStripes(userId);
            usersCompleted.Add(userId);
            statCon.Close();
        }

        public static Game GetGame(long rowId)
        {
            if (games.TryGetValue(rowId, out var game)) return game;
            using var statCon = new StatisticsConnection();
            statCon.Open();
            game = statCon.RequestGame(rowId, TableRepresentation.Varialbe);
            var playerRowIds = statCon.RequestGamePlayerRowIds(rowId);
            game.Players = statCon.RequestGamePlayers(playerRowIds, TableRepresentation.Varialbe);
            statCon.Close();
            games.Add(rowId, game);
            gamesPlayerRowIds.Add(rowId, playerRowIds);
            return game;
        }

        public static IEnumerable<Game> GetGames(IEnumerable<long> rowIds)
        {
            foreach(var rowId in rowIds)
            {
                yield return GetGame(rowId);
            }
        }

        public static void CompleteGame(Game game)
        {
            var rowId = games.First(x => x.Value.Start == game.Start && x.Value.End == game.End).Key;
            if (gamesCompleted.Contains(rowId)) return;
            using var statCon = new StatisticsConnection();
            statCon.Open();
            CompleteGame(rowId, game, statCon);
            statCon.Close();
        }

        public static void CompleteGame(long rowId)
        {
            if (gamesCompleted.Contains(rowId)) return;
            using var statCon = new StatisticsConnection();
            statCon.Open();
            CompleteGame(rowId, GetGame(rowId), statCon);
            statCon.Close();
        }

        private static void CompleteGame(long rowId, Game game, StatisticsConnection con)
        {
            var playerRowIds = con.RequestGamePlayerRowIds(rowId);
            var complement = con.RequestGamePlayers(playerRowIds, TableRepresentation.Array);
            for (int i = 0; i < playerRowIds.Length; i++)
            {
                foreach (var vi in VariableInfo
                    .FromType(typeof(Player))
                    .Where(x => (TableRepresentation.Array & Serialization.GetTableRepresentation(x.VariableType, IStatisticData.Implementations)) == Serialization.GetTableRepresentation(x.VariableType, IStatisticData.Implementations)))
                {
                    vi.SetValue(game.Players[i], vi.GetValue(complement[i]));
                }
            }
            game.Rounds = con.RequestGameRounds(rowId);
            game.Weapons = con.RequestGameWeapons(rowId);
            gamesCompleted.Add(rowId);
        }

        public static List<WeaponGlobal> GetWeapons()
        {
            var weapons = new List<WeaponGlobal>();
            using var statCon = new StatisticsConnection();
            statCon.Open();
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
            statCon.Open();
            weapons.Add(hash, weapon = statCon.RequestWeapon(name, TableRepresentation.All & ~TableRepresentation.ReferenceArray));
            var userIds = statCon.RequestWeaponUserIDs(name);
            weapon.Users = new List<User>();
            foreach (var uid in userIds)
            {
                weapon.Users.Add(GetUser(uid));
            }
            return weapon;
        }

        public static List<GameMap> GetMaps()
        {
            using var statCon = new StatisticsConnection();
            statCon.Open();
            var maps = new List<GameMap>();
            foreach (var mapRowId in statCon.RequestMapRowIds())
            {
                var map = statCon.RequestMap(mapRowId);
                var games = GetGames(statCon.RequestMapGameRowIds(mapRowId)).OrderByDescending(x => x.Start);
                maps.Add(new GameMap(map, games));
            }
            return maps;
        }
    }
}
