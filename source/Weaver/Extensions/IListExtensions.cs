using System.Collections.Generic;

namespace Weaver.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IList"/>
    /// </summary>
    public static class IList
    {
        /// <summary>
        /// Inserts the range of tiems into a set position.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination.</param>
        /// <param name="index">The index.</param>
        /// <param name="source">The source.</param>
        public static void InsertRange<T>(this IList<T> destination, int index, IEnumerable<T> source)
        {
            foreach(T item in source)
            {
                destination.Insert(index, item);
                index++;
            }
        }

        /// <summary>
        /// Adds the range items to a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        public static void AddRange<T>(this IList<T> destination, IEnumerable<T> source)
        {
            foreach (T item in source)
            {
                destination.Add(item);
            }
        }
    }
}
