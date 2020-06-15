using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.Common
{
    public static class ConcurrentDictionaryExtention
    {
        public static void Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.AddOrUpdate(key, value, (k, oldValue) => value);
        }

        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryRemove(key, out _);
        }
    }
}
