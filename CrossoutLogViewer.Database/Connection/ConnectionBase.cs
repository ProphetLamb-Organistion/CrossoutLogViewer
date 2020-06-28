using CrossoutLogView.Common;
using CrossoutLogView.Database.Reflection;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using static CrossoutLogView.Common.SQLiteFormat;
using static CrossoutLogView.Database.Reflection.Serialization;

namespace CrossoutLogView.Database.Connection
{
    public abstract class ConnectionBase : IDisposable
    {
        private static bool firstLaunch;
        protected ConnectionBase()
        {
            if (!firstLaunch)
            {
                Task.Run(FirstInitialize).Wait();
                firstLaunch = true;
            }
            InitializeConnection();
        }

        /// <summary>
        /// Returns the state of the SQLite Connection.
        /// </summary>
        public ConnectionState State => Connection.State;

        public void Open() => Connection.Open();

        public void Close() => Connection.Close();

        public long LastInsertRowId => Connection.LastInsertRowId;

        public ConcurrentBag<long> RequestRowIds { get; } = new ConcurrentBag<long>();

        protected SQLiteConnection Connection { get; set; }

        public Type[] DatabaseTableTypes { get; protected set; }

        public SQLiteTransaction BeginTransaction()
        {
            return Connection.BeginTransaction();
        }

        public async ValueTask<IDbTransaction> BeginTransactionAsync()
        {
            return await Connection.BeginTransactionAsync();
        }

        /// <summary>
        /// Creates the database, and the tables (if not already exisiting).
        /// </summary>
        public abstract void InitializeConnection();

        /// <summary>
        /// Runs actions only required on first initialization of a instance.
        /// </summary>
        public abstract Task FirstInitialize();

        /// <summary>
        /// Invokes any SQLite command via the provided Connection.
        /// </summary>
        /// <param name="command">The command sent to the <see cref="SQLiteConnection"/>.</param>
        /// <param name="con"></param>
        protected static void InvokeNonQuery(string command, SQLiteConnection con)
        {
            try
            {
                using var cmd = new SQLiteCommand(command, con);
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException) { }
        }

        /// <summary>
        /// Creates a datatable based on a given type using a provided <see cref="SQLiteConnection"/>.
        /// </summary>
        /// <param name="type">Type of the table.</param>
        /// <param name="con"></param>
        protected static void CreateDataTable(Type type, SQLiteConnection con)
        {
            if (type is null || con is null) return;
            var varInfos = VariableInfo.FromType(type);
            var definition = new string[varInfos.Length];
            int i = 0;
            foreach (var vi in varInfos)
            {
                definition[i++] = String.Format(FormatCreateTableField,
                    vi.Name.ToLowerInvariant(),
                    SQLiteType(vi.VariableType));
            }
            Array.Sort(definition);
            InvokeNonQuery(String.Format(FormatCreateTable, type.Name, String.Join(", ", definition)), con);
        }

        /// <summary>
        /// Creates a datatable based on a given type using a provided <see cref="SQLiteConnection"/>.
        /// </summary>
        /// <param name="type">Type of the table.</param>
        /// <param name="primaryKey">The primary key.</param>
        /// <param name="con"></param>
        protected static void CreateDataTable(Type type, string primaryKey, SQLiteConnection con)
        {
            if (type is null || primaryKey is null || con is null) return;
            var varInfos = VariableInfo.FromType(type);
            var definition = new string[varInfos.Length];
            int i = 0;
            foreach (var vi in varInfos)
            {
                definition[i++] = String.Format(FormatCreateTableField,
                    vi.Name.ToLowerInvariant(),
                    Strings.NameEquals(vi.Name, primaryKey)
                    ? PrimaryKeyDefintion
                    : SQLiteType(vi.VariableType));
            }
            Array.Sort(definition);
            InvokeNonQuery(String.Format(FormatCreateTable, type.Name, String.Join(", ", definition)), con);
        }

