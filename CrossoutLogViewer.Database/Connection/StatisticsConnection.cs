using CrossoutLogView.Common;
using CrossoutLogView.Database.Reflection;
using CrossoutLogView.Statistics;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
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
        public StatisticsConnection() : base()
        {
            DatabaseTableTypes = IStatisticData.Implementations;
        }

        protected override object InsertVariableHandler(in object obj, in VariableInfo vi)
        {
            //Maps
            if (vi.VariableType.Equals(typeof(Map)))
            {
                var map = (Map)vi.GetValue(obj);
                //request existing map
                var request = GetRequestString(vi.VariableType, true)
                + String.Format(" WHERE {0} == {1}", nameof(Map.Name), SQLiteVariable(map.Name));
                using var cmd = new SQLiteCommand(request, connection);
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
            var request = String.Format(FormatRequest + " WHERE {2} == {3}", RowIdName, nameof(Game), nameof(Game.Map), rowId);
            using var cmd = new SQLiteCommand(request, connection);
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
            using var cmd = new SQLiteCommand(request, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rowIds.Add(ReadField<long>(RowIdName, reader));
            }
            return rowIds;
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
            var request = String.Format(FormatRequest, nameof(User.UserID), nameof(User));
            using var cmd = new SQLiteCommand(request, connection);
            using var reader = cmd.ExecuteReader();
            var userIDs = new List<int>();
            while (reader.Read())
            {
                userIDs.Add(reader.GetInt32(0));
            }
            return userIDs;
        }

        public List<int> RequestUserIDs(long[] rowIDs)
        {
            var conditions = new string[rowIDs.Length];
            var conditionTemplate = RowIdName + " == ";
            for (int i = 0; i < rowIDs.Length; i++)
            {
                conditions[i] = conditionTemplate + rowIDs[i];
            }
            var userIDs = new List<int>();
            var request = String.Concat(String.Format(FormatRequest, nameof(User.UserID), nameof(User)), " WHERE ", String.Join(" OR ", conditions));
            using var cmd = new SQLiteCommand(request, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                userIDs.Add(reader.GetInt32(0));
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

        public List<string> RequestWeaponNames()
        {
            var names = new List<string>();
            var field = nameof(WeaponGlobal.Name).ToLowerInvariant();
            var request = String.Format(FormatRequest, field, nameof(WeaponGlobal));
            using var cmd = new SQLiteCommand(request, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                names.Add(ReadField<string>(field, reader));
            }
            return names;
        }

        public void UpdateUser(User user)
        {
            if (user.UserID <= 0) throw new ArgumentOutOfRangeException(nameof(User.UserID), "Valid UserIDs range from 1 upwards. UserID: " + user.UserID);
            var type = typeof(User);
            var request = GetRequestString(type) + String.Format(" WHERE {0} == {1}",
                nameof(User.UserID).ToLowerInvariant(),
                user.UserID);
            using var cmd = new SQLiteCommand(request, connection);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) //insert new user
            {
                Insert(user);
                return;
            }
            //update existing user            
            var sets = new StringBuilder();
            var varInfos = VariableInfo.FromType(typeof(User));
            foreach (var vi in varInfos)
            {
                if (vi.Name.Equals(nameof(User.UserID), StringComparison.InvariantCulture)
                    || vi.Name.Equals(nameof(User.Name), StringComparison.InvariantCulture))
                    continue;
                var name = vi.Name.ToLowerInvariant();
                //increment numerical stats
                if (vi.VariableType == typeof(double))
                    sets.AppendFormat(FormatEquals, name, ReadField<double>(vi.Name, reader) + (double)vi.GetValue(user));
                else if (vi.VariableType == typeof(int))
                    sets.AppendFormat(FormatEquals, name, ReadField<int>(vi.Name, reader) + (int)vi.GetValue(user));
                else if (GetTableRepresentation(vi.VariableType, DatabaseTableTypes) == TableRepresentation.ReferenceArray)
                {
                    if (vi.Name.Equals(nameof(User.Participations), StringComparison.InvariantCulture)) //globaly unique: find by start&end time make ref
                    {
                        var refs = new StringBuilder();
                        foreach (var game in user.Participations)
                        {
                            var gameReq = String.Format(FormatRequest, RowIdName, nameof(Game)) //request rowid
                                + String.Format(" WHERE {0} == {1}", nameof(Game.Start).ToLowerInvariant(), game.Start.Ticks);
                            using var gameCmd = new SQLiteCommand(gameReq, connection);
                            using var gameReader = gameCmd.ExecuteReader();
                            refs.Append(Strings.ArrayDelimiter);
                            if (gameReader.Read())
                            {
                                refs.Append(Base85.Encode(ReadField<long>(RowIdName, gameReader)));
                            }
                            else
                            {
                                throw new NotImplementedException();
                                //Insert(game);
                                //refs.Append(Strings.Base85EncodeInteger(LastInsertRowId));
                            }
                        }
                        sets.AppendFormat(FormatEquals, name, SQLiteVariable(ReadField<string>(vi.Name, reader) + refs.ToString()));
                    }
                    else //unique per player: increment stats per element
                    {
                        //request existing elements
                        var itemType = Types.GetEnumerableBaseType(vi.VariableType);
                        var references = ReadField<string>(vi.Name, reader);
                        var rowIds = ParseSerializedArray(references, Base85.DecodeInt64);
                        if (rowIds.Length != 0) //update exisitng elements
                        {
                            var conditions = new string[rowIds.Length];
                            for (int i = 0; i < conditions.Length; i++)
                            {
                                conditions[i] = RowIdName + " = " + rowIds[i];
                            }
                            var requestCommand = GetRequestString(itemType, true) + " WHERE " + String.Join(" OR ", conditions);
                            var additions = new List<object>();
                            RequestRowIds.Clear();
                            if (vi.Name.Equals(nameof(User.Weapons), StringComparison.InvariantCulture))
                            {
                                var weapons = ExecuteRequest<Weapon>(requestCommand);
                                Parallel.ForEach(user.Weapons, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                                delegate (Weapon userWeapon)
                                {
                                    var dbWeaponIndex = weapons.FindIndex(x => Strings.NameEquals(x.Name, userWeapon.Name));
                                    if (dbWeaponIndex == -1) //add new weapon
                                    {
                                        lock (additions) additions.Add(userWeapon);
                                    }
                                    else //merge & update existing weapon
                                    {
                                        UpdateValues(weapons[dbWeaponIndex].Merge(userWeapon), VariableInfo.FromType(itemType), itemType.Name,
                                            RowIdName + " == " + RequestRowIds[dbWeaponIndex]);
                                    }
                                });
                            }
                            else if (vi.Name.Equals(nameof(User.Stripes), StringComparison.InvariantCulture))
                            {
                                var stripes = ExecuteRequest<Stripe>(requestCommand);
                                Parallel.ForEach(user.Stripes, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                                delegate (Stripe userStripe)
                                {
                                    var dbStripeIndex = stripes.FindIndex(x => Strings.NameEquals(x.Name, userStripe.Name));
                                    if (dbStripeIndex == -1) //add new stripe
                                    {
                                        lock (additions) additions.Add(userStripe);
                                    }
                                    else //merge & update existing stripe
                                    {
                                        UpdateValues(Stripe.Merge(stripes[dbStripeIndex], userStripe), VariableInfo.FromType(itemType), itemType.Name,
                                            "rowid == " + RequestRowIds[dbStripeIndex]);
                                    }
                                });
                            }
                            sets.AppendFormat(FormatEquals, name, SQLiteVariable(references + UpdateListReferenceHelper(additions, itemType)));
                        }
                        else //element doesnt yet exist for the user
                        {
                            var refs = UpdateListReferenceHelper(user, vi);
                            if (!String.IsNullOrEmpty(refs))
                                sets.AppendFormat(FormatEquals, name, SQLiteVariable(refs.Remove(0, 1)));
                        }
                    }
                }
            }
            //compose update string
            InvokeNonQuery(String.Format(FormatUpdate, nameof(User), sets.Remove(0, 2).ToString(),
                String.Format("{0} == {1}", nameof(User.UserID), user.UserID)),
                connection);
            
        }

        public void UpdateWeaponGlobal(WeaponGlobal weapon)
        {
            var type = typeof(WeaponGlobal);
            var condtion = nameof(WeaponGlobal.Name) + " == " + SQLiteVariable(weapon.Name);
            var request = GetRequestString(type) + " WHERE " + condtion;
            var dbWeapon = ExecuteRequestSingleObject<WeaponGlobal>(request, TableRepresentation.All & ~TableRepresentation.ReferenceArray); //do not request users: TableRepresentation.ReferenceArray
            if (dbWeapon == null)
            {
                Insert(weapon);
                return;
            }
            //increment uses on existing users
            var refs = RequestReferences(nameof(WeaponGlobal.Name) + " == " + SQLiteVariable(weapon.Name), type, nameof(WeaponGlobal.Users));
            var uses = new List<int>();
            for (int i = 0; i < refs.Length; i++)
            {
                var userID = RequestUserId(refs[i]);
                var userIndex = weapon.Users.FindIndex(x => x.UserID == userID);
                if (userIndex == -1)
                {
                    //consume use
                    uses.Add(dbWeapon.Uses[i]);
                }
                else
                {
                    uses.Add(dbWeapon.Uses[i] + weapon.Uses[userIndex]);
                    //consume from weapon
                    weapon.Users.RemoveAt(userIndex);
                    weapon.Uses.RemoveAt(userIndex);
                }
            }
            //get rowid refereces from unconsumed/new users
            var newRefs = new List<long>(refs);
            if (weapon.Users.Count != 0) //no new users
            {
                var condtions = new string[weapon.Users.Count];
                var conditionTemplate = nameof(User.UserID) + " == ";
                for (int i = 0; i < condtions.Length; i++)
                {
                    condtions[i] = conditionTemplate + RequestUserRowId(weapon.Users[i].UserID);
                }
                var rowIdRequest = String.Concat(String.Format(FormatRequest, RowIdName, nameof(User)), " WHERE ", String.Join(" OR ", condtions));
                using (var cmd = new SQLiteCommand(rowIdRequest, connection))
                {
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        newRefs.Add(ReadField<long>(RowIdName, reader));
                    }
                }
            }
            //update existing weapon
            var sets = new StringBuilder();
            sets.Append(nameof(WeaponGlobal.ArmorDamage)).Append(" = ").Append(SQLiteVariable(dbWeapon.ArmorDamage + weapon.ArmorDamage)).Append(", ");
            sets.Append(nameof(WeaponGlobal.CriticalDamage)).Append(" = ").Append(SQLiteVariable(dbWeapon.CriticalDamage + weapon.CriticalDamage)).Append(", ");
            sets.Append(nameof(WeaponGlobal.Users)).Append(" = ").Append(SQLiteVariable(SerializeArray(newRefs, Base85.Encode))).Append(", ");
            sets.Append(nameof(WeaponGlobal.Uses)).Append(" = ").Append(SQLiteVariable(SerializeArray(uses, Base85.Encode)));
            var update = String.Format(FormatUpdate, nameof(WeaponGlobal), sets.ToString(), condtion);
            InvokeNonQuery(update, connection);
        }

        public long RequestUserRowId(int userId)
        {
            var request = String.Concat(String.Format(FormatRequest, RowIdName, nameof(User)), " WHERE ", nameof(User.UserID).ToLowerInvariant(), " == ", userId);
            using var cmd = new SQLiteCommand(request, connection);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return -1;
            return ReadField<long>(RowIdName, reader);
        }

        public int RequestUserId(long rowId)
        {
            if (rowId < 0) return -1;
            var request = String.Concat(String.Format(FormatRequest, nameof(User.UserID), nameof(User)), " WHERE ", RowIdName, " == ", rowId);
            using var cmd = new SQLiteCommand(request, connection);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return -1;
            return reader.GetInt32(0);
        }

        private static Dictionary<long, long> gameRowIdStart = new Dictionary<long, long>();
        public long RequestGameRowId(long startTicks)
        {
            if (gameRowIdStart.TryGetValue(startTicks, out var rowId)) return rowId;
            var request = String.Concat(String.Format(FormatRequest, RowIdName, nameof(Game)), " WHERE ", nameof(Game.Start).ToLowerInvariant(), " == ", startTicks);
            using var cmd = new SQLiteCommand(request, connection);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return -1;
            gameRowIdStart.Add(startTicks, rowId = ReadField<long>(RowIdName, reader));
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
            if (!Directory.Exists(Strings.DataBaseRootPath)) Directory.CreateDirectory(Strings.DataBaseRootPath);
            if (!File.Exists(Strings.DataBaseStatisticsPath)) SQLiteConnection.CreateFile(Strings.DataBaseStatisticsPath);
            connection = new SQLiteConnection("Data Source = " + Strings.DataBaseStatisticsPath);
            Open();
            //create datatables
            foreach (var type in IStatisticData.Implementations)
            {
                if (primaryKeys.TryGetValue(type.GUID, out var primaryKey))
                    CreateDataTable(type, primaryKey, connection);
                else CreateDataTable(type, connection);
            }
            Close();
        }
    }
}
