using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace CrossoutLogView.Database.Reflection
{
    public static class Generics
    {
        public static object CastEnumerable(Type type, Type itemType, IEnumerable items)
        {
            var ienumerable = GetEnumerableCast(itemType).Invoke(null, new object[] { items }); //items.Cast<itemType>
            if (typeof(IList).IsAssignableFrom(type))
            {
                return GetGenericToList(itemType).Invoke(null, new[] { ienumerable }); //ienumerable.ToList<itemType>
            }
            if (typeof(Array).IsAssignableFrom(type))
            {
                return GetGenericToArray(itemType).Invoke(null, new[] { ienumerable });//ienumerable.ToArray<itemType>
            }
            return ienumerable;
        }

        private static ConcurrentDictionary<Guid, MethodInfo> generatedGernericIEnumerableCast = new ConcurrentDictionary<Guid, MethodInfo>();
        public static MethodInfo GetEnumerableCast(Type itemType)
        {
            //try to get cached or make generic cast method for the type
            if (!generatedGernericIEnumerableCast.TryGetValue(itemType.GUID, out MethodInfo method))
            {
                method = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast)).MakeGenericMethod(itemType);
                //cache methodinfo
                generatedGernericIEnumerableCast.AddOrUpdate(itemType.GUID, method, (guid, mi) => method);
            }
            return method;
        }

        private static ConcurrentDictionary<Guid, MethodInfo> generatedGernericToList = new ConcurrentDictionary<Guid, MethodInfo>();
        public static MethodInfo GetGenericToList(Type itemType)
        {
            //try to get cached or make generic cast method for the type
            if (!generatedGernericToList.TryGetValue(itemType.GUID, out MethodInfo method))
            {
                method = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList)).MakeGenericMethod(itemType);
                //cache methodinfo
                generatedGernericToList.AddOrUpdate(itemType.GUID, method, (guid, mi) => method);
            }
            return method;
        }

        private static ConcurrentDictionary<Guid, MethodInfo> generatedGernericToArray = new ConcurrentDictionary<Guid, MethodInfo>();
        public static MethodInfo GetGenericToArray(Type itemType)
        {
            //try to get cached or make generic cast method for the type
            if (!generatedGernericToArray.TryGetValue(itemType.GUID, out MethodInfo method))
            {
                method = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(itemType);
                //cache methodinfo
                generatedGernericToArray.AddOrUpdate(itemType.GUID, method, (guid, mi) => method);
            }
            return method;
        }
    }
}
