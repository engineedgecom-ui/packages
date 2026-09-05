using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// A serializable generic stack that implements
    /// <see cref="ISerializationCallbackReceiver"/> so Unity can persist its contents
    /// across serialization boundaries (e.g. Play-mode, domain reload, asset saves).
    /// </summary>
    /// <remarks>
    /// Uses composition rather than inheritance: an internal <see cref="Stack{T}"/>
    /// holds the live data, and a backing <see cref="List{T}"/> is used exclusively
    /// for Unity serialization. During <see cref="OnBeforeSerialize"/> the stack is
    /// flushed into the list in top-first order; during <see cref="OnAfterDeserialize"/>
    /// the list is iterated in reverse so the original push order is restored.
    /// </remarks>
    /// <typeparam name="T">
    /// The element type. Must be serializable by Unity (e.g. primitives, structs
    /// decorated with <c>[Serializable]</c>, or <see cref="UnityEngine.Object"/>-derived types).
    /// </typeparam>
    [Serializable]
    public class SerializableStack<T> : ISerializationCallbackReceiver, IEnumerable<T>
    {
        // ── Internal state ───────────────────────────────────────────────────────

        /// <summary>
        /// The live stack that backs all runtime operations.
        /// Not serialized directly; mirrored to/from <see cref="_serializedItems"/>.
        /// </summary>
        private Stack<T> _stack = new Stack<T>();

        /// <summary>
        /// The list Unity actually serializes. Populated top-first during
        /// <see cref="OnBeforeSerialize"/> and consumed during
        /// <see cref="OnAfterDeserialize"/>.
        /// </summary>
        [SerializeField]
        private List<T> _serializedItems = new List<T>();

        // ── Events ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Raised after an item is pushed onto the stack via <see cref="Push"/>.
        /// The parameter is the item that was pushed.
        /// </summary>
        public event Action<T> OnPushed;

        /// <summary>
        /// Raised after an item is popped from the stack via <see cref="Pop"/> or
        /// <see cref="TryPop"/>. The parameter is the item that was removed.
        /// </summary>
        public event Action<T> OnPopped;

        /// <summary>
        /// Raised after the stack has been cleared via <see cref="Clear"/>.
        /// </summary>
        public event Action OnCleared;

        // ── Constructor ──────────────────────────────────────────────────────────

        /// <summary>
        /// Initialises a new, empty <see cref="SerializableStack{T}"/>.
        /// </summary>
        public SerializableStack() { }

        // ── ISerializationCallbackReceiver ───────────────────────────────────────

        /// <summary>
        /// Called by Unity immediately before the object is serialized.
        /// Copies the stack into <see cref="_serializedItems"/> in top-first order
        /// so the order can be faithfully restored during deserialization.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _serializedItems.Clear();
            foreach (T item in _stack)
            {
                // Stack<T> enumerates top-first, so the list is already top-first.
                _serializedItems.Add(item);
            }
        }

        /// <summary>
        /// Called by Unity immediately after the object has been deserialized.
        /// Rebuilds the live <see cref="Stack{T}"/> from <see cref="_serializedItems"/>.
        /// The list is pushed in reverse so the element that was at the top of the
        /// stack ends up on top again after reconstruction.
        /// </summary>
        public void OnAfterDeserialize()
        {
            _stack = new Stack<T>(_serializedItems.Count);

            // Push from the bottom of the serialized list upward so the first
            // element in the list (original top) ends up on top of the new stack.
            for (int i = _serializedItems.Count - 1; i >= 0; i--)
            {
                _stack.Push(_serializedItems[i]);
            }
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the number of elements currently on the stack.
        /// </summary>
        public int Count => _stack.Count;

        /// <summary>
        /// Pushes <paramref name="item"/> onto the top of the stack and raises
        /// <see cref="OnPushed"/>.
        /// </summary>
        /// <param name="item">The element to push.</param>
        public void Push(T item)
        {
            _stack.Push(item);
            OnPushed?.Invoke(item);
        }

        /// <summary>
        /// Removes and returns the element at the top of the stack, then raises
        /// <see cref="OnPopped"/>.
        /// </summary>
        /// <returns>The element that was removed from the top of the stack.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the stack is empty.
        /// </exception>
        public T Pop()
        {
            if (_stack.Count == 0)
                throw new InvalidOperationException(
                    "[SerializableStack] Cannot Pop from an empty stack.");

            T item = _stack.Pop();
            OnPopped?.Invoke(item);
            return item;
        }

        /// <summary>
        /// Attempts to remove and return the element at the top of the stack.
        /// Raises <see cref="OnPopped"/> on success.
        /// </summary>
        /// <param name="item">
        /// When this method returns <c>true</c>, contains the element that was
        /// removed from the top of the stack; otherwise the default value for
        /// <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if an element was successfully removed; <c>false</c> if
        /// the stack was empty.
        /// </returns>
        public bool TryPop(out T item)
        {
            if (_stack.Count == 0)
            {
                item = default;
                return false;
            }

            item = _stack.Pop();
            OnPopped?.Invoke(item);
            return true;
        }

        /// <summary>
        /// Returns the element at the top of the stack without removing it.
        /// </summary>
        /// <returns>The element at the top of the stack.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the stack is empty.
        /// </exception>
        public T Peek()
        {
            if (_stack.Count == 0)
                throw new InvalidOperationException(
                    "[SerializableStack] Cannot Peek an empty stack.");

            return _stack.Peek();
        }

        /// <summary>
        /// Attempts to return the element at the top of the stack without removing it.
        /// </summary>
        /// <param name="item">
        /// When this method returns <c>true</c>, contains the element at the top of
        /// the stack; otherwise the default value for <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if an element was available; <c>false</c> if the stack was empty.
        /// </returns>
        public bool TryPeek(out T item)
        {
            if (_stack.Count == 0)
            {
                item = default;
                return false;
            }

            item = _stack.Peek();
            return true;
        }

        /// <summary>
        /// Removes all elements from the stack and raises <see cref="OnCleared"/>.
        /// </summary>
        public void Clear()
        {
            _stack.Clear();
            OnCleared?.Invoke();
        }

        /// <summary>
        /// Determines whether <paramref name="item"/> is present in the stack
        /// using the default equality comparer.
        /// </summary>
        /// <param name="item">The element to locate.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was found; otherwise <c>false</c>.
        /// </returns>
        public bool Contains(T item) => _stack.Contains(item);

        /// <summary>
        /// Copies the stack elements to a new array in top-first order.
        /// </summary>
        /// <returns>
        /// An array containing copies of the stack elements in top-first order.
        /// </returns>
        public T[] ToArray() => _stack.ToArray();

        /// <summary>
        /// Returns an enumerator that iterates through the stack in top-first order.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> for the stack.
        /// </returns>
        public IEnumerator<T> GetEnumerator() => _stack.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
