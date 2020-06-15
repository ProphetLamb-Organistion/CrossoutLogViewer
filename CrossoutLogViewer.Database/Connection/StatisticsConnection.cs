using CrossoutLogView.Common;
using CrossoutLogView.Database.Reflection;
using CrossoutLogView.Statistics;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static CrossoutLogView.Common.SQLiteFormat;
using static CrossoutLogView.Database.Reflection.Serialization;

namespace CrossoutLogView.Database.Connection
{
    public class StatisticsConnection : ConnectionBase
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public StatisticsConnection()
        {
            InitializeDataStructure();
            DatabaseTableTypes = IStatisticData.Implementations;
        }

        protected override object InsertVariableHandler(in object obj, in VariableInfo vi)
        {
            if (obj is null || vi is null) return default;
            //Maps
            if (vi.VariableType.Equals(typeof(Map)))
            {
                var map = (Map)vi.GetValue(obj);
                //request existing map
                var request = GetRequestString(vi.VariableType, true)
                + String.Format(" WHERE {0} == {1}", nameof(Map.Name), SQLiteVariable(map.Name));
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                if (reader.Read()) //existing map
                {
                    var rowid = ReadField<long>(RowIdName, reader);
                    //increment gamesplayed
                    map.GamesPlayed += ReadField<int>(nameof(Map.GamesPlayed), reader);
                    UpdateValues(map, VariableInfo.FromType(vi.VariableType), vi.VariableType.Name, "rowid == " + rowid); //update
                    return rowid;
                }
                else
                {
                    Insert(map); //insert new map
                    return LastInsertRowId;
                }
            }
            //IEnumarable
            else if (GetTableRepresentation(vi.VariableType, DatabaseTableTypes) == TableRepresentation.ReferenceArray)
            {
                var baseType = Types.GetEnumerableBaseType(vi.VariableType);
                if (baseType == typeof(Game)) //IEnumarable<Game>
                {
                    var refs = new List<long>();
                    foreach (var game in (IEnumerable<Game>)vi.GetValue(obj))
                    {
                        var rowId = RequestGameRowId(game.Start.Ticks);
                        if (rowId == -1)
                        {
                            Insert(game);
                            refs.Add(LastInsertRowId);
                        }
                        else refs.Add(rowId);
                    }
                    return SerializeArray(refs, Base85.Encode);
                }
                if (baseType == typeof(User)) //IEnumerable<User>
                {
                    var refs = new List<long>();
                    foreach (var user in (IEnumerable<User>)vi.GetValue(obj))
                    {
                        var rowId = RequestUserRowId(user.UserID);
                        if (rowId == -1)
                        {
                            Insert(user);
                            refs.Add(LastInsertRowId);
                        }
                        else refs.Add(RequestUserRowId(user.UserID));
                    }
                    return SerializeArray(refs, Base85.Encode);
                }
            }
            return null;
        }

        public void Insert(User user)
        {
            if (user is null) return;
            if (user.UserID > 0) Insert<User>(user);
        }

        public void Insert(IEnumerable<Game> games)
        {
            InsertMany(games);
        }

        public void Insert(WeaponGlobal weapon)
        {
            Insert<WeaponGlobal>(weapon);
        }

        public Game RequestGame(DateTime containsDateTime, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            var type = typeof(Game);
            var request = GetRequestString(type, false, includeTableRepresentation) + String.Format(" WHERE {0} <= {1} AND {1} <= {2}",
                nameof(Game.Start).ToLowerInvariant(),
                containsDateTime.Ticks,
                nameof(Game.End).ToLowerInvariant());
            return ExecuteRequestSingleObject<Game>(request, includeTableRepresentation);
        }

        public Game RequestGame(long rowId, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            var request = GetRowIdRequestString(typeof(Game), rowId, includeTableRepresentation);
            return ExecuteRequestSingleObject<Game>(request, includeTableRepresentation);
        }

