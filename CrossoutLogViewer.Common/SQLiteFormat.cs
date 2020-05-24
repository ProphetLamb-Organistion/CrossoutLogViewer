using System;

namespace CrossoutLogView.Common
{
    public static class SQLiteFormat
    {
        public const string FormatRequest = @"SELECT {0} FROM {1}";
        public const string FormatRequestWhere = @"SELECT {0} FROM {1} WHERE {2}";
        public const string FormatDataTimeStampRequest = FormatRequest + @"WHERE timestamp >= {2} AND timestamp <= {3}";
        public const string FormatInsert = @"INSERT INTO {0} ({1}) VALUES {2}";
        public const string FormatDelete = @"DELETE FROM {0} WHERE timestamp >= {1} AND timestamp <= {2}";
        public const string FormatTrim = @"DELETE FROM {0} WHERE timestamp < {1}";
        public const string FormatUpdate = @"UPDATE {0} SET {1} WHERE {2}";

        public const string FormatEquals = ", {0} = {1}";

        public const string FormatCreateTable = @"CREATE TABLE IF NOT EXISTS [{0}] ({1})";
        public const string FormatCreateTableField = @"[{0}] {1} ";

        public const string RowIdName = "rowid";
        public const string PrimaryKeyDefintion = "PRIMARY KEY NOT NULL";

        public static string SQLiteType(Type type)
        {
            return type == null
                ? String.Empty
                : Types.IsGenericIEnumerable(type)
                ? "TEXT NULL"
                : type.IsEnum
                ? "INT DEFAULT 0"
                : type == typeof(string)
                ? "TEXT NULL"
                : type == typeof(float)
                ? "FLOAT DEFAULT 0"
                : type == typeof(double)
                ? "DOUBLE DEFAULT 0"
                : type == typeof(long) || type == typeof(DateTime)
                ? "BIGINT DEFAULT 0"
                : type == typeof(int)
                ? "INT DEFAULT 0"
                : type == typeof(short)
                ? "SMALLINT DEFAULT 0"
                : type == typeof(bool) || type == typeof(byte)
                ? "TINYINT DEFAULT 0"
                : "BLOB NULL";
        }
    }
}