        #region REQUEST
        /// <summary>
        /// Returns data from the <see cref="SQLiteConnection"/> using the provided command.
        /// </summary>
        /// <typeparam name="T">Indicates the return type, and which table should be accessed.</typeparam>
        /// <param name="requestCommand">The REQUEST command sent to the SQLite database.</param>
        /// <param name="filterTableRepresentation">Masks the <see cref="TableRepresentation"/> of each variable.</param>
        /// <returns>All items in the SQLite database fullfilling the conditions of the REQUEST.</returns>
        protected List<T> ExecuteRequest<T>(string requestCommand, TableRepresentation filterTableRepresentation = TableRepresentation.All) where T : new()
        {
            var type = typeof(T);
            var result = new List<T>();
            try
            {
                var varInfos = VariableInfo.FromType(type);
                using var cmd = new SQLiteCommand(requestCommand, Connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var obj = (T)ReadCurrentObject(() => new T(), varInfos, reader, filterTableRepresentation);
                    if (obj != null) result.Add(obj);
                }
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (SQLiteException) { }
            return result;
        }

        /// <summary>
        /// Returns data from the <see cref="SQLiteConnection"/> using the provided command.
        /// </summary>
        /// <param name="type">Indicates the return type, and which table should be accessed.</param>
        /// <param name="requestCommand">The REQUEST command sent to the SQLite database.</param>
        /// <param name="filterTableRepresentation">Masks the <see cref="TableRepresentation"/> of each variable.</param>
        /// <returns>All items in the SQLite database fullfilling the conditions of the REQUEST.</returns>
        protected IList ExecuteRequest(Type type, string requestCommand, TableRepresentation filterTableRepresentation = TableRepresentation.All)
        {
            var varInfos = VariableInfo.FromType(type);
            var result = new List<object>();
            try
            {
                using var cmd = new SQLiteCommand(requestCommand, Connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var obj = ReadCurrentObject(() => Activator.CreateInstance(type), varInfos, reader, filterTableRepresentation);
                    if (obj != null) result.Add(obj);
                }
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (SQLiteException) { }
            return result;
        }

        /// <summary>
        /// Returns the first dataset from the <see cref="SQLiteConnection"/> using the provided command.
        /// </summary>
        /// <typeparam name="T">Indicates the return type, and which table should be accessed.</typeparam>
        /// <param name="requestCommand">The REQUEST command sent to the SQLite database.</param>
        /// <param name="filterTableRepresentation">Masks the <see cref="TableRepresentation"/> of each variable.</param>
        /// <returns>The first item in the SQLite database fullfilling the conditions of the REQUEST.</returns>
        protected T ExecuteRequestSingleObject<T>(string requestCommand, TableRepresentation filterTableRepresentation = TableRepresentation.All) where T : new()
        {
            var type = typeof(T);
            var varInfos = VariableInfo.FromType(type);
            T result = default;
            try
            {
                using var cmd = new SQLiteCommand(requestCommand, Connection);
                using var reader = cmd.ExecuteReader();
                result = reader.Read()
                    ? (T)ReadCurrentObject(() => new T(), varInfos, reader, filterTableRepresentation)
                    : default;
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (SQLiteException) { }
            return result;
        }

        /// <summary>
        /// Returns the first dataset from the <see cref="SQLiteConnection"/> using the provided command.
        /// </summary>
        /// <param name="type">Indicates the return type, and which table should be accessed.</param>
        /// <param name="requestCommand">The REQUEST command sent to the SQLite database.</param>
        /// <param name="con">The <see cref="SQLiteConnection"/> used.</param>
        /// <param name="filterTableRepresentation">Masks the <see cref="TableRepresentation"/> of each variable.</param>
        /// <returns>The first item in the SQLite database fullfilling the conditions of the REQUEST.</returns>
        protected object ExecuteRequestSingleObject(Type type, string requestCommand, TableRepresentation filterTableRepresentation = TableRepresentation.All)
        {
            var varInfos = VariableInfo.FromType(type);
            object result = null;
            try
            {
                using var cmd = new SQLiteCommand(requestCommand, Connection);
                using var reader = cmd.ExecuteReader();
                result = reader.Read()
                    ? ReadCurrentObject(() => Activator.CreateInstance(type), varInfos, reader, filterTableRepresentation)
                    : null;
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (SQLiteException) { }
            return result;
        }

        /// <summary>
        /// Returns the double array of rowid references by the field name. 
        /// Field must have <see cref="TableRepresentation.Reference"/> or <see cref="TableRepresentation.ReferenceArray"/>.
        /// </summary>
        /// <param name="type">The type indicating the table.</param>
        /// <param name="variableName">The name of the field.</param>
        /// <returns>The double array of rowid references by the field name. Null if the field is not found or the table representation is invalid.</returns>
        protected long[] RequestReferences(string condition, Type type, string variableName)
        {
            if (condition is null || type is null || variableName is null) return Array.Empty<long>();
            var request = String.Format(FormatRequestWhere, variableName.ToLowerInvariant(), type.Name, condition);
            long[] refs = null;
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return Array.Empty<long>();
                var vi = VariableInfo.FromType(type).FirstOrDefault(x => x.Name.Equals(variableName, StringComparison.InvariantCultureIgnoreCase));
                if (vi == null) return Array.Empty<long>();
                refs = (GetTableRepresentation(vi.VariableType, DatabaseTableTypes)) switch
                {
                    TableRepresentation.Reference => new long[] { reader.GetInt64(0) },
                    TableRepresentation.ReferenceArray => ParseSerializedArray(reader.GetString(0), Base85.DecodeInt64),
                    _ => null
                };
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (SQLiteException) { }
            return refs ?? Array.Empty<long>();
        }

        protected T[] RequestStoredArray<T>(string condition, Type type, string variableName) where T : IEquatable<T>
        {
            if (condition is null || type is null || variableName is null) return Array.Empty<T>();
            var request = String.Format(FormatRequestWhere, variableName.ToLowerInvariant(), type.Name, condition);
            T[] result = null;
            try
            {
                using var cmd = new SQLiteCommand(request, Connection);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return Array.Empty<T>();
                result = ParseSerializedArray(reader.GetString(0), GetDeserializer<T>());
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (SQLiteException) { }
            return result ?? Array.Empty<T>();
        }

        protected static object ReadField(VariableInfo variableInfo, SQLiteDataReader reader)
        {
            if (variableInfo is null || reader is null) return default;
            var type = variableInfo.VariableType;
            object value = null;
            try
            {
                var col = reader.GetOrdinal(variableInfo.Name.ToLowerInvariant());
                if (col != -1)
                {
                    value = type == typeof(byte) ? reader.GetByte(col)
                        : type == typeof(short) ? reader.GetInt16(col)
                        : type == typeof(int) ? reader.GetInt32(col)
                        : type == typeof(long) ? reader.GetInt64(col)
                        : type == typeof(string) ? reader.GetString(col)
                        : type == typeof(DateTime) ? new DateTime(reader.GetInt64(col))
                        : type.IsEnum ? Enum.Parse(type, reader.GetInt32(col).ToString(CultureInfo.InvariantCulture))
                        : Convert.ChangeType(reader.GetValue(col), type, CultureInfo.InvariantCulture);
                }
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (InvalidCastException) { }
            return value;
        }

        protected static T ReadField<T>(string name, SQLiteDataReader reader)
        {
            if (name is null || reader is null) return default;
            var type = typeof(T);
            T value = default;
            try
            {
                var col = reader.GetOrdinal(name.ToLowerInvariant());
                if (col != -1)
                {
                    value = Types.CastObject<T>(type == typeof(byte) ? reader.GetByte(col)
                    : type == typeof(short) ? reader.GetInt16(col)
                    : type == typeof(int) ? reader.GetInt32(col)
                    : type == typeof(long) ? reader.GetInt64(col)
                    : type == typeof(string) ? reader.GetString(col)
                    : type == typeof(DateTime) ? new DateTime(reader.GetInt64(col))
                    : type.IsEnum ? Enum.Parse(type, reader.GetString(col))
                    : Convert.ChangeType(reader.GetValue(col), type, CultureInfo.InvariantCulture));
                }
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (InvalidCastException) { }
            return value;
        }

        protected object ReadCurrentObject(Func<object> constructor, VariableInfo[] fields, SQLiteDataReader reader, TableRepresentation filterTableRepresentations)
        {
            if (constructor is null || fields is null || reader is null) return null;
            var obj = constructor();
            int rowIdIndex = reader.GetOrdinal(RowIdName);
            if (rowIdIndex != -1)
            {
                RequestRowIds.Add(reader.GetInt64(rowIdIndex));
            }
            foreach (var vi in fields)
            {
                if (reader.GetOrdinal(vi.Name.ToLowerInvariant()) == -1) //field not requested; skip
                    continue;
                var varType = vi.VariableType;
                var varTableRepresentation = GetTableRepresentation(varType, DatabaseTableTypes);
                if (!MaskFilter(varTableRepresentation, filterTableRepresentations))  //fitler doesnt allows variable; skip
                    continue;
                switch (varTableRepresentation)
                {
                    case TableRepresentation.Store:
                        vi.SetValue(obj, ReadField(vi, reader));
                        break;
                    //class that has own talbe: stored rowid as long 
                    case TableRepresentation.Reference:
                        //obtain rowid reference from reader
                        var reference = ReadField<long>(vi.Name, reader);
                        vi.SetValue(obj, ExecuteRequestSingleObject(varType, GetRowIdRequestString(varType, reference)));
                        break;
                    case TableRepresentation.StoreArray:
                        var baseType = Types.GetEnumerableBaseType(varType);
                        if (baseType == null)
                            continue;
                        object[] values = ParseSerializedArray(ReadField<string>(vi.Name, reader), GetDeserializer(baseType));
                        vi.SetValue(obj, Generics.CastEnumerable(varType, baseType, values));
                        break;
                    //stored rowid references in text
                    case TableRepresentation.ReferenceArray:
                        //obtain rowid references from database
                        baseType = Types.GetEnumerableBaseType(varType);
                        if (baseType == null)
                            continue;
                        long[] refs = ParseSerializedArray(ReadField<string>(vi.Name, reader), Base85.DecodeInt64);
                        //request & store each item referenced
                        var items = new object[refs.Length];
                        for (int i = 0; i < refs.Length; i++)
                        {
                            items[i] = ExecuteRequestSingleObject(baseType, GetRowIdRequestString(baseType, refs[i]));
                        }
                        vi.SetValue(obj, Generics.CastEnumerable(varType, baseType, items));
                        break;
                    default:
                        throw new InvalidOperationException("Value of TableRepresentation is invalid.");
                }
            }
            return obj;
        }

        private static ConcurrentDictionary<(Guid GUID, TableRepresentation Included), string> requestVariableInfos = new ConcurrentDictionary<(Guid GUID, TableRepresentation Incuded), string>();
        /// <summary>
        /// Returns the SQLite REQUEST string for a given type, requesting only reference and stored fields, never arrays or refernce arrays.
        /// </summary>
        /// <param name="type">The type of the REQUEST.</param>
        /// <param name="includeRowId">If rowids should be requested.</param>
        /// <param name="includedTableRepresentation">Mask used to dettermine which varialbes to incude in the request.</param>
        /// <returns>The SQlite REQUEST string for a given type.</returns>
        protected string GetRequestString(Type type, bool includeRowId = false, TableRepresentation includedTableRepresentation = TableRepresentation.All)
        {
            if (type is null) return null;
            if (!requestVariableInfos.TryGetValue((type.GUID, includedTableRepresentation), out var varInfoString))
            {
                varInfoString = String.Join(", ", includedTableRepresentation == TableRepresentation.All
                    ? VariableInfo.FromType(type).Select(x => x.Name.ToLowerInvariant())
                    : VariableInfo.FromType(type).Where(x => MaskFilter(GetTableRepresentation(x.VariableType, DatabaseTableTypes), includedTableRepresentation)).Select(x => x.Name.ToLowerInvariant()));
                requestVariableInfos.AddOrUpdate((type.GUID, includedTableRepresentation), varInfoString,
                    ((Guid, TableRepresentation) key, string value) => value);
            }
            return String.Format(FormatRequest, (includeRowId ? RowIdName + ", " : String.Empty) + varInfoString, type.Name);
        }

        /// <summary>
        /// Returns the SQlite REQUEST string for a given type, with rowid conditions.
        /// </summary>
        /// <param name="type">The type of the REQUEST.</param>
        /// <param name="rowIds">The rowids for the REQEUSTs where condition.</param>
        /// <param name="includedTableRepresentation">Mask used to dettermine which varialbes to incude in the request.</param>
        /// <returns>The SQlite REQUEST string for a given type, with rowid conditions.</returns>
        protected string GetRowIdRequestString(Type type, long[] rowIds, TableRepresentation includedTableRepresentation = TableRepresentation.All)
        {
            if (type is null || rowIds is null) return null;
            var conditions = new string[rowIds.Length];
            var reqTemplate = RowIdName + " == {0}";
            for (int i = 0; i < rowIds.Length; i++)
            {
                conditions[i] = String.Format(reqTemplate, rowIds[i]);
            }
            return String.Concat(GetRequestString(type, false, includedTableRepresentation), " WHERE ", String.Join(" OR ", conditions));
        }

        /// <summary>
        /// Returns the SQlite REQUEST string for a given type, with a rowid condition.
        /// </summary>
        /// <param name="type">The type of the REQUEST.</param>
        /// <param name="rowId">The rowid for the REQEUSTs where condition.</param>
        /// <returns>The SQlite REQUEST string for a given type, with a rowid condition.</returns>
        protected string GetRowIdRequestString(Type type, long rowId, TableRepresentation includedTableRepresentation = TableRepresentation.All)
        {
            return GetRequestString(type, false, includedTableRepresentation) + String.Format(" WHERE {0} == {1}", RowIdName, rowId);
        }

        #endregion REQUEST

        #region INSERT
        /// <summary>
        /// INSERTs a provided object of the type <typeparamref name="T"/> into the database using the <see cref="SQLiteConnection"/>.
        /// </summary>
        /// <typeparam name="T">Indicates the type of the object, and into which table it will be INSERTed.</typeparam>
        /// <param name="obj">The object that will be INSERTed into the database.</param>
        /// <param name="filterTableRepresentation">Masks the <see cref="TableRepresentation"/> of each variable.</param>
        protected void Insert<T>(T obj, TableRepresentation filterTableRepresentation = TableRepresentation.All) where T : new()
        {
            var type = typeof(T);
            var membData = MemberDefinitionData.FromType(type);
            InvokeNonQuery(String.Format(FormatInsert,
                type.Name,
                membData.Header,
                GenerateInsertValuesString(obj, membData.VariableInfos, filterTableRepresentation)),
                Connection);
        }
        /// <summary>
        /// INSERTs a provided object of the <typeparamref name="type"/> into the database using the <see cref="SQLiteConnection"/>.
        /// </summary>
        /// <param name="obj">The object that will be INSERTed into the database.</param>
        /// <param name="type">Indicates the type of the object, and into which table it will be INSERTed.</param>
        /// <param name="filterTableRepresentation">Masks the <see cref="TableRepresentation"/> of each variable.</param>
        protected void Insert(object obj, Type type, TableRepresentation filterTableRepresentation = TableRepresentation.All)
        {
            if (obj is null || type is null) return;
            var membData = MemberDefinitionData.FromType(type);
            InvokeNonQuery(String.Format(FormatInsert,
                type.Name,
                membData.Header,
                GenerateInsertValuesString(obj, membData.VariableInfos, filterTableRepresentation)),
                Connection);
        }
        /// <summary>
        /// INSERTs provided objects of the type <typeparamref name="T"/> into the database using the <see cref="SQLiteConnection"/>.
        /// </summary>
        /// <typeparam name="T">Indicates the type of the object, and into which table it will be INSERTed.</typeparam>
        /// <param name="objects">The objects that will be INSERTed into the database.</param>
        /// <param name="filterTableRepresentation">Masks the <see cref="TableRepresentation"/> of each variable.</param>
        /// <returns>The number of insertions.</returns>
        protected int InsertMany<T>(IEnumerable<T> objects, TableRepresentation filterTableRepresentation = TableRepresentation.All)
        {
            if (objects is null) return 0;
            var type = typeof(T);
            var membData = MemberDefinitionData.FromType(type);
            var values = new List<string>();
            var count = 0;
            foreach (var obj in objects)
            {
                values.Add(GenerateInsertValuesString(obj, membData.VariableInfos, filterTableRepresentation));
                count++;
            }
            if (count == 0) return 0;
            InvokeNonQuery(String.Format(FormatInsert,
                type.Name,
                membData.Header,
                String.Join(", ", values)),
                Connection);
            return count;
        }
        /// <summary>
        /// INSERTs provided objects of the type <typeparamref name="type"/> into the database using the <see cref="SQLiteConnection"/>.
        /// </summary>
        /// <param name="objects">The objects that will be inserted into the database.</param>
        /// <param name="type">Indicates the type of the object, and into which table it will be inserted.</param>
        /// <param name="filterTableRepresentation">Masks the <see cref="TableRepresentation"/> of each variable.</param>
        /// <returns>The number of insertions.</returns>
        protected int InsertMany(IEnumerable objects, Type type, TableRepresentation filterTableRepresentation = TableRepresentation.All)
        {
            if (objects is null || type is null) return 0;
            var membData = MemberDefinitionData.FromType(type);
            var values = new List<string>();
            var count = 0;
            foreach (var obj in objects)
            {
                values.Add(GenerateInsertValuesString(obj, membData.VariableInfos, filterTableRepresentation));
                count++;
            }
            if (count == 0) return 0;
            InvokeNonQuery(String.Format(FormatInsert,
                type.Name,
                membData.Header,
                String.Join(", ", values)),
                Connection);
            return count;
        }

        private string ReferenceFromInsertEnumerable(IEnumerable objects, Type type)
        {
            var refs = new List<long>();
            var baseType = Types.GetEnumerableBaseType(type);
            if (baseType == null)
                return null;
            //insert each item in enumerable & populate reference list
            foreach (var item in objects)
            {
                Insert(item, baseType);
                refs.Add(LastInsertRowId);
            }
            return SerializeArray(refs, Base85.Encode);
        }

        /// <summary>
        /// Handles specific variables where the default behaviour is not appropriate. If the return value is <see cref="null"/>, the default behaviour will be executed, otherwise the the returned <see cref="Object"/> is handled as the value of the variable in context of SQLite database.
        /// The returned object must be a primitive datatype (integer, floating point, <see cref="String"/>), a <see cref="DateTime"/> or <see cref="null"/>.
        /// </summary>
        /// <param name="obj">The instance of the variable.<param>
        /// <param name="variableInfo">The <see cref="VariableInfo"/> describing the variable.</param>
        /// <returns><see cref="null"/> if the default behaviour should be executed, otherwise the <see cref="Object"/> represensting the value of the variable in context of SQLite database.</returns>
        protected abstract object InsertVariableHandler(in object obj, in VariableInfo variableInfo);

        private string GenerateInsertValuesString(object obj, VariableInfo[] varInfos, TableRepresentation filterTableRepresentation)
        {
            var values = new string[varInfos.Length];
            int i = 0;
            foreach (var vi in varInfos)
            {
                var type = vi.VariableType;
                object current;
                if ((current = InsertVariableHandler(obj, vi)) != null)
                {
                    values[i++] = SQLiteVariable(current); //custom variable hander applies
                    continue;
                }
                var varTableRepresentation = GetTableRepresentation(type, DatabaseTableTypes);
                if (!MaskFilter(varTableRepresentation, filterTableRepresentation))
                {
                    values[i++] = SQLiteDefault(type);
                    continue;
                }
                else switch (varTableRepresentation)
                    {
                        case TableRepresentation.Store:
                            current = vi.GetValue(obj);
                            break;
                        case TableRepresentation.Reference:
                            Insert(vi.GetValue(obj), type);
                            current = Connection.LastInsertRowId;
                            break;
                        case TableRepresentation.StoreArray:
                            current = SerializeArray(vi.GetValue(obj) as IEnumerable, GetSerializer(Types.GetEnumerableBaseType(vi.VariableType)));
                            break;
                        case TableRepresentation.ReferenceArray:
                            current = ReferenceFromInsertEnumerable(vi.GetValue(obj) as IEnumerable, type);
                            break;
                    }
                values[i++] = SQLiteVariable(current);
            }
            return '(' + String.Join(", ", values) + ')';
        }
        #endregion INSERT

        #region UPDATE
        protected void UpdateValues(object obj, VariableInfo[] variableInfos, string tableName, string whereCondition)
        {
            if (obj is null || variableInfos is null || tableName is null || whereCondition is null) return;
            var sets = new string[variableInfos.Length];
            for (int i = 0; i < variableInfos.Length; i++)
            {
                var rep = GetTableRepresentation(variableInfos[i].VariableType, DatabaseTableTypes);
                object value;
                if (rep == TableRepresentation.Reference)
                {
                    Insert(variableInfos[i].GetValue(obj), variableInfos[i].VariableType);
                    value = LastInsertRowId;
                }
                else if (rep == TableRepresentation.Store)
                {
                    value = variableInfos[i].GetValue(obj);
                }
                else throw new NotSupportedException();
                sets[i] = variableInfos[i].Name.ToLowerInvariant() + " = " + SQLiteVariable(value);
            }
            var update = String.Format(FormatUpdate, tableName, String.Join(", ", sets), whereCondition);
            InvokeNonQuery(update, Connection);
        }
        #endregion UPDATE

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Connection.Close();
                    Connection.Dispose();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
