using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// A generic dictionary that is fully serializable by Unity's serialization system.
    /// Inherits from <see cref="Dictionary{TKey, TValue}"/> and implements
    /// <see cref="ISerializationCallbackReceiver"/> to persist data across Unity
    /// serialization cycles (play mode, domain reload, asset saves, etc.).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unity cannot natively serialize generic dictionaries. This class bridges that
    /// gap by maintaining a backing <see cref="List{T}"/> of
    /// <see cref="SerializableKeyValuePair{TKey,TValue}"/> which Unity <em>can</em>
    /// serialize, and synchronising both representations automatically via
    /// <see cref="ISerializationCallbackReceiver"/>.
    /// </para>
    /// <para>
    /// For reactive, event-driven collections with C# Actions and serialized UnityEvents,
    /// see <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TKey">
    /// The type of the keys. Must be serializable by Unity (e.g. <c>string</c>,
    /// <c>int</c>, or a <c>[Serializable]</c> struct/class).
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of the values. Same serialization requirements as <typeparamref name="TKey"/>.
    /// </typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        // ── Serialized backing store ──────────────────────────────────────────────

        [SerializeField]
        private List<SerializableKeyValuePair<TKey, TValue>> _serializedPairs
            = new List<SerializableKeyValuePair<TKey, TValue>>();

        [NonSerialized]
        protected bool _isRuntimeModified = false;

        // ── Constructors ──────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes a new, empty <see cref="SerializableDictionary{TKey,TValue}"/>
        /// using the default equality comparer for <typeparamref name="TKey"/>.
        /// </summary>
        public SerializableDictionary() : base() { }

        /// <summary>
        /// Initializes a new <see cref="SerializableDictionary{TKey,TValue}"/> by
        /// copying all entries from <paramref name="dict"/>.
        /// </summary>
        /// <param name="dict">The dictionary whose entries are copied.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dict"/> is <c>null</c>.
        /// </exception>
        public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict)
        {
            _isRuntimeModified = true;
        }

        /// <summary>
        /// Initializes a new, empty <see cref="SerializableDictionary{TKey,TValue}"/>
        /// using the specified equality comparer for keys.
        /// </summary>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> to use when comparing keys.
        /// </param>
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

        /// <summary>
        /// Initializes a new <see cref="SerializableDictionary{TKey,TValue}"/> with elements copied from
        /// the specified dictionary and using the specified equality comparer.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied.</param>
        /// <param name="comparer">The equality comparer to use for keys.</param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {
            _isRuntimeModified = true;
        }

        // ── ISerializationCallbackReceiver ────────────────────────────────────────

        /// <summary>
        /// Called by Unity before serializing this object.
        /// Flushes the current dictionary contents into
        /// <c>_serializedPairs</c> so that Unity can persist them.
        /// </summary>
        public void OnBeforeSerialize()
        {
            if (_isRuntimeModified || (_serializedPairs.Count == 0 && Count > 0))
            {
                _serializedPairs.Clear();
                foreach (var kvp in this)
                    _serializedPairs.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
                _isRuntimeModified = false;
            }
        }

        /// <summary>
        /// Called by Unity after deserializing this object.
        /// Rebuilds the runtime dictionary from <c>_serializedPairs</c>.
        /// Duplicate keys are skipped.
        /// </summary>
        public void OnAfterDeserialize()
        {
            base.Clear();
            foreach (var pair in _serializedPairs)
            {
                if (pair == null || pair.Key == null)
                {
                    continue;
                }

                if (ContainsKey(pair.Key))
                {
                    continue;
                }

                base.Add(pair.Key, pair.Value);
            }
            _isRuntimeModified = false;
        }

        // ── Indexer override ──────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the value associated with <paramref name="key"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Get:</b> returns the stored value when the key exists, or
        /// <c>default(TValue)</c> when it does not — never throws
        /// <see cref="KeyNotFoundException"/>.
        /// </para>
        /// <para>
        /// <b>Set:</b> adds or updates the entry for <paramref name="key"/>.
        /// </para>
        /// </remarks>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>
        /// The value associated with <paramref name="key"/>, or
        /// <c>default(TValue)</c> if the key is not present.
        /// </returns>
        public virtual new TValue this[TKey key]
        {
            get => TryGetValue(key, out var val) ? val : default;
            set
            {
                base[key] = value;
                _isRuntimeModified = true;
            }
        }

        // ── Core Mutation Methods ─────────────────────────────────────────────────

        /// <summary>
        /// Adds <paramref name="key"/> with <paramref name="value"/> to the dictionary.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to associate with <paramref name="key"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="key"/> already exists in the dictionary.
        /// </exception>
        public virtual new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            _isRuntimeModified = true;
        }

        /// <summary>
        /// Removes all entries from the dictionary.
        /// </summary>
        public virtual new void Clear()
        {
            base.Clear();
            _isRuntimeModified = true;
        }

        // ── Safe Access Methods ───────────────────────────────────────────────────

        /// <summary>
        /// Adds <paramref name="key"/> with <paramref name="value"/> only if the key
        /// is not already present in the dictionary.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to associate with <paramref name="key"/>.</param>
        /// <returns>
        /// <c>true</c> if the entry was added; <c>false</c> if the key was already present.
        /// </returns>
        public virtual new bool TryAdd(TKey key, TValue value)
        {
            if (ContainsKey(key))
                return false;

            base.Add(key, value);
            _isRuntimeModified = true;
            return true;
        }

        /// <summary>
        /// Removes the entry with the specified <paramref name="key"/> from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><c>true</c> if the element is successfully found and removed; otherwise <c>false</c>.</returns>
        public virtual new bool Remove(TKey key)
        {
            return TryRemove(key);
        }

        /// <summary>
        /// Removes the entry with the specified <paramref name="key"/> and retrieves the removed value.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">When this method returns, contains the removed value.</param>
        /// <returns><c>true</c> if the element is successfully found and removed; otherwise <c>false</c>.</returns>
        public virtual new bool Remove(TKey key, out TValue value)
        {
            return TryRemove(key, out value);
        }

        /// <summary>
        /// Removes the entry with the specified <paramref name="key"/> if it exists.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns>
        /// <c>true</c> if the entry was found and removed; otherwise <c>false</c>.
        /// </returns>
        public virtual bool TryRemove(TKey key)
        {
            if (!base.Remove(key))
                return false;

            _isRuntimeModified = true;
            return true;
        }

        /// <summary>
        /// Removes the entry with the specified <paramref name="key"/> if it exists
        /// and returns the associated value.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <param name="value">
        /// When this method returns <c>true</c>, contains the value that was removed.
        /// Contains <c>default(TValue)</c> when the method returns <c>false</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the entry was found and removed; otherwise <c>false</c>.
        /// </returns>
        public virtual bool TryRemove(TKey key, out TValue value)
        {
            if (!TryGetValue(key, out value))
                return false;

            base.Remove(key);
            _isRuntimeModified = true;
            return true;
        }

        /// <summary>
        /// Returns the value associated with <paramref name="key"/>, or
        /// <paramref name="defaultValue"/> if the key is not present.
        /// Never throws a <see cref="KeyNotFoundException"/>.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="defaultValue">
        /// The fallback value returned when the key is absent. Defaults to
        /// <c>default(TValue)</c>.
        /// </param>
        /// <returns>The stored value, or <paramref name="defaultValue"/>.</returns>
        public virtual TValue GetOrDefault(TKey key, TValue defaultValue = default)
            => TryGetValue(key, out var value) ? value : defaultValue;

        /// <summary>
        /// Returns the value associated with <paramref name="key"/>. If the key is
        /// absent, the value is created via <paramref name="factory"/>, inserted,
        /// and then returned.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="factory">
        /// A delegate invoked with <paramref name="key"/> to produce the value when
        /// the key is not yet present.
        /// </param>
        /// <returns>
        /// The existing value for <paramref name="key"/>, or the newly created value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
        public virtual TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            if (TryGetValue(key, out var existing))
                return existing;

            var newValue = factory(key);
            Add(key, newValue);
            return newValue;
        }

        /// <summary>
        /// Inserts <paramref name="key"/> with <paramref name="value"/> if the key is
        /// absent, or replaces the existing value if the key is already present.
        /// </summary>
        /// <param name="key">The key to add or update.</param>
        /// <param name="value">The value to store.</param>
        public virtual void AddOrUpdate(TKey key, TValue value)
        {
            base[key] = value;
            _isRuntimeModified = true;
        }

        /// <summary>
        /// Inserts a key produced by <paramref name="addFactory"/> if the key is
        /// absent, or replaces the existing value via <paramref name="updateFactory"/>
        /// if the key is already present.
        /// </summary>
        /// <param name="key">The key to add or update.</param>
        /// <param name="addFactory">
        /// A delegate invoked with the key to produce the value for a new entry.
        /// </param>
        /// <param name="updateFactory">
        /// A delegate invoked with the key and the existing value to produce the
        /// replacement value.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="addFactory"/> or <paramref name="updateFactory"/>
        /// is <c>null</c>.
        /// </exception>
        public virtual void AddOrUpdate(TKey key,
                                        Func<TKey, TValue> addFactory,
                                        Func<TKey, TValue, TValue> updateFactory)
        {
            if (addFactory    == null) throw new ArgumentNullException(nameof(addFactory));
            if (updateFactory == null) throw new ArgumentNullException(nameof(updateFactory));

            if (TryGetValue(key, out var oldValue))
            {
                base[key] = updateFactory(key, oldValue);
            }
            else
            {
                base.Add(key, addFactory(key));
            }
            _isRuntimeModified = true;
        }

        // ── Search Methods ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the first key for which <paramref name="predicate"/> returns
        /// <c>true</c>, or <c>default(TKey)</c> if no match is found.
        /// </summary>
        /// <param name="predicate">
        /// A function that tests each key-value pair. Must not be <c>null</c>.
        /// </param>
        /// <returns>The first matching key, or <c>default(TKey)</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="predicate"/> is <c>null</c>.
        /// </exception>
        public TKey FindKey(Func<TKey, TValue, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (var kvp in this)
                if (predicate(kvp.Key, kvp.Value))
                    return kvp.Key;

            return default;
        }

        /// <summary>
        /// Returns the first value for which <paramref name="predicate"/> returns
        /// <c>true</c>, or <c>default(TValue)</c> if no match is found.
        /// </summary>
        /// <param name="predicate">
        /// A function that tests each key-value pair. Must not be <c>null</c>.
        /// </param>
        /// <returns>The first matching value, or <c>default(TValue)</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="predicate"/> is <c>null</c>.
        /// </exception>
        public TValue FindValue(Func<TKey, TValue, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (var kvp in this)
                if (predicate(kvp.Key, kvp.Value))
                    return kvp.Value;

            return default;
        }

        /// <summary>
        /// Returns a list of all keys whose associated value equals
        /// <paramref name="value"/> using the default equality comparer.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>
        /// A <see cref="List{TKey}"/> containing every key that maps to
        /// <paramref name="value"/>. The list is empty if no matches are found.
        /// </returns>
        public List<TKey> FindAllByValue(TValue value)
        {
            var comparer = EqualityComparer<TValue>.Default;
            var result   = new List<TKey>();

            foreach (var kvp in this)
                if (comparer.Equals(kvp.Value, value))
                    result.Add(kvp.Key);

            return result;
        }

        /// <summary>
        /// Returns a list of all keys whose key-value pair satisfies the specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">A delegate taking key and value returning true for matching pairs.</param>
        /// <returns>A list of matching keys.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null.</exception>
        public List<TKey> FindAllByValue(Func<TKey, TValue, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var result = new List<TKey>();
            foreach (var kvp in this)
                if (predicate(kvp.Key, kvp.Value))
                    result.Add(kvp.Key);

            return result;
        }

        /// <summary>
        /// Returns a new <see cref="SerializableDictionary{TKey,TValue}"/> containing
        /// only the entries for which <paramref name="predicate"/> returns <c>true</c>.
        /// </summary>
        /// <param name="predicate">The filter condition.</param>
        /// <returns>A filtered copy of this dictionary.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="predicate"/> is <c>null</c>.
        /// </exception>
        public SerializableDictionary<TKey, TValue> Filter(Func<TKey, TValue, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var result = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in this)
                if (predicate(kvp.Key, kvp.Value))
                    result.Add(kvp.Key, kvp.Value);

            return result;
        }

        /// <summary>
        /// Projects each value in the dictionary to a new form using
        /// <paramref name="valueSelector"/> and returns the results as a new
        /// <see cref="SerializableDictionary{TKey,TOut}"/> with the same keys.
        /// </summary>
        /// <typeparam name="TOut">The type of the projected values.</typeparam>
        /// <param name="valueSelector">
        /// A transform function applied to each key-value pair.
        /// </param>
        /// <returns>
        /// A new dictionary mapping the same keys to the transformed values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="valueSelector"/> is <c>null</c>.
        /// </exception>
        public SerializableDictionary<TKey, TOut> Map<TOut>(Func<TKey, TValue, TOut> valueSelector)
        {
            if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));

            var result = new SerializableDictionary<TKey, TOut>();
            foreach (var kvp in this)
                result.Add(kvp.Key, valueSelector(kvp.Key, kvp.Value));

            return result;
        }
    }
}
