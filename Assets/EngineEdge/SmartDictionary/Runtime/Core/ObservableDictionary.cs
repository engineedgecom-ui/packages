using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// Serializable UnityEvent that passes the new entry count when dictionary size changes.
    /// </summary>
    [Serializable]
    public class DictionaryCountEvent : UnityEvent<int> { }

    /// <summary>
    /// A reactive, observable dictionary that inherits from <see cref="SerializableDictionary{TKey, TValue}"/>
    /// and fires both C# <see cref="Action"/> delegates and serializable <see cref="UnityEvent"/> callbacks
    /// whenever the dictionary is mutated.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary. Must be serializable by Unity.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary. Must be serializable by Unity.</typeparam>
    [Serializable]
    public class ObservableDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue>
    {
        // ── Events ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Raised after a new entry is successfully added to the dictionary.
        /// </summary>
        /// <remarks>
        /// Parameters: <c>TKey key</c>, <c>TValue value</c>.
        /// Only fires when <see cref="EventsEnabled"/> is <c>true</c>.
        /// </remarks>
        public event Action<TKey, TValue> OnEntryAdded;

        /// <summary>
        /// Raised after an entry is successfully removed from the dictionary.
        /// </summary>
        /// <remarks>
        /// Parameters: <c>TKey key</c>, <c>TValue removedValue</c>.
        /// Only fires when <see cref="EventsEnabled"/> is <c>true</c>.
        /// </remarks>
        public event Action<TKey, TValue> OnEntryRemoved;

        /// <summary>
        /// Raised after an existing entry's value is replaced.
        /// </summary>
        /// <remarks>
        /// Parameters: <c>TKey key</c>, <c>TValue oldValue</c>, <c>TValue newValue</c>.
        /// Only fires when <see cref="EventsEnabled"/> is <c>true</c>.
        /// </remarks>
        public event Action<TKey, TValue, TValue> OnEntryUpdated;

        /// <summary>
        /// Raised after the dictionary is cleared via <see cref="Clear"/>.
        /// Only fires when <see cref="EventsEnabled"/> is <c>true</c>.
        /// </summary>
        public event Action OnCleared;

        /// <summary>
        /// Raised whenever the number of entries in the dictionary changes.
        /// </summary>
        /// <remarks>
        /// Parameter: <c>int newCount</c> — the count after the change.
        /// Only fires when <see cref="EventsEnabled"/> is <c>true</c>.
        /// </remarks>
        public event Action<int> OnCountChanged;

        // ── Configuration ─────────────────────────────────────────────────────────

        [NonSerialized]
        private bool _eventsEnabled = true;

        /// <summary>
        /// Gets or sets a value indicating whether dictionary-mutation events are
        /// fired. Set to <c>false</c> to suppress all event callbacks temporarily
        /// (e.g. during bulk import operations).
        /// </summary>
        /// <value><c>true</c> by default.</value>
        public bool EventsEnabled
        {
            get => _eventsEnabled;
            set => _eventsEnabled = value;
        }

        // ── NonSerialized Unity Events (Data-only JSON serialization) ─────────────

        [NonSerialized]
        private UnityEvent _onEntryAddedEvent = new UnityEvent();

        [NonSerialized]
        private UnityEvent _onEntryRemovedEvent = new UnityEvent();

        [NonSerialized]
        private UnityEvent _onEntryUpdatedEvent = new UnityEvent();

        [NonSerialized]
        private UnityEvent _onClearedEvent = new UnityEvent();

        [NonSerialized]
        private DictionaryCountEvent _onCountChangedEvent = new DictionaryCountEvent();

        /// <summary>Serialized UnityEvent invoked when a new entry is added.</summary>
        public UnityEvent OnEntryAddedEvent => _onEntryAddedEvent;

        /// <summary>Serialized UnityEvent invoked when an entry is removed.</summary>
        public UnityEvent OnEntryRemovedEvent => _onEntryRemovedEvent;

        /// <summary>Serialized UnityEvent invoked when an existing entry's value is replaced.</summary>
        public UnityEvent OnEntryUpdatedEvent => _onEntryUpdatedEvent;

        /// <summary>Serialized UnityEvent invoked when the dictionary is cleared.</summary>
        public UnityEvent OnClearedEvent => _onClearedEvent;

        /// <summary>Serialized UnityEvent invoked with the new count when entries change.</summary>
        public DictionaryCountEvent OnCountChangedEvent => _onCountChangedEvent;

        // ── Constructors ──────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes a new, empty <see cref="ObservableDictionary{TKey, TValue}"/> instance.
        /// </summary>
        public ObservableDictionary() : base() { }

        /// <summary>
        /// Initializes a new <see cref="ObservableDictionary{TKey, TValue}"/> copied from an existing dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        /// <summary>
        /// Initializes a new <see cref="ObservableDictionary{TKey, TValue}"/> using the specified equality comparer.
        /// </summary>
        /// <param name="comparer">The equality comparer to use for keys.</param>
        public ObservableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

        /// <summary>
        /// Initializes a new <see cref="ObservableDictionary{TKey, TValue}"/> with elements copied from
        /// the specified dictionary and using the specified equality comparer.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied.</param>
        /// <param name="comparer">The equality comparer to use for keys.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer) { }

        // ── Overrides with Event Dispatching ──────────────────────────────────────

        /// <inheritdoc/>
        public override TValue this[TKey key]
        {
            get => base[key];
            set
            {
                bool existed = TryGetValue(key, out var oldValue);
                base[key] = value;

                if (EventsEnabled)
                {
                    if (existed)
                    {
                        OnEntryUpdated?.Invoke(key, oldValue, value);
                        _onEntryUpdatedEvent?.Invoke();
                    }
                    else
                    {
                        OnEntryAdded?.Invoke(key, value);
                        _onEntryAddedEvent?.Invoke();
                        OnCountChanged?.Invoke(Count);
                        _onCountChangedEvent?.Invoke(Count);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Add(TKey key, TValue value)
        {
            base.Add(key, value);

            if (EventsEnabled)
            {
                OnEntryAdded?.Invoke(key, value);
                _onEntryAddedEvent?.Invoke();
                OnCountChanged?.Invoke(Count);
                _onCountChangedEvent?.Invoke(Count);
            }
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            base.Clear();

            if (EventsEnabled)
            {
                OnCleared?.Invoke();
                _onClearedEvent?.Invoke();
                OnCountChanged?.Invoke(0);
                _onCountChangedEvent?.Invoke(0);
            }
        }

        /// <inheritdoc/>
        public override bool TryAdd(TKey key, TValue value)
        {
            if (!base.TryAdd(key, value))
                return false;

            if (EventsEnabled)
            {
                OnEntryAdded?.Invoke(key, value);
                _onEntryAddedEvent?.Invoke();
                OnCountChanged?.Invoke(Count);
                _onCountChangedEvent?.Invoke(Count);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool TryRemove(TKey key)
        {
            bool existed = TryGetValue(key, out var removedValue);
            if (!base.TryRemove(key))
                return false;

            if (EventsEnabled && existed)
            {
                OnEntryRemoved?.Invoke(key, removedValue);
                _onEntryRemovedEvent?.Invoke();
                OnCountChanged?.Invoke(Count);
                _onCountChangedEvent?.Invoke(Count);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool TryRemove(TKey key, out TValue value)
        {
            if (!base.TryRemove(key, out value))
                return false;

            if (EventsEnabled)
            {
                OnEntryRemoved?.Invoke(key, value);
                _onEntryRemovedEvent?.Invoke();
                OnCountChanged?.Invoke(Count);
                _onCountChangedEvent?.Invoke(Count);
            }

            return true;
        }

        /// <inheritdoc/>
        public override void AddOrUpdate(TKey key, TValue value)
        {
            bool existed = TryGetValue(key, out var oldValue);
            base.AddOrUpdate(key, value);

            if (EventsEnabled)
            {
                if (existed)
                {
                    OnEntryUpdated?.Invoke(key, oldValue, value);
                    _onEntryUpdatedEvent?.Invoke();
                }
                else
                {
                    OnEntryAdded?.Invoke(key, value);
                    _onEntryAddedEvent?.Invoke();
                    OnCountChanged?.Invoke(Count);
                    _onCountChangedEvent?.Invoke(Count);
                }
            }
        }

        /// <inheritdoc/>
        public override void AddOrUpdate(TKey key,
                                        Func<TKey, TValue> addFactory,
                                        Func<TKey, TValue, TValue> updateFactory)
        {
            bool existed = TryGetValue(key, out var oldValue);
            base.AddOrUpdate(key, addFactory, updateFactory);

            if (EventsEnabled)
            {
                if (existed)
                {
                    OnEntryUpdated?.Invoke(key, oldValue, this[key]);
                    _onEntryUpdatedEvent?.Invoke();
                }
                else
                {
                    OnEntryAdded?.Invoke(key, this[key]);
                    _onEntryAddedEvent?.Invoke();
                    OnCountChanged?.Invoke(Count);
                    _onCountChangedEvent?.Invoke(Count);
                }
            }
        }
    }
}
