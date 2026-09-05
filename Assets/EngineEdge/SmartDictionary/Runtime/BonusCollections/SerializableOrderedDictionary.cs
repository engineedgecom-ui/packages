using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// A serializable generic dictionary that preserves insertion order and implements
    /// <see cref="ISerializationCallbackReceiver"/> so Unity can persist its contents
    /// across serialization boundaries (e.g. Play-mode, domain reload, asset saves).
    /// </summary>
    /// <remarks>
    /// Uses composition rather than inheritance. Two internal data structures work
    /// together to provide O(1) keyed lookup while still tracking insertion order:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       A <see cref="Dictionary{TKey,TValue}"/> for fast key-based access.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       A <see cref="List{T}"/> of <typeparamref name="TKey"/> that records
    ///       the insertion sequence and is used for ordered enumeration, index-based
    ///       access, and the <see cref="MoveToFront"/>/<see cref="MoveToBack"/> helpers.
    ///     </description>
    ///   </item>
    /// </list>
    /// A <see cref="List{T}"/> of <see cref="SerializableKeyValuePair{TKey,TValue}"/>
    /// bridges the gap to Unity's serialization system.
    /// </remarks>
    /// <typeparam name="TKey">
    /// The key type. Must be serializable by Unity and usable as a dictionary key.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The value type. Must be serializable by Unity.
    /// </typeparam>
    [Serializable]
    public class SerializableOrderedDictionary<TKey, TValue>
        : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        // ── Internal state ───────────────────────────────────────────────────────

        /// <summary>
        /// Primary O(1) lookup dictionary.
        /// </summary>
        private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();

        /// <summary>
        /// Ordered list of keys that tracks the insertion sequence.
        /// </summary>
        private List<TKey> _keyOrder = new List<TKey>();

        /// <summary>
        /// The list Unity actually serializes. Populated in insertion order during
        /// <see cref="OnBeforeSerialize"/> and consumed during
        /// <see cref="OnAfterDeserialize"/>.
        /// </summary>
        [SerializeField]
        private List<SerializableKeyValuePair<TKey, TValue>> _serializedPairs
            = new List<SerializableKeyValuePair<TKey, TValue>>();

        // ── Constructor ──────────────────────────────────────────────────────────

        /// <summary>
        /// Initialises a new, empty <see cref="SerializableOrderedDictionary{TKey,TValue}"/>.
        /// </summary>
        public SerializableOrderedDictionary() { }

        // ── ISerializationCallbackReceiver ───────────────────────────────────────

        /// <summary>
        /// Called by Unity immediately before the object is serialized.
        /// Flushes the dictionary into <see cref="_serializedPairs"/> in insertion
        /// order so the order can be faithfully restored after deserialization.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _serializedPairs.Clear();
            foreach (TKey key in _keyOrder)
            {
                if (_dict.TryGetValue(key, out TValue value))
                {
                    _serializedPairs.Add(new SerializableKeyValuePair<TKey, TValue>(key, value));
                }
            }
        }

        /// <summary>
        /// Called by Unity immediately after the object has been deserialized.
        /// Rebuilds the live dictionary and insertion-order list from
        /// <see cref="_serializedPairs"/>. Duplicate keys are skipped with a warning.
        /// </summary>
        public void OnAfterDeserialize()
        {
            _dict = new Dictionary<TKey, TValue>(_serializedPairs.Count);
            _keyOrder = new List<TKey>(_serializedPairs.Count);

            for (int i = 0; i < _serializedPairs.Count; i++)
            {
                TKey key = _serializedPairs[i].Key;
                TValue value = _serializedPairs[i].Value;

                if (_dict.ContainsKey(key))
                {
                    Debug.LogWarning(
                        $"[SerializableOrderedDictionary] Duplicate key at index {i} " +
                        $"skipped: '{key}'. Type: {typeof(TKey).Name}");
                    continue;
                }

                _dict[key] = value;
                _keyOrder.Add(key);
            }
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the number of key/value pairs currently in the dictionary.
        /// </summary>
        public int Count => _dict.Count;

        /// <summary>
        /// Gets the keys in insertion order.
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                foreach (TKey key in _keyOrder)
                    yield return key;
            }
        }

        /// <summary>
        /// Gets the values in insertion order.
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (TKey key in _keyOrder)
                    yield return _dict[key];
            }
        }

        /// <summary>
        /// Gets or sets the value associated with <paramref name="key"/>.
        /// When setting, if <paramref name="key"/> does not already exist it is
        /// appended at the end of the insertion order.
        /// </summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown on get when <paramref name="key"/> does not exist.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is <c>null</c>.
        /// </exception>
        public TValue this[TKey key]
        {
            get => _dict[key];
            set
            {
                if (!_dict.ContainsKey(key))
                    _keyOrder.Add(key);

                _dict[key] = value;
            }
        }

        /// <summary>
        /// Attempts to add a new key/value pair. The key is appended at the end of
        /// the insertion order.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>
        /// <c>true</c> if the pair was added; <c>false</c> if
        /// <paramref name="key"/> already exists.
        /// </returns>
        public bool TryAdd(TKey key, TValue value)
        {
            if (_dict.ContainsKey(key))
                return false;

            _dict[key] = value;
            _keyOrder.Add(key);
            return true;
        }

        /// <summary>
        /// Adds an element with the provided key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentException">Thrown when an element with the same key already exists.</exception>
        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
                throw new ArgumentException($"An item with the same key has already been added. Key: {key}", nameof(key));
        }

        /// <summary>
        /// Attempts to remove the entry with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// <c>true</c> if the element was found and removed; <c>false</c> otherwise.
        /// </returns>
        public bool TryRemove(TKey key)
        {
            if (!_dict.Remove(key))
                return false;

            _keyOrder.Remove(key);
            return true;
        }

        /// <summary>
        /// Determines whether the dictionary contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

        /// <summary>
        /// Gets the value associated with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">
        /// When this method returns <c>true</c>, contains the value; otherwise
        /// the default value for <typeparamref name="TValue"/>.
        /// </param>
        /// <returns><c>true</c> if the key was found; otherwise <c>false</c>.</returns>
        public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);

        /// <summary>
        /// Removes all entries from the dictionary and resets the insertion order.
        /// </summary>
        public void Clear()
        {
            _dict.Clear();
            _keyOrder.Clear();
        }

        /// <summary>
        /// Returns the key/value pair at the given insertion-order <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based insertion-order index of the entry to retrieve.
        /// </param>
        /// <returns>The <see cref="KeyValuePair{TKey,TValue}"/> at the given index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="index"/> is outside the range
        /// <c>[0, Count)</c>.
        /// </exception>
        public KeyValuePair<TKey, TValue> GetAt(int index)
        {
            if (index < 0 || index >= _keyOrder.Count)
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    $"[SerializableOrderedDictionary] Index {index} is out of range. " +
                    $"Valid range: [0, {_keyOrder.Count - 1}].");

            TKey key = _keyOrder[index];
            return new KeyValuePair<TKey, TValue>(key, _dict[key]);
        }

        /// <summary>
        /// Moves the entry with <paramref name="key"/> to insertion position 0
        /// (the front of the ordered sequence).
        /// </summary>
        /// <param name="key">The key of the entry to move.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when <paramref name="key"/> does not exist.
        /// </exception>
        public void MoveToFront(TKey key)
        {
            if (!_dict.ContainsKey(key))
                throw new KeyNotFoundException(
                    $"[SerializableOrderedDictionary] Key not found: '{key}'.");

            _keyOrder.Remove(key);
            _keyOrder.Insert(0, key);
        }

        /// <summary>
        /// Moves the entry with <paramref name="key"/> to the last insertion
        /// position (the back of the ordered sequence).
        /// </summary>
        /// <param name="key">The key of the entry to move.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when <paramref name="key"/> does not exist.
        /// </exception>
        public void MoveToBack(TKey key)
        {
            if (!_dict.ContainsKey(key))
                throw new KeyNotFoundException(
                    $"[SerializableOrderedDictionary] Key not found: '{key}'.");

            _keyOrder.Remove(key);
            _keyOrder.Add(key);
        }

        /// <summary>
        /// Returns an enumerator that iterates through key/value pairs in insertion order.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>
        /// in insertion order.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (TKey key in _keyOrder)
                yield return new KeyValuePair<TKey, TValue>(key, _dict[key]);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