        public Map RequestMap(string name)
        {
            var request = String.Concat(GetRequestString(typeof(Map)), " WHERE ", nameof(Map.Name), " == ", name);
            return ExecuteRequestSingleObject<Map>(request);
        }

        public Map RequestMap(long rowId)
        {
            var request = GetRowIdRequestString(typeof(Map), rowId);
            return ExecuteRequestSingleObject<Map>(request);
        }

        public WeaponGlobal RequestWeapon(string name, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            var type = typeof(WeaponGlobal);
            var request = String.Concat(GetRequestString(type, false, includeTableRepresentation), " WHERE ", nameof(WeaponGlobal.Name), " == ", SQLiteVariable(name));
            return ExecuteRequestSingleObject<WeaponGlobal>(request, includeTableRepresentation);
        }

        public long[] RequestGamePlayerRowIds(long gameRowId)
        {
            return RequestReferences(RowIdName + " == " + gameRowId, typeof(Game), nameof(Game.Players));
        }

        public List<Player> RequestGamePlayers(long[] rowIds, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            if (rowIds == null || rowIds.Length == 0) return new List<Player>();
            return ExecuteRequest<Player>(GetRowIdRequestString(typeof(Player), rowIds, includeTableRepresentation), includeTableRepresentation);
        }

        public List<Round> RequestGameRounds(long gameRowId, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            var refs = RequestReferences(RowIdName + " == " + gameRowId, typeof(Game), nameof(Game.Rounds));
            if (refs == null || refs.Length == 0) return new List<Round>();
            return ExecuteRequest<Round>(GetRowIdRequestString(typeof(Round), refs, includeTableRepresentation), includeTableRepresentation);
        }

        public List<Weapon> RequestGameWeapons(long gameRowId, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            var refs = RequestReferences(RowIdName + " == " + gameRowId, typeof(Game), nameof(Game.Weapons));
            if (refs == null || refs.Length == 0) return new List<Weapon>();
            return ExecuteRequest<Weapon>(GetRowIdRequestString(typeof(Weapon), refs, includeTableRepresentation), includeTableRepresentation);
        }

        public List<long> RequestMapGameRowIds(long rowId, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            var rowIds = new List<long>();
            var request = String.Format(FormatRequestWhere, RowIdName, nameof(Game), nameof(Game.Map) + " == " + rowId);
            using var cmd = new SQLiteCommand(request, Connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rowIds.Add(ReadField<long>(RowIdName, reader));
            }
            return rowIds;
        }

