using System;

namespace CrossoutLogView.Common
{
    public static class ArrayUtility
    {
        /// <summary>
        /// Sort a one-dimesional array by swapping each element at <paramref name="items"/> to the index indicated by <paramref name="keys"/>. 
        /// The length of both arrays must be equal.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that contains the elements to be sorted.</param>
        /// <param name="keys">The one-dimensional <see cref="Int32[]"/> that contains indicies.</param>
        public static T[] SortByKeys<T>(this T[] array, int[] keys)
        {
            var newArray = new T[array.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                newArray.SetValue(array.GetValue(keys[i]), i);
            }
            return newArray;
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurence within the entire <<see cref="Array"/>.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="predicate">The <see cref="Predicate{T}"/> use to locate the object.</param>
        /// <returns> the zero-based index of the first occurence of the specified object.</returns>
        public static int IndexOf(this Array array, Predicate<object> predicate)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array.GetValue(i))) return i;
            }
            return -1;
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurence within the entire <<see cref="Array"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="IEquatable{T}"/> type.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="element">The object to locate in the <see cref="Array"/>.</param>
        /// <returns> the zero-based index of the first occurence of the specified object.</returns>
        public static int IndexOf<T>(this T[] array, T element) where T : IEquatable<T>
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (element.Equals(array.GetValue(i))) return i;
            }
            return -1;
        }

    }
}
