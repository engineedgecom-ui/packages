using System;
using System.Collections.Generic;
using UnityEngine;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// Provides merge, set-operation, and cloning extension methods for
    /// <see cref="SerializableDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <remarks>
    /// Methods that return a new dictionary do so as shallow copies unless noted.
    /// Event subscriptions are never carried over to new dictionary instances.
    /// </remarks>
    public static class SmartDictionaryMergeExtensions
    {
        /// <summary>
        /// Merges entries from <paramref name="other"/> into <paramref name="source"/>,
        /// adding only those keys that do not already exist in <paramref name="source"/>.
        /// Existing entries are left unchanged.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to merge into.</param>
        /// <param name="other">The dictionary whose entries are candidates for merging.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="other"/> is <c>null</c>.
        /// </exception>
        public static void Merge<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source,
            IDictionary<TKey, TValue>                 other)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (other  == null) throw new ArgumentNullException(nameof(other));
            foreach (var kvp in other)
                source.TryAdd(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Merges entries from <paramref name="other"/> into <paramref name="source"/>,
        /// adding new keys and overwriting the values of existing keys.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to merge into.</param>
        /// <param name="other">The dictionary whose entries are merged in.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="other"/> is <c>null</c>.
        /// </exception>
        public static void MergeOverwrite<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source,
            IDictionary<TKey, TValue>                 other)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (other  == null) throw new ArgumentNullException(nameof(other));
            foreach (var kvp in other)
                source.AddOrUpdate(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Returns a new <see cref="SerializableDictionary{TKey,TValue}"/> containing
        /// only the entries from <paramref name="source"/> whose keys appear in
        /// <paramref name="keys"/> (set intersection by key).
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to intersect.</param>
        /// <param name="keys">The sequence of keys to keep.</param>
        /// <returns>
        /// A new dictionary containing only entries whose key is in <paramref name="keys"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="keys"/> is <c>null</c>.
        /// </exception>
        public static SerializableDictionary<TKey, TValue> Intersect<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source,
            IEnumerable<TKey>                         keys)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keys   == null) throw new ArgumentNullException(nameof(keys));
            var keySet = new HashSet<TKey>(keys);
            var result = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in source)
                if (keySet.Contains(kvp.Key))
                    result.Add(kvp.Key, kvp.Value);
            return result;
        }

        /// <summary>
        /// Returns a new <see cref="SerializableDictionary{TKey,TValue}"/> containing
        /// only the entries from <paramref name="source"/> whose keys appear in
        /// <paramref name="other"/> (set intersection by key).
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to intersect.</param>
        /// <param name="other">The other dictionary to intersect with.</param>
        /// <returns>A new dictionary containing only entries whose key is in <paramref name="other"/>.</returns>
        public static SerializableDictionary<TKey, TValue> Intersect<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source,
            IDictionary<TKey, TValue>                 other)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (other  == null) throw new ArgumentNullException(nameof(other));
            return source.Intersect(other.Keys);
        }

        /// <summary>
        /// Returns a new <see cref="SerializableDictionary{TKey,TValue}"/> containing
        /// all entries from <paramref name="source"/> whose keys do <em>not</em>
        /// appear in <paramref name="keys"/> (set difference by key).
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to subtract from.</param>
        /// <param name="keys">The sequence of keys to exclude.</param>
        /// <returns>A new dictionary with the specified keys removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="keys"/> is <c>null</c>.
        /// </exception>
        public static SerializableDictionary<TKey, TValue> Except<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source,
            IEnumerable<TKey>                         keys)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keys   == null) throw new ArgumentNullException(nameof(keys));
            var keySet = new HashSet<TKey>(keys);
            var result = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in source)
                if (!keySet.Contains(kvp.Key))
                    result.Add(kvp.Key, kvp.Value);
            return result;
        }

        /// <summary>
        /// Returns a new <see cref="SerializableDictionary{TKey,TValue}"/> containing
        /// all entries from <paramref name="source"/> whose keys do <em>not</em>
        /// appear in <paramref name="other"/> (set difference by key).
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to subtract from.</param>
        /// <param name="other">The other dictionary whose keys will be excluded.</param>
        /// <returns>A new dictionary with the specified keys removed.</returns>
        public static SerializableDictionary<TKey, TValue> Except<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source,
            IDictionary<TKey, TValue>                 other)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (other  == null) throw new ArgumentNullException(nameof(other));
            return source.Except(other.Keys);
        }

        /// <summary>
        /// Returns a new <see cref="SerializableDictionary{TKey,TValue}"/> that is
        /// the union of <paramref name="source"/> and <paramref name="other"/>.
        /// When a key exists in both dictionaries, the value from
        /// <paramref name="source"/> is preferred.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The primary dictionary (its values take priority).</param>
        /// <param name="other">The secondary dictionary.</param>
        /// <returns>
        /// A new dictionary containing all keys from both dictionaries.
        /// Conflicts are resolved in favour of <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="other"/> is <c>null</c>.
        /// </exception>
        public static SerializableDictionary<TKey, TValue> Union<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source,
            IDictionary<TKey, TValue>                 other)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (other  == null) throw new ArgumentNullException(nameof(other));
            var result = source.Clone();
            foreach (var kvp in other)
                result.TryAdd(kvp.Key, kvp.Value);
            return result;
        }

        /// <summary>
        /// Creates a shallow copy of <paramref name="source"/>. The new dictionary
        /// contains references to the same value objects as the original (for
        /// reference types). Value types are copied by value.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to clone.</param>
        /// <returns>
        /// A new <see cref="SerializableDictionary{TKey,TValue}"/> with the same entries.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static SerializableDictionary<TKey, TValue> Clone<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var result = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in source)
                result.Add(kvp.Key, kvp.Value);
            return result;
        }

        /// <summary>
        /// Creates a deep copy of <paramref name="source"/> by serializing the
        /// dictionary to JSON via <see cref="JsonUtility"/> and immediately
        /// deserializing the result into a new instance.
        /// </summary>
        /// <remarks>
        /// This method relies on <see cref="JsonUtility"/> and therefore only works
        /// correctly when both <typeparamref name="TKey"/> and
        /// <typeparamref name="TValue"/> are supported by Unity's JSON serializer
        /// (primitives, strings, and <c>[Serializable]</c> value types).
        /// For complex reference-type graphs, use a custom serializer.
        /// </remarks>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to deep-clone.</param>
        /// <returns>
        /// A fully independent copy of <paramref name="source"/> where each value
        /// is a new instance (for simple serializable types).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static SerializableDictionary<TKey, TValue> DeepClone<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            source.OnBeforeSerialize();
            var json  = source.ToJson();
            var clone = new SerializableDictionary<TKey, TValue>();
            clone.FromJson(json);
            return clone;
        }
    }
}