        public List<long> RequestMapRowIds()
        {
            var rowIds = new List<long>();
            var request = String.Format(FormatRequest, RowIdName, nameof(Map));
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rowIds.Add(ReadField<long>(RowIdName, reader));
                }
            }
            catch (SQLiteException)
            {
                logger.Error(request.Substring(0, 254));
            }
            return rowIds;
        }

        public long RequestMapRowId(string mapName)
        {
            long rowId = -1;
            var request = String.Format(FormatRequestWhere, RowIdName, nameof(Map), nameof(Map.Name) + " == " + SQLiteVariable(mapName));
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    rowId = ReadField<long>(RowIdName, reader);
            }
            catch (SQLiteException)
            {
                logger.Error(request.Substring(0, 254));
            }
            return rowId;
        }

        public User RequestUser(string name, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            var type = typeof(User);
            var request = GetRequestString(type, false, includeTableRepresentation)
                + String.Format(" WHERE {0} == {1}",
                nameof(User.Name).ToLowerInvariant(),
                name);
            return ExecuteRequestSingleObject<User>(request, includeTableRepresentation);
        }

        public User RequestUser(int userId, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            var type = typeof(User);
            var request = GetRequestString(type, false, includeTableRepresentation)
                + String.Format(" WHERE {0} == {1}",
                nameof(User.UserID).ToLowerInvariant(),
                userId);
            return ExecuteRequestSingleObject<User>(request, includeTableRepresentation);
        }

        public List<Weapon> RequestUserWeapons(long userId, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            var refs = RequestReferences(nameof(User.UserID) + " == " + userId, typeof(User), nameof(User.Weapons));
            if (refs == null || refs.Length == 0) return new List<Weapon>();
            return ExecuteRequest<Weapon>(GetRowIdRequestString(typeof(Weapon), refs, includeTableRepresentation), includeTableRepresentation);
        }

        public List<Stripe> RequestUserStripes(long userId, TableRepresentation includeTableRepresentation = TableRepresentation.All)
        {
            var refs = RequestReferences(nameof(User.UserID) + " == " + userId, typeof(User), nameof(User.Stripes));
            if (refs == null || refs.Length == 0) return new List<Stripe>();
            return ExecuteRequest<Stripe>(GetRowIdRequestString(typeof(Stripe), refs, includeTableRepresentation), includeTableRepresentation);
        }

        public List<int> RequestUserIDs()
        {
            var userIDs = new List<int>();
            var request = String.Format(FormatRequest, nameof(User.UserID), nameof(User));
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    userIDs.Add(reader.GetInt32(0));
                }
            }
            catch (SQLiteException)
            {
                logger.Error(request.Substring(0, 254));
            }
            return userIDs;
        }

        public List<int> RequestUserIDs(long[] rowIDs)
        {
            if (rowIDs is null) return new List<int>();
            var conditions = new string[rowIDs.Length];
            var conditionTemplate = RowIdName + " == ";
            for (int i = 0; i < rowIDs.Length; i++)
            {
                conditions[i] = conditionTemplate + rowIDs[i];
            }
            var userIDs = new List<int>();
            var request = String.Concat(String.Format(FormatRequest, nameof(User.UserID), nameof(User)), " WHERE ", String.Join(" OR ", conditions));
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    userIDs.Add(reader.GetInt32(0));
                }
            }
            catch (SQLiteException)
            {
                logger.Error(request.Substring(0, 254));
            }
            return userIDs;
        }

        public long[] RequestUserParticipationRowIds(int userId)
        {
            return RequestReferences(nameof(User.UserID) + " == " + userId, typeof(User), nameof(User.Participations));
        }

        public List<int> RequestWeaponUserIDs(string weaponName)
        {
            var refs = RequestReferences(nameof(WeaponGlobal.Name) + " == " + SQLiteVariable(weaponName), typeof(WeaponGlobal), nameof(WeaponGlobal.Users));
            var userIDs = new List<int>();
            for (int i = 0; i < refs.Length; i++)
            {
                var userId = RequestUserId(refs[i]);
                if (userId > 0) userIDs.Add(userId);
            }
            return userIDs;
        }

        public long[] RequestWeaponGamesRowIDs(string weaponName)
        {
            return RequestReferences(nameof(WeaponGlobal.Name).ToLowerInvariant() + " == " + SQLiteVariable(weaponName), typeof(WeaponGlobal), nameof(WeaponGlobal.Games).ToLowerInvariant());
        }

        public List<string> RequestWeaponNames()
        {
            var names = new List<string>();
            var field = nameof(WeaponGlobal.Name).ToLowerInvariant();
            var request = String.Format(FormatRequest, field, nameof(WeaponGlobal));
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    names.Add(ReadField<string>(field, reader));
                }
            }
            catch (SQLiteException)
            {
                logger.Error(request.Length > 254 ? request.Substring(0, 254) : request);
            }
            return names;
        }

        public void UpdateUser(User user)
        {
            if (user is null) return;
            if (user.UserID <= 0) throw new ArgumentOutOfRangeException(nameof(User.UserID), "Value must be greater then zero. Value = " + user.UserID);
            var type = typeof(User);
            var request = GetRequestString(type) + String.Format(" WHERE {0} == {1}",
                nameof(User.UserID).ToLowerInvariant(),
                user.UserID);
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) // Insert new user
                {
                    Insert(user);
                }
                else // Update existing user            
                {
                    var sets = new StringBuilder();
                    var varInfos = VariableInfo.FromType(typeof(User));
                    for (int varInfoIndex = 0; varInfoIndex < varInfos.Length; varInfoIndex++)
                    {
                        var vi = varInfos[varInfoIndex];
                        if (vi.Name.Equals(nameof(User.UserID), StringComparison.InvariantCulture)
                            || vi.Name.Equals(nameof(User.Name), StringComparison.InvariantCulture))
                            continue;
                        var name = vi.Name.ToLowerInvariant();
                        // Increment numerical stats
                        if (vi.VariableType == typeof(double))
                            sets.AppendFormat(FormatEquals, name, ReadField<double>(vi.Name, reader) + (double)vi.GetValue(user));
                        else if (vi.VariableType == typeof(int))
                            sets.AppendFormat(FormatEquals, name, ReadField<int>(vi.Name, reader) + (int)vi.GetValue(user));
                        else if (GetTableRepresentation(vi.VariableType, DatabaseTableTypes) == TableRepresentation.ReferenceArray)
                        {
                            if (vi.Name.Equals(nameof(User.Participations), StringComparison.InvariantCulture)) // Globaly unique: find by start&end time make ref
                            {
                                var refs = new StringBuilder();
                                for (int i = 0; i < user.Participations.Count; i++)
                                {
                                    var gameReq = String.Format(FormatRequest, RowIdName, nameof(Game)) // Request rowid
                                        + String.Format(" WHERE {0} == {1}", nameof(Game.Start).ToLowerInvariant(), user.Participations[i].Start.Ticks);
                                    using var gameCmd = new SQLiteCommand(gameReq, Connection);
                                    using var gameReader = gameCmd.ExecuteReader();
                                    refs.Append(Strings.ArrayDelimiter);
                                    if (gameReader.Read())
                                    {
                                        refs.Append(Base85.Encode(ReadField<long>(RowIdName, gameReader)));
                                    }
                                }
                                sets.AppendFormat(FormatEquals, name, SQLiteVariable(ReadField<string>(vi.Name, reader) + refs.ToString()));
                            }
                            else if (vi.Name.Equals(nameof(User.Weapons), StringComparison.InvariantCulture))
                            {
                                var rowIds = ParseSerializedArray(ReadField<string>(vi.Name, reader), Base85.DecodeInt64);
                                var newWeapons = new List<Weapon>(user.Weapons);
                                for (int i = 0; i < rowIds.Length; i++)
                                {
                                    var weapon = ExecuteRequestSingleObject<Weapon>(GetRowIdRequestString(typeof(Weapon), rowIds[i], TableRepresentation.Store));
                                    if (weapon == null) continue;
                                    var newWeaponIndex = newWeapons.FindIndex(w => w.Name.Equals(weapon.Name, StringComparison.InvariantCultureIgnoreCase));
                                    if (newWeaponIndex != -1) // Weapon already exists; update existing 
                                    {
                                        UpdateValues(weapon.Merge(newWeapons[newWeaponIndex]), VariableInfo.FromType(typeof(Weapon)), nameof(Weapon), RowIdName + " == " + rowIds[i]);
                                        newWeapons.RemoveAt(newWeaponIndex);
                                    }
                                }
                                // Insert remaining weapons
                                var newRowIds = new long[newWeapons.Count + rowIds.Length];
                                for (int i = 0; i < newWeapons.Count; i++)
                                {
                                    Insert(newWeapons[i]);
                                    newRowIds[i] = LastInsertRowId;
                                }
                                Array.Copy(rowIds, 0, newRowIds, newWeapons.Count, rowIds.Length);
                                sets.AppendFormat(FormatEquals, name, SQLiteVariable(SerializeArray(newRowIds, Base85.Encode)));
                            }
                            else if (vi.Name.Equals(nameof(User.Stripes), StringComparison.InvariantCulture))
                            {
                                var rowIds = ParseSerializedArray(ReadField<string>(vi.Name, reader), Base85.DecodeInt64);
                                var newStripes = new List<Stripe>(user.Stripes);
                                for (int i = 0; i < rowIds.Length; i++)
                                {
                                    var stripe = ExecuteRequestSingleObject<Stripe>(GetRowIdRequestString(typeof(Stripe), rowIds[i], TableRepresentation.Store));
                                    if (stripe == null) continue;
                                    var newStripeIndex = newStripes.FindIndex(s => s.Name.Equals(stripe.Name, StringComparison.InvariantCultureIgnoreCase));
                                    if (newStripeIndex != -1) // Weapon already exists; update existing 
                                    {
                                        UpdateValues(stripe.Merge(newStripes[newStripeIndex]), VariableInfo.FromType(typeof(Stripe)), nameof(Stripe), RowIdName + " == " + rowIds[i]);
                                        newStripes.RemoveAt(newStripeIndex);
                                    }
                                }
                                // Insert remaining weapons
                                var newRowIds = new long[newStripes.Count + rowIds.Length];
                                for (int i = 0; i < newStripes.Count; i++)
                                {
                                    Insert(newStripes[i]);
                                    newRowIds[i] = LastInsertRowId;
                                }
                                Array.Copy(rowIds, 0, newRowIds, newStripes.Count, rowIds.Length);
                                sets.AppendFormat(FormatEquals, name, SQLiteVariable(SerializeArray(newRowIds, Base85.Encode)));
                            }
                        }
                    }
                    // Compose update string
                    InvokeNonQuery(String.Format(FormatUpdate, nameof(User), sets.Remove(0, 2).ToString(),
                        String.Format("{0} == {1}", nameof(User.UserID), user.UserID)),
                        Connection);
                }
            }
            catch (SQLiteException)
            {
                logger.Error(request.Substring(0, 254));
            }
        }

        public void UpdateWeaponGlobal(WeaponGlobal weapon)
        {
            if (weapon is null) return;
            var type = typeof(WeaponGlobal);
            var condition = nameof(WeaponGlobal.Name) + " == " + SQLiteVariable(weapon.Name);
            var request = GetRequestString(type) + " WHERE " + condition;
            // Request weapon from database
            var dbWeapon = ExecuteRequestSingleObject<WeaponGlobal>(request, TableRepresentation.All & ~TableRepresentation.ReferenceArray);
            if (dbWeapon is null)
            {
                Insert(weapon);
                return;
            }
            // References to users of dbWeapon
            var dbUserRefs = RequestReferences(nameof(WeaponGlobal.Name) + " == " + SQLiteVariable(weapon.Name), type, nameof(WeaponGlobal.Users));
            var uses = new List<int>(weapon.Users.Count + dbUserRefs.Length);
            for (int i = 0; i < dbUserRefs.Length; i++)
            {
                var uid = RequestUserId(dbUserRefs[i]);
                var index = weapon.Users.FindIndex(u => u.UserID == uid);
                if (index == -1)
                    uses.Add(dbWeapon.Uses[i]);
                else
                {
                    uses.Add(weapon.Uses[index] + dbWeapon.Uses[i]);
                    // Remove user in dbWeapon from weapon
                    weapon.Users.RemoveAt(index);
                    weapon.Uses.RemoveAt(index);
                }
            }
            // Obtain references to remaining users
            var newRefs = new List<long>(weapon.Users.Count + dbUserRefs.Length);
            for (int i = 0; i < newRefs.Count; i++)
            {
                uses.Add(weapon.Uses[i]);
                var rowId = RequestUserRowId(weapon.Users[i].UserID);
                if (rowId != -1)
                    newRefs.Add(rowId);
            }
            newRefs.AddRange(dbUserRefs);
            // Update string
            var sets = new StringBuilder();
            sets.Append(nameof(WeaponGlobal.ArmorDamage)).Append(" = ").Append(SQLiteVariable(dbWeapon.ArmorDamage + weapon.ArmorDamage)).Append(", ");
            sets.Append(nameof(WeaponGlobal.CriticalDamage)).Append(" = ").Append(SQLiteVariable(dbWeapon.CriticalDamage + weapon.CriticalDamage)).Append(", ");
            sets.Append(nameof(WeaponGlobal.Users)).Append(" = ").Append(SQLiteVariable(SerializeArray(newRefs, Base85.Encode))).Append(", ");
            sets.Append(nameof(WeaponGlobal.Uses)).Append(" = ").Append(SQLiteVariable(SerializeArray(uses, Base85.Encode)));
            InvokeNonQuery(String.Format(FormatUpdate, nameof(WeaponGlobal), sets.ToString(), condition), Connection);
        }

        public long RequestUserRowId(int userId)
        {
            long rowId = -1;
            var request = String.Concat(String.Format(FormatRequest, RowIdName, nameof(User)), " WHERE ", nameof(User.UserID).ToLowerInvariant(), " == ", userId);
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    rowId = ReadField<long>(RowIdName, reader);
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (SQLiteException)
            {
                logger.Error(request.Substring(0, 254));
            }
            return rowId;
        }

        public int RequestUserId(long rowId)
        {
            if (rowId == -1) return -1;
            int uid = -1;
            var request = String.Concat(String.Format(FormatRequest, nameof(User.UserID), nameof(User)), " WHERE ", RowIdName, " == ", rowId);
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    uid = reader.GetInt32(0);
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (SQLiteException)
            {
                logger.Error(request.Substring(0, 254));
            }
            return uid;
        }

        private static ConcurrentDictionary<long, long> gameRowIdStart = new ConcurrentDictionary<long, long>();
        public long RequestGameRowId(long startTicks)
        {
            if (gameRowIdStart.TryGetValue(startTicks, out var rowId)) return rowId;
            var request = String.Concat(String.Format(FormatRequest, RowIdName, nameof(Game)), " WHERE ", nameof(Game.Start).ToLowerInvariant(), " == ", startTicks);
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    gameRowIdStart.AddOrUpdate(startTicks, rowId = ReadField<long>(RowIdName, reader), (rid, st) => rowId);
                else rowId = -1;
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (SQLiteException)
            {
                logger.Error(request.Substring(0, 254));
            }
            return rowId;
        }

        private string UpdateListReferenceHelper(object obj, VariableInfo variableInfo)
        {
            var count = InsertMany((IEnumerable)variableInfo.GetValue(obj),
                Types.GetEnumerableBaseType(variableInfo.VariableType));
            if (count == 0) return String.Empty;
            var refs = new StringBuilder();
            for (long rowId = LastInsertRowId - count; rowId <= LastInsertRowId; rowId++)
            {
                refs.Append(Strings.ArrayDelimiter);
                refs.Append(Base85.Encode(rowId));
            }
            return refs.ToString();
        }

        private string UpdateListReferenceHelper(IEnumerable objects, Type itemType)
        {
            var count = InsertMany(objects, itemType);
            if (count == 0) return String.Empty;
            var refs = new StringBuilder();
            for (long rowId = LastInsertRowId - count; rowId <= LastInsertRowId; rowId++)
            {
                refs.Append(Strings.ArrayDelimiter);
                refs.Append(Base85.Encode(rowId));
            }
            return refs.ToString();
        }

        private static Dictionary<Guid, string> primaryKeys = new Dictionary<Guid, string>()
        {
            //{ typeof(Map).GUID, nameof(Map.Name) },
            { typeof(User).GUID, nameof(User.UserID) },
        };

        public override void InitializeDataStructure()
        {
            if (!Directory.Exists(Strings.DataBasePath)) Directory.CreateDirectory(Strings.DataBasePath);
            if (!File.Exists(Strings.DataBaseStatisticsPath)) SQLiteConnection.CreateFile(Strings.DataBaseStatisticsPath);
            Connection = new SQLiteConnection("Data Source = " + Strings.DataBaseStatisticsPath);
            Connection.Open();
            //create datatables
            foreach (var type in IStatisticData.Implementations)
            {
                if (primaryKeys.TryGetValue(type.GUID, out var primaryKey))
                    CreateDataTable(type, primaryKey, Connection);
                else CreateDataTable(type, Connection);
            }
        }
    }
}
