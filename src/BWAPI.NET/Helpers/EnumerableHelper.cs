using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BWAPI.NET
{
    public static class EnumerableHelper
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var item in source)
            {
                action(item);
            }
        }

        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] source)
        {
            return new ReadOnlyCollection<T>(source);
        }

        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

        /// <summary>Returns the minimum value in a generic sequence according to a specified key selector function.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>The value with the minimum key in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">No key extracted from <paramref name="source" /> implements the <see cref="IComparable" /> or <see cref="System.IComparable{TKey}" /> interface.</exception>
        /// <remarks>
        /// <para>If <typeparamref name="TKey" /> is a reference type and the source sequence is empty or contains only values that are <see langword="null" />, this method returns <see langword="null" />.</para>
        /// </remarks>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return MinBy(source, keySelector, comparer: null);
        }

        /// <summary>Returns the minimum value in a generic sequence according to a specified key selector function.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of key to compare elements by.</typeparam>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">The <see cref="IComparer{TKey}" /> to compare keys.</param>
        /// <returns>The value with the minimum key in the sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">No key extracted from <paramref name="source" /> implements the <see cref="IComparable" /> or <see cref="IComparable{TKey}" /> interface.</exception>
        /// <remarks>
        /// <para>If <typeparamref name="TKey" /> is a reference type and the source sequence is empty or contains only values that are <see langword="null" />, this method returns <see langword="null" />.</para>
        /// </remarks>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            comparer ??= Comparer<TKey>.Default;

            using IEnumerator<TSource> e = source.GetEnumerator();

            if (!e.MoveNext())
            {
                if (default(TSource) is null)
                {
                    return default;
                }
                else
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
            }

            TSource value = e.Current;
            TKey key = keySelector(value);

            if (default(TKey) is null)
            {
                if (key == null)
                {
                    TSource firstValue = value;

                    do
                    {
                        if (!e.MoveNext())
                        {
                            // All keys are null, surface the first element.
                            return firstValue;
                        }

                        value = e.Current;
                        key = keySelector(value);
                    }
                    while (key == null);
                }

                while (e.MoveNext())
                {
                    TSource nextValue = e.Current;
                    TKey nextKey = keySelector(nextValue);
                    if (nextKey != null && comparer.Compare(nextKey, key) < 0)
                    {
                        key = nextKey;
                        value = nextValue;
                    }
                }
            }
            else
            {
                if (comparer == Comparer<TKey>.Default)
                {
                    while (e.MoveNext())
                    {
                        TSource nextValue = e.Current;
                        TKey nextKey = keySelector(nextValue);
                        if (Comparer<TKey>.Default.Compare(nextKey, key) < 0)
                        {
                            key = nextKey;
                            value = nextValue;
                        }
                    }
                }
                else
                {
                    while (e.MoveNext())
                    {
                        TSource nextValue = e.Current;
                        TKey nextKey = keySelector(nextValue);
                        if (comparer.Compare(nextKey, key) < 0)
                        {
                            key = nextKey;
                            value = nextValue;
                        }
                    }
                }
            }

            return value;
        }
    }
}
