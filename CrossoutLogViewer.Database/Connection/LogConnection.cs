using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.Database.Reflection;
using CrossoutLogView.Log;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using static CrossoutLogView.Common.SQLiteFormat;

namespace CrossoutLogView.Database.Connection
{
    public class LogConnection : ConnectionBase
    {
        private static string TimeStampFieldName => nameof(ILogEntry.TimeStamp).ToLowerInvariant();

        public LogConnection()
        {
            InitializeDataStructure();
            DatabaseTableTypes = ILogEntry.Implementations;
        }
        protected override object InsertVariableHandler(in object obj, in VariableInfo variableInfo) => null;

        public List<ILogEntry> RequestAll(long start, long end)
        {
            var result = new List<ILogEntry>();
            Parallel.ForEach(DatabaseTableTypes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                delegate (Type t)
            {
                var command = GetDateTimeRangeRequest(t, start, end);
                var partial = ExecuteRequest(t, command).Cast<ILogEntry>();
                lock (result) result.AddRange(partial);
            });
            return result;
        }
        public List<ILogEntry> RequestAll(long start)
        {
            var result = new List<ILogEntry>();
            Parallel.ForEach(DatabaseTableTypes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                delegate (Type t)
            {
                var command = GetDateTimeRangeRequest(t, start);
                var partial = ExecuteRequest(t, command).Cast<ILogEntry>();
                lock (result) result.AddRange(partial);
            });
            return result;
        }

        public IEnumerable<T> Request<T>(long start) where T : ILogEntry, new()
        {
            if (typeof(T).IsInterface) return (IEnumerable<T>)RequestAll(start);
            var command = GetDateTimeRangeRequest(typeof(T), start);
            return ExecuteRequest<T>(command);
        }

        public IEnumerable<T> Request<T>(long start, long end) where T : ILogEntry, new()
        {
            if (typeof(T).IsInterface) return (IEnumerable<T>)RequestAll(start, end);
            var command = GetDateTimeRangeRequest(typeof(T), start, end);
            return ExecuteRequest<T>(command);
        }

        public T RequestLatest<T>() where T : ILogEntry, new()
        {
            var name = nameof(T);
            var command = String.Concat("SELECT a.* FROM ", name, " a INNER JOIN (SELECT ROWID, MAX(timestamp) timestamp FROM ", name, ") b ON a.ROWID == b.ROWID AND a.timestamp = b.timestamp");
            return ExecuteRequestSingleObject<T>(command);
        }

        public long RequestNewestLogEntryTimeStamp()
        {
            var format = "SELECT a.timestamp FROM {0} a INNER JOIN (SELECT ROWID, MAX(timestamp) timestamp FROM {0}) b ON a.ROWID == b.ROWID AND a.timestamp = b.timestamp";
            long result = 0;
            foreach (var type in DatabaseTableTypes)
            {
                var command = String.Format(format, type.Name);
                using var cmd = new SQLiteCommand(command, connection);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) continue;
                var newest = reader.GetInt64(0);
                if (result < newest) result = newest;
            }
            return result;
        }

        public IEnumerable<LogMetadata> RequestMetadata()
        {
            var type = typeof(LogMetadata);
            var command = GetRequestString(type);
            return ExecuteRequest<LogMetadata>(command);
        }

        public void Insert<T>(IEnumerable<T> values) where T : ILogEntry, new()
        {
            InsertMany(values);
        }

        public void InsertMetadata(LogMetadata value)
        {
            Insert(value);
        }

        public void Delete<T>(DateTime start, DateTime end) where T : ILogEntry, new()
        {
            InvokeNonQuery(String.Format(FormatDelete, nameof(T), start.Ticks, end.Ticks), connection);
        }

        private string GetDateTimeRangeRequest(Type type, long start, long end)
        {
            return GetRequestString(type) + String.Format(" WHERE {0} <= {1} AND {1} <= {2}", start, TimeStampFieldName, end);
        }
        private string GetDateTimeRangeRequest(Type type, long start)
        {
            return GetRequestString(type) + String.Format(" WHERE {0} <= {1} ", start, TimeStampFieldName);
        }

        public override void InitializeDataStructure()
        {
            if (!Directory.Exists(Strings.DataBaseRootPath)) Directory.CreateDirectory(Strings.DataBaseRootPath);
            if (!File.Exists(Strings.DataBaseLogPath)) SQLiteConnection.CreateFile(Strings.DataBaseLogPath);
            connection = new SQLiteConnection("Data Source = " + Strings.DataBaseLogPath);
            connection.Open();
            CreateDataTable(typeof(LogMetadata), connection);
            foreach (var t in ILogEntry.Implementations)
            {
                CreateDataTable(t, connection);
            }
        }
    }
}
