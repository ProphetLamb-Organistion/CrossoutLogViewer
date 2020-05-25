using CrossoutLogView.Common;
using CrossoutLogView.Database.Reflection;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

using static CrossoutLogView.Common.SQLiteFormat;
using static CrossoutLogView.Database.Reflection.Serialization;

namespace CrossoutLogView.Database.Connection
{
    public abstract class ConnectionBase : IDisposable
    {
        protected SQLiteConnection connection;
        protected Type[] DatabaseTableTypes;
        
        /// <summary>
        /// Returns the state of the SQLite connection.
        /// </summary>
        public ConnectionState State => connection.State;

        public void Open() => connection.Open();

        public void Close() => connection.Close();

        public long LastInsertRowId => connection.LastInsertRowId;

        public ConcurrentBag<long> RequestRowIds { get; } = new ConcurrentBag<long>();

        public SQLiteTransaction BeginTransaction()
        {
            return connection.BeginTransaction();
        }

        public async ValueTask<IDbTransaction> BeginTransactionAsync()
        {
            return await connection.BeginTransactionAsync();
        }

        /// <summary>
        /// Creates the database, and the tables (if not already exisiting).
        /// </summary>
        public abstract void InitializeDataStructure();

        /// <summary>
        /// Invokes any SQLite command via the provided connection.
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
            catch (SQLiteException ex)
            {
                Logging.WriteLine(ex);
            }
        }

        /// <summary>
        /// Creates a datatable based on a given type using a provided <see cref="SQLiteConnection"/>.
        /// </summary>
        /// <param name="type">Type of the table.</param>
        /// <param name="con"></param>
        protected static void CreateDataTable(Type type, SQLiteConnection con)
        {
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
            var varInfos = VariableInfo.FromType(type);
            var result = new List<T>();
            using var cmd = new SQLiteCommand(requestCommand, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add((T)ReadCurrentObject(() => new T(), varInfos, reader, filterTableRepresentation));
            }
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
            using var cmd = new SQLiteCommand(requestCommand, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(ReadCurrentObject(() => Activator.CreateInstance(type), varInfos, reader, filterTableRepresentation));
            }
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
            using var cmd = new SQLiteCommand(requestCommand, connection);
            using var reader = cmd.ExecuteReader();
            return !reader.Read() ? default
                : (T)ReadCurrentObject(() => new T(), varInfos, reader, filterTableRepresentation);
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
            using var cmd = new SQLiteCommand(requestCommand, connection);
            using var reader = cmd.ExecuteReader();
            return !reader.Read() ? null
                : ReadCurrentObject(() => Activator.CreateInstance(type), varInfos, reader, filterTableRepresentation);
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
            var request = String.Format(FormatRequestWhere, variableName.ToLowerInvariant(), type.Name, condition);
            using var cmd = new SQLiteCommand(request, connection);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return Array.Empty<long>();
            var vi = VariableInfo.FromType(type).FirstOrDefault(x => x.Name.Equals(variableName, StringComparison.InvariantCultureIgnoreCase));
            if (vi == null) return Array.Empty<long>();
            return (GetTableRepresentation(vi.VariableType, DatabaseTableTypes)) switch
            {
                TableRepresentation.Reference => new long[] { reader.GetInt64(0) },
                TableRepresentation.ReferenceArray => ParseSerializedArray(reader.GetString(0), Base85.DecodeInt64),
                _ => Array.Empty<long>()
            };
        }

        protected T[] RequestStoredArray<T>(string condition, Type type, string variableName)
        {
            var request = String.Format(FormatRequestWhere, variableName.ToLowerInvariant(), type.Name, condition);
            using var cmd = new SQLiteCommand(request, connection);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return Array.Empty<T>();
            return ParseSerializedArray(reader.GetString(0), GetDeserializer<T>());
        }

        protected static object ReadField(VariableInfo variableInfo, SQLiteDataReader reader)
        {
            var type = variableInfo.VariableType;
            var col = reader.GetOrdinal(variableInfo.Name.ToLowerInvariant());
            return type == typeof(byte) ? reader.GetByte(col)
                : type == typeof(short) ? reader.GetInt16(col)
                : type == typeof(int) ? reader.GetInt32(col)
                : type == typeof(long) ? reader.GetInt64(col)
                : type == typeof(string) ? reader.GetString(col)
                : type == typeof(DateTime) ? new DateTime(reader.GetInt64(col))
                : type.IsEnum ? Enum.Parse(type, reader.GetInt32(col).ToString())
                : Convert.ChangeType(reader.GetValue(col), type);
        }

