using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Chartreuse.Today.Core.Shared.Tools.Extensions
{
    public static class CollectionExtension
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> source, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                source.Add(item);
            }
        }

        public static int IndexOf<T>(this IList<T> source, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return source.IndexOf(source.FirstOrDefault(predicate));
        }

        public static bool Contains<T>(this IList<T> source, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return source.FirstOrDefault(predicate) != null;
        }

        public static void Remove<T>(this IList<T> source, Func<T, bool> predicate)
        {
            var item = source.FirstOrDefault(predicate);
            if (item != null)
                source.Remove(item);
        }

        /// <summary>
        /// Try to move an element from a old index to a new index. This methods defaults to a no-op instead
        /// of an exception if one of the argument is not valid (>0 or >= collection size)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">Source observable collection</param>
        /// <param name="oldIndex">Old index</param>
        /// <param name="newIndex">New index</param>
        public static void TryMove<T>(this ObservableCollection<T> source, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= source.Count)
                return;
            if (newIndex < 0 || newIndex >= source.Count)
                return;
            if (oldIndex == newIndex)
                return;

            T item = source[oldIndex];
            source.RemoveAt(oldIndex);
            source.Insert(newIndex, item);
        }

        /// <summary>
        /// Returns true if both list contains the exact same elements, whatever the order. If both lists are 
        /// empty returns false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool ContainsSameContentAs<T>(this IList<T> source, IList<T> other)
        {
            if (source.Count != other.Count)
            {
                return false;
            }
            else if (source.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (var item in source)
                {
                    if (!other.Contains(item))
                        return false;
                }
                foreach (var item in other)
                {
                    if (!source.Contains(item))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Retruns true if all elements of the source list can be found in the other list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool Contains<T>(this IList<T> source, IList<T> other)
        {
            if (other.Count == 0)
            {
                return true;
            }
            else if (source.Count == 0 && other.Count != 0)
            {
                return false;
            }
            else
            {
                bool allFound = true;
                foreach (var item in other)
                {
                    allFound &= source.Contains(item);
                }

                return allFound;
            }
        }

        /// <summary>
        /// Returns true if both list contains the exact same elements, in the exact same order. If both lists are
        /// empty, returns true.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool ContainsSameExactContentAs<T>(this IList<T> source, IList<T> other)
        {
            if (source.Count != other.Count)
            {
                return false;
            }
            else if (source.Count == 0)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < source.Count; i++)
                {
                    if (!source[i].Equals(other[i]))
                        return false;
                }

                return true;
            }
        }

        public static void ReorderFrom<T>(this IList<T> source, IList<T> sorted)
        {
            sorted = new List<T>(sorted.Where(s => s != null));

            var index = new Dictionary<T, int>();
            for (int i = 0; i < source.Count; i++)
                index.Add(source[i], i);

            int startIndex = int.MaxValue;
            for (int i = 0; i < sorted.Count; i++)
            {
                if (index.ContainsKey(sorted[i]))
                {
                    var k = index[sorted[i]];
                    if (k < startIndex)
                        startIndex = k;
                }
            }

            if (startIndex == int.MaxValue)
                return;

            for (int i = 0; i < sorted.Count; i++)
            {
                var item = sorted[i];
                if (!item.Equals(source[i + startIndex]))
                {
                    var oldItem = source[i + startIndex];
                    source[i + startIndex] = item;
                    source[index[item]] = oldItem;

                    index[oldItem] = index[item];
                    index[item] = i + startIndex;
                }
            }
        }

        public static List<object> AlternateTwoColumns(this IEnumerable<object> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            var origin = new List<object>(enumerable);
            var source = new List<object>(origin.Count);

            int rowCount = (int)Math.Floor((double)(origin.Count / 2));

            int j = 1;
            if (origin.Count%2 == 0)
                j = 0;

            for (int i = 0; i < origin.Count; i++)
            {
                if (i % 2 == 0)
                    source.Add(origin[i / 2]);
                else
                    source.Add(origin[rowCount + j++]);
            }

            return source;
        }
    }
}
