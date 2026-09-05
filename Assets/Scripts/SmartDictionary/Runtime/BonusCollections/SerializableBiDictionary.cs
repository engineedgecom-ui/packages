using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// A serializable generic bidirectional dictionary that provides O(1) lookup in
    /// both directions (key→value and value→key) and implements
    /// <see cref="ISerializationCallbackReceiver"/> so Unity can persist its contents
    /// across serialization boundaries (e.g. Play-mode, domain reload, asset saves).
    /// </summary>
    /// <remarks>
    /// Two internal <see cref="Dictionary{TKey,TValue}"/> instances are kept in
    /// perfect sync at all times:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>_forward</c> — maps <typeparamref name="TKey"/> →
    ///       <typeparamref name="TValue"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>_inverse</c> — maps <typeparamref name="TValue"/> →
    ///       <typeparamref name="TKey"/>.
    ///     </description>
    ///   </item>
    /// </list>
    /// Because both keys and values must uniquely identify entries, a value that
    /// already exists in the inverse dictionary cannot be added again with a
    /// different key (and vice-versa).
    /// </remarks>
    /// <typeparam name="TKey">
    /// The key type. Must be serializable by Unity and usable as a dictionary key.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The value type. Must be serializable by Unity and usable as a dictionary key
    /// (because it acts as the key in the inverse dictionary).
    /// </typeparam>
    [Serializable]
    public class SerializableBiDictionary<TKey, TValue>
        : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        // ── Internal state ───────────────────────────────────────────────────────

        /// <summary>Forward lookup: <typeparamref name="TKey"/> → <typeparamref name="TValue"/>.</summary>
        private Dictionary<TKey, TValue> _forward = new Dictionary<TKey, TValue>();

        /// <summary>Inverse lookup: <typeparamref name="TValue"/> → <typeparamref name="TKey"/>.</summary>
        private Dictionary<TValue, TKey> _inverse = new Dictionary<TValue, TKey>();

        /// <summary>
        /// The list Unity actually serializes. Populated from <c>_forward</c> during
        /// <see cref="OnBeforeSerialize"/> and consumed during
        /// <see cref="OnAfterDeserialize"/>.
        /// </summary>
        [SerializeField]
        private List<SerializableKeyValuePair<TKey, TValue>> _serializedPairs
            = new List<SerializableKeyValuePair<TKey, TValue>>();

        // ── Constructor ──────────────────────────────────────────────────────────

        /// <summary>
        /// Initialises a new, empty <see cref="SerializableBiDictionary{TKey,TValue}"/>.
        /// </summary>
        public SerializableBiDictionary() { }

        // ── ISerializationCallbackReceiver ───────────────────────────────────────

        /// <summary>
        /// Called by Unity immediately before the object is serialized.
        /// Flushes the forward dictionary into <see cref="_serializedPairs"/>.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _serializedPairs.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in _forward)
            {
                _serializedPairs.Add(
                    new SerializableKeyValuePair<TKey, TValue>(pair.Key, pair.Value));
            }
        }

        /// <summary>
        /// Called by Unity immediately after the object has been deserialized.
        /// Rebuilds both the forward and inverse dictionaries from
        /// <see cref="_serializedPairs"/>. Duplicate keys or values are skipped with
        /// a warning to avoid violating the bijection constraint.
        /// </summary>
        public void OnAfterDeserialize()
        {
            _forward = new Dictionary<TKey, TValue>(_serializedPairs.Count);
            _inverse = new Dictionary<TValue, TKey>(_serializedPairs.Count);

            for (int i = 0; i < _serializedPairs.Count; i++)
            {
                TKey key = _serializedPairs[i].Key;
                TValue value = _serializedPairs[i].Value;

                if (_forward.ContainsKey(key))
                {
                    Debug.LogWarning(
                        $"[SerializableBiDictionary] Duplicate key at index {i} " +
                        $"skipped: '{key}'. Type: {typeof(TKey).Name}");
                    continue;
                }

                if (_inverse.ContainsKey(value))
                {
                    Debug.LogWarning(
                        $"[SerializableBiDictionary] Duplicate value at index {i} " +
                        $"skipped: '{value}'. Type: {typeof(TValue).Name}");
                    continue;
                }

                _forward[key] = value;
                _inverse[value] = key;
            }
        }

        // ── Public Properties ────────────────────────────────────────────────────

        /// <summary>
        /// Gets the number of entries currently in the bidirectional dictionary.
        /// </summary>
        public int Count => _forward.Count;

        /// <summary>
        /// Gets a read-only view of the forward dictionary
        /// (<typeparamref name="TKey"/> → <typeparamref name="TValue"/>).
        /// </summary>
        public IReadOnlyDictionary<TKey, TValue> Forward => _forward;

        /// <summary>
        /// Gets a read-only view of the inverse dictionary
        /// (<typeparamref name="TValue"/> → <typeparamref name="TKey"/>).
        /// </summary>
        public IReadOnlyDictionary<TValue, TKey> Inverse => _inverse;

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to add a new key/value pair to both the forward and inverse
        /// dictionaries.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>
        /// <c>true</c> if both <paramref name="key"/> and <paramref name="value"/>
        /// were unique and the pair was added; <c>false</c> if either already exists.
        /// </returns>
        public bool TryAdd(TKey key, TValue value)
        {
            if (_forward.ContainsKey(key) || _inverse.ContainsKey(value))
                return false;

            _forward[key] = value;
            _inverse[value] = key;
            return true;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>The value associated with the specified key.</returns>
        public TValue this[TKey key] => GetByKey(key);

        /// <summary>
        /// Adds the specified key and value to both the forward and inverse dictionaries.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentException">Thrown when either key or value already exists.</exception>
        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
                throw new ArgumentException($"Cannot add duplicate key '{key}' or duplicate value '{value}' to BiDictionary.");
        }

        /// <summary>
        /// Attempts to remove the entry identified by <paramref name="key"/> from
        /// both dictionaries.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns>
        /// <c>true</c> if the entry was found and removed; <c>false</c> otherwise.
        /// </returns>
        public bool TryRemove(TKey key)
        {
            if (!_forward.TryGetValue(key, out TValue value))
                return false;

            _forward.Remove(key);
            _inverse.Remove(value);
            return true;
        }

        /// <summary>
        /// Attempts to remove the entry identified by <paramref name="value"/> from
        /// both dictionaries using the inverse lookup.
        /// </summary>
        /// <param name="value">The value of the entry to remove.</param>
        /// <returns>
        /// <c>true</c> if the entry was found and removed; <c>false</c> otherwise.
        /// </returns>
        public bool TryRemoveByValue(TValue value)
        {
            if (!_inverse.TryGetValue(value, out TKey key))
                return false;

            _inverse.Remove(value);
            _forward.Remove(key);
            return true;
        }

        /// <summary>
        /// Returns the value associated with <paramref name="key"/> using the
        /// forward dictionary.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The value mapped to <paramref name="key"/>.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when <paramref name="key"/> does not exist.
        /// </exception>
        public TValue GetByKey(TKey key) => _forward[key];

        /// <summary>
        /// Returns the key associated with <paramref name="value"/> using the
        /// inverse dictionary.
        /// </summary>
        /// <param name="value">The value to look up.</param>
        /// <returns>The key mapped to <paramref name="value"/>.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when <paramref name="value"/> does not exist in the inverse dictionary.
        /// </exception>
        public TKey GetByValue(TValue value) => _inverse[value];

        /// <summary>
        /// Attempts to get the value associated with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">
        /// When this method returns <c>true</c>, contains the mapped value;
        /// otherwise the default value for <typeparamref name="TValue"/>.
        /// </param>
        /// <returns><c>true</c> if the key was found; otherwise <c>false</c>.</returns>
        public bool TryGetByKey(TKey key, out TValue value)
            => _forward.TryGetValue(key, out value);

        /// <summary>
        /// Attempts to get the key associated with <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to look up in the inverse dictionary.</param>
        /// <param name="key">
        /// When this method returns <c>true</c>, contains the mapped key;
        /// otherwise the default value for <typeparamref name="TKey"/>.
        /// </param>
        /// <returns><c>true</c> if the value was found; otherwise <c>false</c>.</returns>
        public bool TryGetByValue(TValue value, out TKey key)
            => _inverse.TryGetValue(value, out key);

        /// <summary>
        /// Determines whether the dictionary contains an entry with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        public bool ContainsKey(TKey key) => _forward.ContainsKey(key);

        /// <summary>
        /// Determines whether the dictionary contains an entry with the specified value.
        /// </summary>
        /// <param name="value">The value to locate in the inverse dictionary.</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        public bool ContainsValue(TValue value) => _inverse.ContainsKey(value);

        /// <summary>
        /// Removes all entries from both the forward and inverse dictionaries.
        /// </summary>
        public void Clear()
        {
            _forward.Clear();
            _inverse.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the forward key/value pairs.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => _forward.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