        protected static T ReadField<T>(string name, SQLiteDataReader reader)
        {
            var type = typeof(T);
            var col = reader.GetOrdinal(name.ToLowerInvariant());
            if (col == -1) return default;
            return Types.CastObject<T>(type == typeof(byte) ? reader.GetByte(col)
                : type == typeof(short) ? reader.GetInt16(col)
                : type == typeof(int) ? reader.GetInt32(col)
                : type == typeof(long) ? reader.GetInt64(col)
                : type == typeof(string) ? reader.GetString(col)
                : type == typeof(DateTime) ? new DateTime(reader.GetInt64(col))
                : type.IsEnum ? Enum.Parse(type, reader.GetString(col))
                : Convert.ChangeType(reader.GetValue(col), type));
        }

        protected object ReadCurrentObject(Func<object> constructor, VariableInfo[] fields, SQLiteDataReader reader, TableRepresentation filterTableRepresentations)
        {
            var obj = constructor();
            int rowIdIndex = reader.GetOrdinal(RowIdName);
            if (rowIdIndex != -1)
            {
                RequestRowIds.Add(reader.GetInt64(rowIdIndex));
            }
            foreach (var vi in fields)
            {
                if (reader.GetOrdinal(vi.Name.ToLowerInvariant()) == -1) //field not requested
                {
                    continue;
                }
                var varType = vi.VariableType;
                var varTableRepresentation = GetTableRepresentation(varType, DatabaseTableTypes);
                if (!MaskFilter(varTableRepresentation, filterTableRepresentations)) continue; //fitler doesnt allows variable
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
                        object[] values = ParseSerializedArray(ReadField<string>(vi.Name, reader), GetDeserializer(baseType));
                        vi.SetValue(obj, Generics.CastEnumerable(varType, baseType, values));
                        break;
                    //stored rowid references in text
                    case TableRepresentation.ReferenceArray:
                        //obtain rowid references from database
                        baseType = Types.GetEnumerableBaseType(varType);
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
                connection);
        }
        /// <summary>
        /// INSERTs a provided object of the <typeparamref name="type"/> into the database using the <see cref="SQLiteConnection"/>.
        /// </summary>
        /// <param name="obj">The object that will be INSERTed into the database.</param>
        /// <param name="type">Indicates the type of the object, and into which table it will be INSERTed.</param>
        /// <param name="filterTableRepresentation">Masks the <see cref="TableRepresentation"/> of each variable.</param>
        protected void Insert(object obj, Type type, TableRepresentation filterTableRepresentation = TableRepresentation.All)
        {
            var membData = MemberDefinitionData.FromType(type);
            InvokeNonQuery(String.Format(FormatInsert,
                type.Name,
                membData.Header,
                GenerateInsertValuesString(obj, membData.VariableInfos, filterTableRepresentation)),
                connection);
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
                connection);
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
                connection);
            return count;
        }

        private string ReferenceFromInsertEnumerable(IEnumerable objects, Type type)
        {
            var refs = new List<long>();
            var baseType = Types.GetEnumerableBaseType(type);
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
                            current = connection.LastInsertRowId;
                            break;
                        case TableRepresentation.StoreArray:
                            current = SerializeArray(vi.GetValue(obj) as IEnumerable, GetSerializer(Types.GetEnumerableBaseType(vi.VariableType)));
                            break;
                        case TableRepresentation.ReferenceArray:
                            current = ReferenceFromInsertEnumerable(vi.GetValue(obj) as IEnumerable, type);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                values[i++] = SQLiteVariable(current);
            }
            return '(' + String.Join(", ", values) + ')';
        }
        #endregion INSERT

        #region UPDATE
        protected void UpdateValues(object obj, VariableInfo[] variableInfos, string tableName, string whereCondition)
        {
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
            InvokeNonQuery(update, connection);
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
                    connection.Dispose();
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
