using System;
using UnityEngine;

namespace EngineEdge.SmartDictionary
{
    /// <summary>
    /// A serializable generic key-value pair that Unity can serialize in the Inspector.
    /// Wraps a key and value of arbitrary types, provided those types are themselves
    /// serializable by Unity.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        [SerializeField]
        private TKey _key;

        [SerializeField]
        private TValue _value;

        // ── Properties ────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the key component of this pair.
        /// </summary>
        public TKey Key
        {
            get => _key;
            set => _key = value;
        }

        /// <summary>
        /// Gets or sets the value component of this pair.
        /// </summary>
        public TValue Value
        {
            get => _value;
            set => _value = value;
        }

        // ── Constructors ──────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes a new <see cref="SerializableKeyValuePair{TKey, TValue}"/> with
        /// default key and value.
        /// </summary>
        public SerializableKeyValuePair() { }

        /// <summary>
        /// Initializes a new <see cref="SerializableKeyValuePair{TKey, TValue}"/> with
        /// the specified key and value.
        /// </summary>
        /// <param name="key">The key for this pair.</param>
        /// <param name="value">The value for this pair.</param>
        public SerializableKeyValuePair(TKey key, TValue value)
        {
            _key   = key;
            _value = value;
        }

        // ── Methods ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Deconstructs this pair into its key and value components, enabling
        /// C# tuple-deconstruction syntax.
        /// </summary>
        /// <param name="key">When this method returns, contains the key.</param>
        /// <param name="value">When this method returns, contains the value.</param>
        /// <example>
        /// <code>
        /// var pair = new SerializableKeyValuePair&lt;string, int&gt;("health", 100);
        /// var (key, value) = pair;
        /// </code>
        /// </example>
        public void Deconstruct(out TKey key, out TValue value)
        {
            key   = _key;
            value = _value;
        }

        /// <summary>
        /// Returns a string representation of this pair in the format
        /// <c>[Key, Value]</c>.
        /// </summary>
        /// <returns>A string that represents the current pair.</returns>
        public override string ToString() => $"[{_key}, {_value}]";
    }
}
