using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CrossoutLogView.Common
{
    public static class CollectionExtention
    {
        // Add
        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source == null) throw new ArgumentNullException(nameof(source));
            using var en = source.GetEnumerator();
            while (en.MoveNext()) target.Add(en.Current);
        }

        public static void AddRange<TTarget, TSource>(this ICollection<TTarget> target, IEnumerable<TSource> source, Func<TSource, TTarget> selector)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            using var en = source.GetEnumerator();
            while (en.MoveNext()) target.Add(selector(en.Current));
        }

        /// <summary>
        /// Adds all days between <paramref name="start"/> and <paramref name="end"/> to the collection.
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <param name="start">The frist day that will be added to the collection.</param>
        /// <param name="end">The last day that will be added to the collection.</param>
        public static TDateTime AddDays<TDateTime>(this TDateTime collection, DateTime start, DateTime end) where TDateTime : ICollection<DateTime>
        {
            if (end.Date < start.Date)
                throw new ArgumentOutOfRangeException(nameof(end), "The end date must be greater or equal to the start date.");
            while (start.Date != end.Date)
            {
                collection.Add(start);
                // Increment
                start = start.AddDays(1);
            }
            collection.Add(start);
            return collection;
        }

        // Filter
        public static void Filter<T>(this Collection<T> target, Collection<T> source, Predicate<T> filter)
        {
            for (int i = 0; i < source.Count; i++)
            {
                var index = target.IndexOf(source[i]);
                if (filter(source[i]))
                {
                    if (index == -1)
                        target.Add(source[i]);
                }
                else
                {
                    if (index != -1)
                        target.RemoveAt(index);
                }
            }
        }

        // FindIndex
        public static int FindIndex<T>(this Collection<T> collection, Predicate<T> match)
        {
            return FindIndex(collection, 0, collection.Count, match);
        }
        public static int FindIndex(this ICollection collection, Predicate<object> match)
        {
            return FindIndex(collection, 0, collection.Count, match);
        }
        public static int FindIndex<T>(this Collection<T> collection, int startIndex, Predicate<T> match)
        {
            return FindIndex(collection, startIndex, collection.Count - startIndex, match);
        }
        public static int FindIndex(this ICollection collection, int startIndex, Predicate<object> match)
        {
            return FindIndex(collection, startIndex, collection.Count - startIndex, match);
        }

        public static int FindIndex<T>(this Collection<T> collection, int startIndex, int count, Predicate<T> match)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (startIndex > collection.Count) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex > collection.Count - count) throw new ArgumentOutOfRangeException(nameof(count));
            if (match == null) throw new ArgumentNullException(nameof(match));
            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(collection[i])) return i;
            }
            return -1;
        }

        public static int FindIndex(this ICollection collection, int startIndex, int count, Predicate<object> match)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (startIndex > collection.Count) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex > collection.Count - count) throw new ArgumentOutOfRangeException(nameof(count));
            if (match == null) throw new ArgumentNullException(nameof(match));
            int endIndex = startIndex + count, i = 0;
            var em = collection.GetEnumerator();
            while (i < startIndex && em.MoveNext()) i++; //skip until i=startindex
            while (i < endIndex && em.MoveNext()) //take until i=endindex-1
            {
                if (match(em.Current)) return i;
                i++;
            }
            return -1;
        }

        // Find
        public static T Find<T>(this Collection<T> collection, Predicate<T> match)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (match == null) throw new ArgumentNullException(nameof(match));
            for (int i = 0; i < collection.Count; i++)
            {
                if (match(collection[i]))
                {
                    return collection[i];
                }
            }
            return default;
        }

        public static List<T> FindAll<T>(this Collection<T> collection, Predicate<T> match)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (match == null) throw new ArgumentNullException(nameof(match));
            var list = new List<T>();
            for (int i = 0; i < collection.Count; i++)
            {
                if (match(collection[i]))
                {
                    list.Add(collection[i]);
                }
            }
            return list;
        }

        // Sort
        public static void Sort<T>(this Collection<T> collection) where T : IComparable<T>
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            IntrospectiveSort(collection, 0, collection.Count, Comparer<T>.Default);
        }

        public static void Sort<T>(this Collection<T> collection, Comparison<T> comparison)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            IntrospectiveSort(collection, 0, collection.Count, Comparer<T>.Create(comparison));
        }

        public static void Sort<T>(this Collection<T> collection, IComparer<T> comparer)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            IntrospectiveSort(collection, 0, collection.Count, comparer);
        }


        #region IntrospectiveSort
        // All source code in this region is based on the reference source of following classes:
        // System.Collections.Generic.GenericArraySortHelper<T> and System.Collections.Generic.IntrospectiveSortUtilities

        private const int IntrosortSizeThreshold = 16;

        private static void IntrospectiveSort<T>(Collection<T> keys, int left, int length, IComparer<T> comparer)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            if (left < 0) throw new ArgumentOutOfRangeException(nameof(left), "Value must be a non negative integer.");
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Value must be a non negative integer.");
            if (length > keys.Count) throw new ArgumentOutOfRangeException(nameof(length), "Value must be less then the total number of elements in the collection.");
            if (length + left > keys.Count) throw new ArgumentException($"The sum of '{nameof(length)}' and '{nameof(left)}' must be less then the total number of elemments in the collection.");
            if (length < 2) return;
            IntroSort(keys, left, length + left - 1, 2 * FloorLog2(keys.Count), comparer);
        }

        private static void IntroSort<T>(Collection<T> keys, int lo, int hi, int depthLimit, IComparer<T> comparer)
        {
            while (hi > lo)
            {
                int partitionSize = hi - lo + 1;
                if (partitionSize <= IntrosortSizeThreshold)
                {
                    if (partitionSize == 1)
                    {
                        return;
                    }
                    if (partitionSize == 2)
                    {
                        SwapIfGreater(keys, comparer, lo, hi);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        SwapIfGreater(keys, comparer, lo, hi - 1);
                        SwapIfGreater(keys, comparer, lo, hi);
                        SwapIfGreater(keys, comparer, hi - 1, hi);
                        return;
                    }

                    InsertionSort(keys, lo, hi, comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    Heapsort(keys, lo, hi, comparer);
                    return;
                }
                depthLimit--;

                int p = PickPivotAndPartition(keys, lo, hi, comparer);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys, p + 1, hi, depthLimit, comparer);
                hi = p - 1;
            }
        }

        private static void SwapIfGreater<T>(Collection<T> keys, IComparer<T> comparer, int a, int b)
        {
            if (a != b)
            {
                if (comparer.Compare(keys[a], keys[b]) > 0)
                {
                    T key = keys[a];
                    keys[a] = keys[b];
                    keys[b] = key;
                }
            }
        }

        private static void InsertionSort<T>(Collection<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            int i, j;
            T t;
            for (i = lo; i < hi; i++)
            {
                j = i;
                t = keys[i + 1];
                while (j >= lo && comparer.Compare(t, keys[j]) < 0)
                {
                    keys[j + 1] = keys[j];
                    j--;
                }
                keys[j + 1] = t;
            }
        }

        private static void Heapsort<T>(Collection<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            int n = hi - lo + 1;
            for (int i = n / 2; i >= 1; i = i - 1)
            {
                DownHeap(keys, i, n, lo, comparer);
            }
            for (int i = n; i > 1; i = i - 1)
            {
                Swap(keys, lo, lo + i - 1);
                DownHeap(keys, 1, i - 1, lo, comparer);
            }
        }

        private static int PickPivotAndPartition<T>(Collection<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            // Compute median-of-three.  But also partition them, since we've done the comparison.
            int middle = lo + ((hi - lo) / 2);

            // Sort lo, mid and hi appropriately, then pick mid as the pivot.
            SwapIfGreater(keys, comparer, lo, middle);  // swap the low with the mid point
            SwapIfGreater(keys, comparer, lo, hi);   // swap the low with the high
            SwapIfGreater(keys, comparer, middle, hi); // swap the middle with the high

            T pivot = keys[middle];
            Swap(keys, middle, hi - 1);
            int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right)
            {
                while (comparer.Compare(keys[++left], pivot) < 0) ;
                while (comparer.Compare(pivot, keys[--right]) < 0) ;

                if (left >= right)
                    break;

                Swap(keys, left, right);
            }

            // Put pivot in the right location.
            Swap(keys, left, (hi - 1));
            return left;
        }

        private static void DownHeap<T>(Collection<T> keys, int i, int n, int lo, IComparer<T> comparer)
        {
            T d = keys[lo + i - 1];
            int child;
            while (i <= n / 2)
            {
                child = 2 * i;
                if (child < n && comparer.Compare(keys[lo + child - 1], keys[lo + child]) < 0)
                {
                    child++;
                }
                if (comparer.Compare(d, keys[lo + child - 1]) >= 0)
                    break;
                keys[lo + i - 1] = keys[lo + child - 1];
                i = child;
            }
            keys[lo + i - 1] = d;
        }

        private static void Swap<T>(Collection<T> a, int i, int j)
        {
            if (i != j)
            {
                T t = a[i];
                a[i] = a[j];
                a[j] = t;
            }
        }

        private static int FloorLog2(int n)
        {
            int result = 0;
            while (n >= 1)
            {
                result++;
                n /= 2;
            }
            return result;
        }
        #endregion
    }
}
