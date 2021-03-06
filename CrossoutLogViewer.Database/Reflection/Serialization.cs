﻿using CrossoutLogView.Common;
using CrossoutLogView.Database.Connection;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CrossoutLogView.Database.Reflection
{
    public static class Serialization
    {
        private static ConcurrentDictionary<Guid, TableRepresentation> generatedTableRepresentations = new ConcurrentDictionary<Guid, TableRepresentation>();
        public static TableRepresentation GetTableRepresentation(Type type, Type[] databaseReferenceTypes)
        {
            if (Types.IsGenericIEnumerable(type))
            {
                return GetTableRepresentation(Types.GetEnumerableBaseType(type), databaseReferenceTypes) == TableRepresentation.Store
                    ? TableRepresentation.StoreArray
                    : TableRepresentation.ReferenceArray;
            }
            else
            {
                if (generatedTableRepresentations.TryGetValue(type.GUID, out var rep)) return rep;
                rep = databaseReferenceTypes.Contains(type)
                    ? TableRepresentation.Reference
                    : TableRepresentation.Store;
                generatedTableRepresentations.AddOrUpdate(type.GUID, rep, (guid, r) => rep);
                return rep;
            }
        }

        private static ConcurrentDictionary<Guid, Func<object, string>> generatedSerializers = new ConcurrentDictionary<Guid, Func<object, string>>();
        public static Func<object, string> GetSerializer(Type primitiveType)
        {
            if (generatedSerializers.TryGetValue(primitiveType.GUID, out var func)) return func;
            if (primitiveType == typeof(string)) func = x => (string)x;
            else if (primitiveType == typeof(DateTime)) func = x => ((DateTime)x).Ticks.ToString();
            //use fixed format provider to avoid ',' as decimal separator, it is used as delimiter for array elements.
            else if (primitiveType == typeof(double)) func = x => ((double)x).ToString("R", System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            else if (primitiveType == typeof(float)) func = x => ((float)x).ToString("R", System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            //compress space for integer arrays by using a base 85 encoding instead of base 10.
            else if (primitiveType == typeof(sbyte) || primitiveType == typeof(byte)
                || primitiveType == typeof(short) || primitiveType == typeof(ushort)
                || primitiveType == typeof(int))
                func = x => Base85.Encode(Convert.ToInt32(x));
            else if (primitiveType == typeof(uint)
                || primitiveType == typeof(long))
                func = x => Base85.Encode(Convert.ToInt64(x));
            else throw new InvalidOperationException();
            generatedSerializers.AddOrUpdate(primitiveType.GUID, func, (guid, s) => func);
            return func;
        }
        private static ConcurrentDictionary<Guid, Func<string, object>> generatedDeserializers = new ConcurrentDictionary<Guid, Func<string, object>>();
        public static Func<string, object> GetDeserializer(Type primitiveType)
        {
            if (generatedDeserializers.TryGetValue(primitiveType.GUID, out var func)) return func;
            if (primitiveType == typeof(string)) func = x => x;
            else if (primitiveType == typeof(DateTime)) func = x => new DateTime(Int64.Parse(x));
            else if (primitiveType == typeof(double)) func = x => Double.Parse(x, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            else if (primitiveType == typeof(float)) func = x => Single.Parse(x, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            else if (primitiveType == typeof(sbyte) || primitiveType == typeof(byte)
                || primitiveType == typeof(short) || primitiveType == typeof(ushort)
                || primitiveType == typeof(int))
                func = x => Convert.ChangeType(Base85.DecodeInt32(x), primitiveType);
            else if (primitiveType == typeof(uint)
                || primitiveType == typeof(long))
                func = x => Convert.ChangeType(Base85.DecodeInt64(x), primitiveType);
            else throw new InvalidOperationException();
            generatedDeserializers.AddOrUpdate(primitiveType.GUID, func, (guid, s) => func);
            return func;
        }

        public static Func<string, T> GetDeserializer<T>()
        {
            var deserializer = GetDeserializer(typeof(T));
            return x => (T)deserializer(x);
        }

        public static string SerializeArray(IEnumerable values, Func<object, string> serializer)
        {
            var result = new List<string>();
            foreach (var value in values)
            {
                result.Add(serializer(value));
            }
            return String.Join(Strings.ArrayDelimiter, result);
        }
        public static object[] ParseSerializedArray(string text, Func<string, object> deserializer)
        {
            if (String.IsNullOrWhiteSpace(text)) return Array.Empty<object>();
            var parts = text.Split(Strings.ArrayDelimiter);
            var result = new object[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = deserializer(parts[i]);
            }
            return result;
        }
        public static string SerializeArray<T>(IEnumerable<T> values, Func<T, string> serializer)
        {
            var result = new List<string>();
            foreach (var value in values)
            {
                result.Add(serializer(value));
            }
            return String.Join(Strings.ArrayDelimiter, result);
        }
        public static T[] ParseSerializedArray<T>(string text, Func<string, T> deserializer) where T : IEquatable<T>
        {
            if (String.IsNullOrWhiteSpace(text)) return Array.Empty<T>();
            var parts = text.Split(Strings.ArrayDelimiter);
            var result = new List<T>(parts.Length);
            for (int i = 0; i < parts.Length; i++)
            {
                if (String.IsNullOrEmpty(parts[i]))
                    continue;
                var deserialized = deserializer(parts[i]);
                if (!deserialized.Equals(default(T)))
                    result.Add(deserialized);
            }
            return result.ToArray();
        }

        public static string SQLiteVariable(object value)
        {
            var t = value.GetType();
            return t == typeof(string)
                ? '\'' + (string)value + '\''
                : t == typeof(bool)
                ? ((bool)value == false ? "0" : "1")
                : t == typeof(DateTime)
                ? ((DateTime)value).Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
                : t.IsEnum
                ? ((int)value).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
                : t == typeof(float)
                ? ((float)value).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
                : t == typeof(double)
                ? ((double)value).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
                : t == typeof(int)
                ? ((int)value).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
                : t == typeof(long)
                ? ((long)value).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
                : value.ToString();
        }

        public static string SQLiteDefault(Type type)
        {
            return type == typeof(string) || Types.IsGenericIEnumerable(type)
                ? "''"
                : "0";
        }

        public static bool MaskFilter(TableRepresentation value, TableRepresentation mask) => (value & mask) == value;
    }
}
