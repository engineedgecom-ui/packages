using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// A serializable generic queue that implements
    /// <see cref="ISerializationCallbackReceiver"/> so Unity can persist its contents
    /// across serialization boundaries (e.g. Play-mode, domain reload, asset saves).
    /// </summary>
    /// <remarks>
    /// Uses composition rather than inheritance: an internal <see cref="Queue{T}"/>
    /// holds the live data, and a backing <see cref="List{T}"/> is used exclusively
    /// for Unity serialization. During <see cref="OnBeforeSerialize"/> the queue is
    /// flushed into the list in front-to-back order; during
    /// <see cref="OnAfterDeserialize"/> the list is re-enqueued in the same order so
    /// the original FIFO ordering is preserved.
    /// </remarks>
    /// <typeparam name="T">
    /// The element type. Must be serializable by Unity (e.g. primitives, structs
    /// decorated with <c>[Serializable]</c>, or <see cref="UnityEngine.Object"/>-derived types).
    /// </typeparam>
    [Serializable]
    public class SerializableQueue<T> : ISerializationCallbackReceiver, IEnumerable<T>
    {
        // ── Internal state ───────────────────────────────────────────────────────

        /// <summary>
        /// The live queue that backs all runtime operations.
        /// Not serialized directly; mirrored to/from <see cref="_serializedItems"/>.
        /// </summary>
        private Queue<T> _queue = new Queue<T>();

        /// <summary>
        /// The list Unity actually serializes. Populated front-to-back during
        /// <see cref="OnBeforeSerialize"/> and consumed during
        /// <see cref="OnAfterDeserialize"/>.
        /// </summary>
        [SerializeField]
        private List<T> _serializedItems = new List<T>();

        // ── Events ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Raised after an item is enqueued via <see cref="Enqueue"/>.
        /// The parameter is the item that was added.
        /// </summary>
        public event Action<T> OnEnqueued;

        /// <summary>
        /// Raised after an item is dequeued via <see cref="Dequeue"/> or
        /// <see cref="TryDequeue"/>. The parameter is the item that was removed.
        /// </summary>
        public event Action<T> OnDequeued;

        /// <summary>
        /// Raised after the queue has been cleared via <see cref="Clear"/>.
        /// </summary>
        public event Action OnCleared;

        // ── Constructor ──────────────────────────────────────────────────────────

        /// <summary>
        /// Initialises a new, empty <see cref="SerializableQueue{T}"/>.
        /// </summary>
        public SerializableQueue() { }

        // ── ISerializationCallbackReceiver ───────────────────────────────────────

        /// <summary>
        /// Called by Unity immediately before the object is serialized.
        /// Copies the queue into <see cref="_serializedItems"/> in front-to-back order
        /// so the FIFO ordering can be faithfully restored during deserialization.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _serializedItems.Clear();
            foreach (T item in _queue)
            {
                // Queue<T> enumerates front-to-back.
                _serializedItems.Add(item);
            }
        }

        /// <summary>
        /// Called by Unity immediately after the object has been deserialized.
        /// Rebuilds the live <see cref="Queue{T}"/> from <see cref="_serializedItems"/>
        /// in front-to-back order, preserving the original FIFO sequence.
        /// </summary>
        public void OnAfterDeserialize()
        {
            _queue = new Queue<T>(_serializedItems.Count);
            foreach (T item in _serializedItems)
            {
                _queue.Enqueue(item);
            }
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the number of elements currently in the queue.
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// Adds <paramref name="item"/> to the back of the queue and raises
        /// <see cref="OnEnqueued"/>.
        /// </summary>
        /// <param name="item">The element to add to the queue.</param>
        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
            OnEnqueued?.Invoke(item);
        }

        /// <summary>
        /// Removes and returns the element at the front of the queue, then raises
        /// <see cref="OnDequeued"/>.
        /// </summary>
        /// <returns>The element that was removed from the front of the queue.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the queue is empty.
        /// </exception>
        public T Dequeue()
        {
            if (_queue.Count == 0)
                throw new InvalidOperationException(
                    "[SerializableQueue] Cannot Dequeue from an empty queue.");

            T item = _queue.Dequeue();
            OnDequeued?.Invoke(item);
            return item;
        }

        /// <summary>
        /// Attempts to remove and return the element at the front of the queue.
        /// Raises <see cref="OnDequeued"/> on success.
        /// </summary>
        /// <param name="item">
        /// When this method returns <c>true</c>, contains the element that was
        /// removed from the front of the queue; otherwise the default value for
        /// <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if an element was successfully removed; <c>false</c> if
        /// the queue was empty.
        /// </returns>
        public bool TryDequeue(out T item)
        {
            if (_queue.Count == 0)
            {
                item = default;
                return false;
            }

            item = _queue.Dequeue();
            OnDequeued?.Invoke(item);
            return true;
        }

        /// <summary>
        /// Returns the element at the front of the queue without removing it.
        /// </summary>
        /// <returns>The element at the front of the queue.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the queue is empty.
        /// </exception>
        public T Peek()
        {
            if (_queue.Count == 0)
                throw new InvalidOperationException(
                    "[SerializableQueue] Cannot Peek an empty queue.");

            return _queue.Peek();
        }

        /// <summary>
        /// Attempts to return the element at the front of the queue without removing it.
        /// </summary>
        /// <param name="item">
        /// When this method returns <c>true</c>, contains the element at the front
        /// of the queue; otherwise the default value for <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if an element was available; <c>false</c> if the queue was empty.
        /// </returns>
        public bool TryPeek(out T item)
        {
            if (_queue.Count == 0)
            {
                item = default;
                return false;
            }

            item = _queue.Peek();
            return true;
        }

        /// <summary>
        /// Removes all elements from the queue and raises <see cref="OnCleared"/>.
        /// </summary>
        public void Clear()
        {
            _queue.Clear();
            OnCleared?.Invoke();
        }

        /// <summary>
        /// Determines whether <paramref name="item"/> is present in the queue
        /// using the default equality comparer.
        /// </summary>
        /// <param name="item">The element to locate.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was found; otherwise <c>false</c>.
        /// </returns>
        public bool Contains(T item) => _queue.Contains(item);

        /// <summary>
        /// Copies the queue elements to a new array in front-to-back order.
        /// </summary>
        /// <returns>
        /// An array containing copies of the queue elements in front-to-back order.
        /// </returns>
        public T[] ToArray() => _queue.ToArray();

        /// <summary>
        /// Returns an enumerator that iterates through the queue in front-to-back order.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> for the queue.
        /// </returns>
        public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
