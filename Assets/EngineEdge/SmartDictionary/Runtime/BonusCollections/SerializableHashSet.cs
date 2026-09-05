using System;
using System.Collections.Generic;
using UnityEngine;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// A serializable generic <see cref="HashSet{T}"/> that implements
    /// <see cref="ISerializationCallbackReceiver"/> so Unity can persist its contents
    /// across serialization boundaries (e.g. Play-mode, domain reload, asset saves).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Because Unity cannot natively serialize a <see cref="HashSet{T}"/>, this class
    /// mirrors the set into a backing <see cref="List{T}"/> during
    /// <see cref="OnBeforeSerialize"/> and rebuilds the set from that list during
    /// <see cref="OnAfterDeserialize"/>. Duplicate entries encountered during
    /// deserialization are silently discarded after a warning is logged.
    /// </para>
    /// <para>
    /// For reactive, event-driven sets with C# Actions and serialized UnityEvents,
    /// see <see cref="ObservableHashSet{T}"/>.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">
    /// The element type. Must be serializable by Unity (e.g. primitives, structs
    /// decorated with <c>[Serializable]</c>, or <see cref="UnityEngine.Object"/>-derived types).
    /// </typeparam>
    [Serializable]
    public class SerializableHashSet<T> : HashSet<T>, ISerializationCallbackReceiver
    {
        // ── Serialized backing store ─────────────────────────────────────────────

        /// <summary>
        /// The list Unity actually serializes. It is kept in sync with the live
        /// <see cref="HashSet{T}"/> contents via the
        /// <see cref="ISerializationCallbackReceiver"/> callbacks.
        /// </summary>
        [SerializeField]
        private List<T> _serializedItems = new List<T>();

        [NonSerialized]
        protected bool _isRuntimeModified = false;

        // ── Constructors ─────────────────────────────────────────────────────────

        /// <summary>
        /// Initialises a new, empty <see cref="SerializableHashSet{T}"/> using the
        /// default equality comparer for <typeparamref name="T"/>.
        /// </summary>
        public SerializableHashSet() : base() { }

        /// <summary>
        /// Initialises a new <see cref="SerializableHashSet{T}"/> containing the
        /// distinct elements from <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">
        /// The source collection whose elements are copied into the set.
        /// Duplicate elements are automatically collapsed by the base
        /// <see cref="HashSet{T}"/> constructor.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        public SerializableHashSet(IEnumerable<T> collection) : base(collection)
        {
            _isRuntimeModified = true;
        }

        // ── ISerializationCallbackReceiver ───────────────────────────────────────

        /// <summary>
        /// Called by Unity immediately before the object is serialized.
        /// Flushes the current set contents into <see cref="_serializedItems"/>
        /// so Unity can persist them.
        /// </summary>
        public void OnBeforeSerialize()
        {
            if (_isRuntimeModified || (_serializedItems.Count == 0 && Count > 0))
            {
                _serializedItems.Clear();
                foreach (T item in this)
                {
                    _serializedItems.Add(item);
                }
                _isRuntimeModified = false;
            }
        }

        /// <summary>
        /// Called by Unity immediately after the object has been deserialized.
        /// Rebuilds the live <see cref="HashSet{T}"/> from <see cref="_serializedItems"/>.
        /// Any duplicate entries found in the serialized list are skipped.
        /// </summary>
        public void OnAfterDeserialize()
        {
            base.Clear();

            for (int i = 0; i < _serializedItems.Count; i++)
            {
                T item = _serializedItems[i];
                if (item == null) continue;
                if (Contains(item))
                {
                    continue;
                }
                base.Add(item);
            }
            _isRuntimeModified = false;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Adds an item to the set and tracks modification for serialization.
        /// </summary>
        /// <param name="item">The element to add.</param>
        /// <returns><c>true</c> if the element is added; <c>false</c> if it already exists.</returns>
        public virtual new bool Add(T item)
        {
            bool added = base.Add(item);
            if (added)
            {
                _isRuntimeModified = true;
            }
            return added;
        }

        /// <summary>
        /// Removes an item from the set and tracks modification for serialization.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns><c>true</c> if the element is found and removed; otherwise <c>false</c>.</returns>
        public virtual new bool Remove(T item)
        {
            bool removed = base.Remove(item);
            if (removed)
            {
                _isRuntimeModified = true;
            }
            return removed;
        }

        /// <summary>
        /// Attempts to add <paramref name="item"/> to the set.
        /// </summary>
        /// <param name="item">The element to add.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was added;
        /// <c>false</c> if it was already present.
        /// </returns>
        public virtual bool TryAdd(T item)
        {
            return Add(item);
        }

        /// <summary>
        /// Attempts to remove <paramref name="item"/> from the set.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was found and removed;
        /// <c>false</c> if it was not present.
        /// </returns>
        public virtual bool TryRemove(T item)
        {
            return Remove(item);
        }

        /// <summary>
        /// Returns a new <see cref="SerializableHashSet{T}"/> that contains only
        /// those elements from this set that satisfy <paramref name="predicate"/>.
        /// The original set is not modified.
        /// </summary>
        /// <param name="predicate">
        /// A function that returns <c>true</c> for elements that should be included
        /// in the result set.
        /// </param>
        /// <returns>
        /// A new <see cref="SerializableHashSet{T}"/> containing the filtered elements.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="predicate"/> is <c>null</c>.
        /// </exception>
        public SerializableHashSet<T> Filter(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var result = new SerializableHashSet<T>();
            foreach (T item in this)
            {
                if (predicate(item))
                    result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Removes all elements from the set.
        /// </summary>
        public virtual new void Clear()
        {
            base.Clear();
            _isRuntimeModified = true;
        }
    }
}
