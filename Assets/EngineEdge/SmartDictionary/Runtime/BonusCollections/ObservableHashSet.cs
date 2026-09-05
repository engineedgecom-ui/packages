using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// Serializable UnityEvent that passes the new item count when set size changes.
    /// </summary>
    [Serializable]
    public class HashSetCountEvent : UnityEvent<int> { }

    /// <summary>
    /// A reactive, observable hash set that inherits from <see cref="SerializableHashSet{T}"/>
    /// and fires both C# <see cref="Action"/> delegates and serializable <see cref="UnityEvent"/> callbacks
    /// whenever items are added, removed, or cleared.
    /// </summary>
    /// <typeparam name="T">The element type. Must be serializable by Unity.</typeparam>
    [Serializable]
    public class ObservableHashSet<T> : SerializableHashSet<T>
    {
        // ── Events ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Raised after an item is successfully added to the set.
        /// </summary>
        public event Action<T> OnItemAdded;

        /// <summary>
        /// Raised after an item is successfully removed from the set.
        /// </summary>
        public event Action<T> OnItemRemoved;

        /// <summary>
        /// Raised after the set has been cleared via <see cref="Clear"/>.
        /// </summary>
        public event Action OnCleared;

        /// <summary>
        /// Raised whenever the number of items in the set changes.
        /// </summary>
        public event Action<int> OnCountChanged;

        // ── Configuration ─────────────────────────────────────────────────────────

        [SerializeField]
        [Tooltip("If true, set-mutation events (OnItemAdded, OnItemRemoved, etc.) will be dispatched.")]
        private bool _eventsEnabled = true;

        /// <summary>
        /// Gets or sets a value indicating whether set-mutation events are fired.
        /// </summary>
        /// <value><c>true</c> by default.</value>
        public bool EventsEnabled
        {
            get => _eventsEnabled;
            set => _eventsEnabled = value;
        }

        // ── Serialized Unity Events ───────────────────────────────────────────────

        [SerializeField]
        [Tooltip("Fired when a new item is added (only if EventsEnabled is true).")]
        private UnityEvent _onItemAddedEvent = new UnityEvent();

        [SerializeField]
        [Tooltip("Fired when an item is removed (only if EventsEnabled is true).")]
        private UnityEvent _onItemRemovedEvent = new UnityEvent();

        [SerializeField]
        [Tooltip("Fired when the set is cleared (only if EventsEnabled is true).")]
        private UnityEvent _onClearedEvent = new UnityEvent();

        [SerializeField]
        [Tooltip("Fired with the new count when items change (only if EventsEnabled is true).")]
        private HashSetCountEvent _onCountChangedEvent = new HashSetCountEvent();

        /// <summary>Serialized UnityEvent invoked when a new item is added.</summary>
        public UnityEvent OnItemAddedEvent => _onItemAddedEvent;

        /// <summary>Serialized UnityEvent invoked when an item is removed.</summary>
        public UnityEvent OnItemRemovedEvent => _onItemRemovedEvent;

        /// <summary>Serialized UnityEvent invoked when the set is cleared.</summary>
        public UnityEvent OnClearedEvent => _onClearedEvent;

        /// <summary>Serialized UnityEvent invoked with the new count when items change.</summary>
        public HashSetCountEvent OnCountChangedEvent => _onCountChangedEvent;

        // ── Constructors ─────────────────────────────────────────────────────────

        /// <summary>
        /// Initialises a new, empty <see cref="ObservableHashSet{T}"/> using the
        /// default equality comparer for <typeparamref name="T"/>.
        /// </summary>
        public ObservableHashSet() : base() { }

        /// <summary>
        /// Initialises a new <see cref="ObservableHashSet{T}"/> containing the
        /// distinct elements from <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The source collection whose elements are copied into the set.</param>
        public ObservableHashSet(IEnumerable<T> collection) : base(collection) { }

        // ── Overrides with Event Dispatching ──────────────────────────────────────

        /// <inheritdoc/>
        public override bool Add(T item)
        {
            bool added = base.Add(item);
            if (added && _eventsEnabled)
            {
                OnItemAdded?.Invoke(item);
                _onItemAddedEvent?.Invoke();
                OnCountChanged?.Invoke(Count);
                _onCountChangedEvent?.Invoke(Count);
            }
            return added;
        }

        /// <inheritdoc/>
        public override bool Remove(T item)
        {
            bool removed = base.Remove(item);
            if (removed && _eventsEnabled)
            {
                OnItemRemoved?.Invoke(item);
                _onItemRemovedEvent?.Invoke();
                OnCountChanged?.Invoke(Count);
                _onCountChangedEvent?.Invoke(Count);
            }
            return removed;
        }

        /// <inheritdoc/>
        public override bool TryAdd(T item)
        {
            return Add(item);
        }

        /// <inheritdoc/>
        public override bool TryRemove(T item)
        {
            return Remove(item);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            base.Clear();
            if (_eventsEnabled)
            {
                OnCleared?.Invoke();
                _onClearedEvent?.Invoke();
                OnCountChanged?.Invoke(0);
                _onCountChangedEvent?.Invoke(0);
            }
        }
    }
}
