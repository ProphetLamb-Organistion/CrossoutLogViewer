using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CrossoutLogView.Database.Reflection
{
    public static class Generics
    {
        public static object CastEnumerable(Type type, Type itemType, IEnumerable items)
        {
            var ienumerable = GetEnumerableCast(itemType).Invoke(null, new object[] { items }); //items.Cast<itemType>
            if (typeof(IList).IsAssignableFrom(type))
            {
                return GetGenericToList(itemType).Invoke(null, new object[] { ienumerable }); //ienumerable.ToList<itemType>
            }
            if (typeof(Array).IsAssignableFrom(type))
            {
                return GetGenericToArray(itemType).Invoke(null, new object[] { ienumerable });//ienumerable.ToArray<itemType>
            }
            return ienumerable;
        }

        private static Dictionary<Guid, MethodInfo> generatedGernericIEnumerableCast = new Dictionary<Guid, MethodInfo>();
        public static MethodInfo GetEnumerableCast(Type itemType)
        {
            //try to get cached or make generic cast method for the type
            if (!generatedGernericIEnumerableCast.TryGetValue(itemType.GUID, out MethodInfo method))
            {
                method = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast)).MakeGenericMethod(itemType);
                //cache methodinfo
                generatedGernericIEnumerableCast.Add(itemType.GUID, method);
            }
            return method;
        }

        private static Dictionary<Guid, MethodInfo> generatedGernericToList = new Dictionary<Guid, MethodInfo>();
        public static MethodInfo GetGenericToList(Type itemType)
        {
            //try to get cached or make generic cast method for the type
            if (!generatedGernericToList.TryGetValue(itemType.GUID, out MethodInfo method))
            {
                method = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList)).MakeGenericMethod(itemType);
                //cache methodinfo
                generatedGernericToList.Add(itemType.GUID, method);
            }
            return method;
        }

        private static Dictionary<Guid, MethodInfo> generatedGernericToArray = new Dictionary<Guid, MethodInfo>();
        public static MethodInfo GetGenericToArray(Type itemType)
        {
            //try to get cached or make generic cast method for the type
            if (!generatedGernericToList.TryGetValue(itemType.GUID, out MethodInfo method))
            {
                method = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(itemType);
                //cache methodinfo
                generatedGernericToList.Add(itemType.GUID, method);
            }
            return method;
        }
    }
}
