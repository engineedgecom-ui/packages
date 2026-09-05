// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  CoreTests.cs
//  NUnit runtime tests for core SerializableDictionary behaviour.
// ============================================================

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EngineEdge.SmartDictionary.Tests
{
    /// <summary>
    /// Core behavioural tests for <see cref="SerializableDictionary{TKey,TValue}"/>.
    /// </summary>
    [TestFixture]
    public class SerializableDictionaryTests
    {
        // ------------------------------------------------------------------ //
        //  Helpers
        // ------------------------------------------------------------------ //

        /// <summary>Creates a fresh, empty dictionary for each test.</summary>
        private SerializableDictionary<string, int> NewDict()
            => new SerializableDictionary<string, int>();

        // ------------------------------------------------------------------ //
        //  Add / Remove
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Adding a key makes <c>ContainsKey</c> return <see langword="true"/>.
        /// </summary>
        [Test]
        public void BasicAdd_KeyExists()
        {
            var dict = NewDict();
            dict.Add("apple", 1);
            Assert.IsTrue(dict.ContainsKey("apple"));
        }

        /// <summary>
        /// Removing a key makes <c>ContainsKey</c> return <see langword="false"/>.
        /// </summary>
        [Test]
        public void BasicRemove_KeyGone()
        {
            var dict = NewDict();
            dict.Add("apple", 1);
            dict.Remove("apple");
            Assert.IsFalse(dict.ContainsKey("apple"));
        }

        // ------------------------------------------------------------------ //
        //  TryAdd / TryRemove
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>TryAdd</c> returns <see langword="false"/> when a duplicate key is used.
        /// </summary>
        [Test]
        public void TryAdd_DuplicateKey_ReturnsFalse()
        {
            var dict = NewDict();
            dict.TryAdd("key", 1);
            var result = dict.TryAdd("key", 2);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// <c>TryRemove</c> returns <see langword="false"/> for an absent key.
        /// </summary>
        [Test]
        public void TryRemove_MissingKey_ReturnsFalse()
        {
            var dict = NewDict();
            var result = dict.TryRemove("missing");
            Assert.IsFalse(result);
        }

        // ------------------------------------------------------------------ //
        //  GetOrDefault
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>GetOrDefault</c> returns the fallback value when the key is absent.
        /// </summary>
        [Test]
        public void GetOrDefault_MissingKey_ReturnsDefault()
        {
            var dict   = NewDict();
            var result = dict.GetOrDefault("ghost", 99);
            Assert.AreEqual(99, result);
        }

        /// <summary>
        /// <c>GetOrDefault</c> returns the stored value when the key is present.
        /// </summary>
        [Test]
        public void GetOrDefault_ExistingKey_ReturnsValue()
        {
            var dict = NewDict();
            dict.Add("hp", 100);
            Assert.AreEqual(100, dict.GetOrDefault("hp", 0));
        }

        // ------------------------------------------------------------------ //
        //  GetOrAdd
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>GetOrAdd</c> calls the factory, stores and returns the value for a new key.
        /// </summary>
        [Test]
        public void GetOrAdd_NewKey_AddsAndReturns()
        {
            var dict    = NewDict();
            var factoryCalled = false;

            var result = dict.GetOrAdd("score", k =>
            {
                factoryCalled = true;
                return 42;
            });

            Assert.IsTrue(factoryCalled);
            Assert.AreEqual(42, result);
            Assert.IsTrue(dict.ContainsKey("score"));
        }

        /// <summary>
        /// <c>GetOrAdd</c> does NOT call the factory when the key already exists.
        /// </summary>
        [Test]
        public void GetOrAdd_ExistingKey_ReturnsExisting()
        {
            var dict = NewDict();
            dict.Add("score", 10);

            var factoryCalled = false;
            var result = dict.GetOrAdd("score", k =>
            {
                factoryCalled = true;
                return 999;
            });

            Assert.IsFalse(factoryCalled);
            Assert.AreEqual(10, result);
        }

        // ------------------------------------------------------------------ //
        //  AddOrUpdate
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>AddOrUpdate</c> adds a new key when it does not exist.
        /// </summary>
        [Test]
        public void AddOrUpdate_NewKey_Adds()
        {
            var dict = NewDict();
            dict.AddOrUpdate("mana", 50);
            Assert.AreEqual(50, dict["mana"]);
        }

        /// <summary>
        /// <c>AddOrUpdate</c> overwrites an existing key's value.
        /// </summary>
        [Test]
        public void AddOrUpdate_ExistingKey_Updates()
        {
            var dict = NewDict();
            dict.Add("mana", 50);
            dict.AddOrUpdate("mana", 200);
            Assert.AreEqual(200, dict["mana"]);
        }

        // ------------------------------------------------------------------ //
        //  Find helpers
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>FindKey</c> returns the first key that satisfies the predicate.
        /// </summary>
        [Test]
        public void FindKey_ReturnsCorrectKey()
        {
            var dict = NewDict();
            dict.Add("alpha", 1);
            dict.Add("beta", 2);
            dict.Add("gamma", 3);

            var key = dict.FindKey((k, v) => v == 2);
            Assert.AreEqual("beta", key);
        }

        /// <summary>
        /// <c>FindValue</c> returns the first value that satisfies the predicate.
        /// </summary>
        [Test]
        public void FindValue_ReturnsCorrectValue()
        {
            var dict = NewDict();
            dict.Add("a", 10);
            dict.Add("b", 20);

            var value = dict.FindValue((k, v) => k == "b");
            Assert.AreEqual(20, value);
        }

        /// <summary>
        /// <c>FindAllByValue</c> returns all keys whose values match the predicate.
        /// </summary>
        [Test]
        public void FindAllByValue_ReturnsAllMatchingKeys()
        {
            var dict = NewDict();
            dict.Add("x", 5);
            dict.Add("y", 5);
            dict.Add("z", 9);

            var keys = dict.FindAllByValue((k, v) => v == 5);
            Assert.AreEqual(2, keys.Count);
            Assert.IsTrue(keys.Contains("x"));
            Assert.IsTrue(keys.Contains("y"));
        }

        // ------------------------------------------------------------------ //
        //  Filter / Map
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>Filter</c> returns a new dictionary containing only entries
        /// matching the predicate.
        /// </summary>
        [Test]
        public void Filter_ReturnsFilteredDict()
        {
            var dict = NewDict();
            dict.Add("low",  3);
            dict.Add("mid",  50);
            dict.Add("high", 99);

            var result = dict.Filter((k, v) => v >= 50);
            Assert.AreEqual(2, result.Count);
            Assert.IsFalse(result.ContainsKey("low"));
        }

        /// <summary>
        /// <c>Map</c> transforms every value via the provided selector.
        /// </summary>
        [Test]
        public void Map_TransformsValues()
        {
            var dict = NewDict();
            dict.Add("a", 1);
            dict.Add("b", 2);

            var result = dict.Map((k, v) => v * 10);
            Assert.AreEqual(10, result["a"]);
            Assert.AreEqual(20, result["b"]);
        }

        // ------------------------------------------------------------------ //
        //  Clear
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>Clear</c> removes all entries.
        /// </summary>
        [Test]
        public void Clear_EmptiesDict()
        {
            var dict = NewDict();
            dict.Add("a", 1);
            dict.Add("b", 2);
            dict.Clear();
            Assert.AreEqual(0, dict.Count);
        }

        // ------------------------------------------------------------------ //
        //  Foreach / Deconstruct
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Iterating with <c>foreach</c> visits every entry exactly once.
        /// </summary>
        [Test]
        public void Foreach_IteratesAllEntries()
        {
            var dict = NewDict();
            dict.Add("a", 1);
            dict.Add("b", 2);
            dict.Add("c", 3);

            var visited = new HashSet<string>();
            foreach (var kvp in dict)
                visited.Add(kvp.Key);

            Assert.AreEqual(3, visited.Count);
        }

        /// <summary>
        /// Tuple deconstruct syntax works in <c>foreach</c>.
        /// </summary>
        [Test]
        public void DeconstructSyntax_Works()
        {
            var dict = NewDict();
            dict.Add("hp",   100);
            dict.Add("mana",  50);

            var sum = 0;
            foreach (var (k, v) in dict)
                sum += v;

            Assert.AreEqual(150, sum);
        }

        // ------------------------------------------------------------------ //
        //  Serialization & Deserialization
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Calling OnBeforeSerialize flushes runtime changes into serialized backing store,
        /// and OnAfterDeserialize rebuilds the dictionary.
        /// </summary>
        [Test]
        public void Serialization_RoundTrip_PreservesData()
        {
            var dict = NewDict();
            dict.Add("Potion", 10);
            dict.Add("Sword", 1);

            dict.OnBeforeSerialize();

            var restored = new SerializableDictionary<string, int>();
            // Simulate deserialization
            dict.OnBeforeSerialize();
            string json = dict.ToJson();
            restored.FromJson(json);

            Assert.AreEqual(2, restored.Count);
            Assert.AreEqual(10, restored["Potion"]);
            Assert.AreEqual(1, restored["Sword"]);
        }

        /// <summary>
        /// Modifying dictionary via runtime API flags it so OnBeforeSerialize updates backing store.
        /// </summary>
        [Test]
        public void Serialization_RuntimeModifications_FlushedOnSerialize()
        {
            var dict = NewDict();
            dict["Gold"] = 500;
            dict.AddOrUpdate("Silver", 200);

            dict.OnBeforeSerialize();

            Assert.AreEqual(500, dict.GetOrDefault("Gold"));
            Assert.AreEqual(200, dict.GetOrDefault("Silver"));
        }
    }
}
