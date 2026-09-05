using System;
using System.Collections.Generic;
using System.Linq;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// Provides LINQ-style extension methods for <see cref="SerializableDictionary{TKey,TValue}"/>
    /// and related sequences.
    /// </summary>
    /// <remarks>
    /// All methods that return a new dictionary do not modify the source dictionary
    /// and do not carry over event subscriptions.
    /// </remarks>
    public static class SmartDictionaryLinqExtensions
    {
        // ── Construction helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Converts any <see cref="IEnumerable{TSource}"/> sequence into a
        /// <see cref="SerializableDictionary{TKey,TValue}"/> using the provided
        /// key and value selectors.
        /// </summary>
        /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the resulting dictionary.</typeparam>
        /// <param name="source">The sequence to convert.</param>
        /// <param name="keySelector">
        /// A function that extracts a key from each source element.
        /// </param>
        /// <param name="valueSelector">
        /// A function that extracts a value from each source element.
        /// </param>
        /// <returns>
        /// A <see cref="SerializableDictionary{TKey,TValue}"/> built from the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/>, <paramref name="keySelector"/>, or
        /// <paramref name="valueSelector"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the sequence contains duplicate keys.
        /// </exception>
        public static SerializableDictionary<TKey, TValue> ToSerializableDictionary<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey>       keySelector,
            Func<TSource, TValue>     valueSelector)
        {
            if (source        == null) throw new ArgumentNullException(nameof(source));
            if (keySelector   == null) throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));

            var dict = new SerializableDictionary<TKey, TValue>();
            foreach (var item in source)
                dict.Add(keySelector(item), valueSelector(item));

            return dict;
        }

        /// <summary>
        /// Converts a sequence of <see cref="KeyValuePair{TKey,TValue}"/> into a
        /// <see cref="SerializableDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The sequence of key-value pairs to convert.</param>
        /// <returns>
        /// A <see cref="SerializableDictionary{TKey,TValue}"/> containing every pair
        /// in <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when duplicate keys are encountered.
        /// </exception>
        public static SerializableDictionary<TKey, TValue> ToSerializableDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var dict = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in source)
                dict.Add(kvp.Key, kvp.Value);

            return dict;
        }

        // ── Filtering ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Filters the dictionary to those entries for which
        /// <paramref name="predicate"/> returns <c>true</c> and returns a new
        /// <see cref="SerializableDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to filter.</param>
        /// <param name="predicate">
        /// A function that receives each key and value and returns <c>true</c> for
        /// entries that should be included.
        /// </param>
        /// <returns>
        /// A new dictionary containing only the matching entries.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.
        /// </exception>
        public static SerializableDictionary<TKey, TValue> WhereDict<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source,
            Func<TKey, TValue, bool>                  predicate)
        {
            if (source    == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.Filter(predicate);
        }

        // ── Projection ────────────────────────────────────────────────────────────

        /// <summary>
        /// Projects each value in the source dictionary to a new form and returns
        /// a <see cref="SerializableDictionary{TKey,TResult}"/> with the same keys.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The original value type.</typeparam>
        /// <typeparam name="TResult">The projected value type.</typeparam>
        /// <param name="source">The dictionary to project.</param>
        /// <param name="selector">
        /// A transform function applied to each key-value pair.
        /// </param>
        /// <returns>
        /// A new dictionary with the same keys and projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.
        /// </exception>
        public static SerializableDictionary<TKey, TResult> SelectValues<TKey, TValue, TResult>(
            this SerializableDictionary<TKey, TValue> source,
            Func<TKey, TValue, TResult>               selector)
        {
            if (source   == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return source.Map(selector);
        }

        // ── Ordering ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a new <see cref="SerializableDictionary{TKey,TValue}"/> whose
        /// entries are ordered ascending by key using the default comparer.
        /// </summary>
        /// <typeparam name="TKey">The key type. Must implement <see cref="IComparable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to sort.</param>
        /// <returns>A new dictionary sorted by key.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static SerializableDictionary<TKey, TValue> OrderByKey<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source)
            where TKey : IComparable<TKey>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in source.OrderBy(kv => kv.Key))
                result.Add(kvp.Key, kvp.Value);

            return result;
        }

        /// <summary>
        /// Returns a new <see cref="SerializableDictionary{TKey,TValue}"/> whose
        /// entries are ordered ascending by value using the default comparer.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type. Must implement <see cref="IComparable{TValue}"/>.</typeparam>
        /// <param name="source">The dictionary to sort.</param>
        /// <returns>A new dictionary sorted by value.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static SerializableDictionary<TKey, TValue> OrderByValue<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source)
            where TValue : IComparable<TValue>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in source.OrderBy(kv => kv.Value))
                result.Add(kvp.Key, kvp.Value);

            return result;
        }

        // ── Aggregation ───────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the sum of all integer values in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <param name="source">The dictionary whose values are summed.</param>
        /// <returns>The sum of all values, or <c>0</c> if the dictionary is empty.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static int SumValues<TKey>(
            this SerializableDictionary<TKey, int> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Values.Sum();
        }

        /// <summary>
        /// Computes the sum of all float values in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <param name="source">The dictionary whose values are summed.</param>
        /// <returns>The sum of all values, or <c>0f</c> if the dictionary is empty.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static float SumValues<TKey>(
            this SerializableDictionary<TKey, float> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            // Linq Sum overload for float not always available; use explicit cast via double
            return (float)source.Values.Sum(v => (double)v);
        }

        /// <summary>
        /// Computes the arithmetic average of all float values in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <param name="source">The dictionary whose values are averaged.</param>
        /// <returns>
        /// The average of all values, or <c>0f</c> if the dictionary is empty.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static float AverageValues<TKey>(
            this SerializableDictionary<TKey, float> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Count == 0) return 0f;
            return (float)source.Values.Average(v => (double)v);
        }

        /// <summary>
        /// Computes the arithmetic average of all integer values in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <param name="source">The dictionary whose values are averaged.</param>
        /// <returns>The average of all values as a double, or 0.0 if empty.</returns>
        public static double AverageValues<TKey>(
            this SerializableDictionary<TKey, int> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Count == 0) return 0.0;
            return source.Values.Average();
        }

        /// <summary>
        /// Computes the arithmetic average of all double values in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <param name="source">The dictionary whose values are averaged.</param>
        /// <returns>The average of all values as a double, or 0.0 if empty.</returns>
        public static double AverageValues<TKey>(
            this SerializableDictionary<TKey, double> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Count == 0) return 0.0;
            return source.Values.Average();
        }

        /// <summary>
        /// Returns the minimum value present in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">
        /// The value type. Must implement <see cref="IComparable{TValue}"/>.
        /// </typeparam>
        /// <param name="source">The dictionary to inspect.</param>
        /// <returns>The smallest value in the dictionary.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the dictionary is empty.
        /// </exception>
        public static TValue MinValue<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source)
            where TValue : IComparable<TValue>
        {
            if (source == null)  throw new ArgumentNullException(nameof(source));
            if (source.Count == 0)
                throw new InvalidOperationException("The dictionary is empty.");

            return source.Values.Min();
        }

        /// <summary>
        /// Returns the maximum value present in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">
        /// The value type. Must implement <see cref="IComparable{TValue}"/>.
        /// </typeparam>
        /// <param name="source">The dictionary to inspect.</param>
        /// <returns>The largest value in the dictionary.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the dictionary is empty.
        /// </exception>
        public static TValue MaxValue<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source)
            where TValue : IComparable<TValue>
        {
            if (source == null)  throw new ArgumentNullException(nameof(source));
            if (source.Count == 0)
                throw new InvalidOperationException("The dictionary is empty.");

            return source.Values.Max();
        }
    }
}
