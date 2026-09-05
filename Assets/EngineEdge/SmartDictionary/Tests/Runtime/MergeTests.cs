// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  MergeTests.cs
//  NUnit tests for merge / set-operation methods.
// ============================================================

using NUnit.Framework;

namespace EngineEdge.SmartDictionary.Tests
{
    /// <summary>
    /// Tests covering merge, set-algebra and cloning operations on
    /// <see cref="SerializableDictionary{TKey,TValue}"/>.
    /// </summary>
    [TestFixture]
    public class SerializableDictionaryMergeTests
    {
        // ------------------------------------------------------------------ //
        //  Helpers
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Builds a populated source dictionary {a=1, b=2}.
        /// </summary>
        private SerializableDictionary<string, int> Source()
        {
            var d = new SerializableDictionary<string, int>();
            d.Add("a", 1);
            d.Add("b", 2);
            return d;
        }

        /// <summary>
        /// Builds a second dictionary {b=20, c=30}.
        /// </summary>
        private SerializableDictionary<string, int> Other()
        {
            var d = new SerializableDictionary<string, int>();
            d.Add("b", 20);
            d.Add("c", 30);
            return d;
        }

        // ------------------------------------------------------------------ //
        //  Merge (non-overwrite)
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>Merge</c> adds keys from the other dictionary that do not yet exist.
        /// </summary>
        [Test]
        public void Merge_AddsNonExistingKeys()
        {
            var src = Source();   // a=1, b=2
            src.Merge(Other());   // other: b=20, c=30

            Assert.IsTrue(src.ContainsKey("c"));
            Assert.AreEqual(30, src["c"]);
        }

        /// <summary>
        /// <c>Merge</c> (non-overwrite) does NOT change existing keys.
        /// </summary>
        [Test]
        public void Merge_SkipsExistingKeys()
        {
            var src = Source();   // b=2
            src.Merge(Other());   // other has b=20

            Assert.AreEqual(2, src["b"]);  // unchanged
        }

        // ------------------------------------------------------------------ //
        //  MergeOverwrite
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>MergeOverwrite</c> replaces existing values with values from
        /// the other dictionary.
        /// </summary>
        [Test]
        public void MergeOverwrite_OverwritesExistingKeys()
        {
            var src = Source();            // b=2
            src.MergeOverwrite(Other());   // other has b=20

            Assert.AreEqual(20, src["b"]);
        }

        // ------------------------------------------------------------------ //
        //  Intersect
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>Intersect</c> returns only the keys present in both dictionaries.
        /// </summary>
        [Test]
        public void Intersect_KeepsOnlyCommonKeys()
        {
            var src    = Source();   // a, b
            var result = src.Intersect(Other()); // other: b, c  →  common: b

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("b"));
            Assert.IsFalse(result.ContainsKey("a"));
            Assert.IsFalse(result.ContainsKey("c"));
        }

        // ------------------------------------------------------------------ //
        //  Except
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>Except</c> removes all keys that appear in the other dictionary.
        /// </summary>
        [Test]
        public void Except_RemovesSpecifiedKeys()
        {
            var src    = Source();          // a=1, b=2
            var result = src.Except(Other()); // remove b  →  {a=1}

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("a"));
            Assert.IsFalse(result.ContainsKey("b"));
        }

        // ------------------------------------------------------------------ //
        //  Union
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>Union</c> combines both dictionaries, preferring the source
        /// value for conflicting keys.
        /// </summary>
        [Test]
        public void Union_CombinesBothPreferringSource()
        {
            var src    = Source();          // a=1, b=2
            var result = src.Union(Other()); // other: b=20, c=30  →  {a=1,b=2,c=30}

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1,  result["a"]);
            Assert.AreEqual(2,  result["b"]);  // source wins
            Assert.AreEqual(30, result["c"]);
        }

        // ------------------------------------------------------------------ //
        //  Clone
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>Clone</c> produces an independent shallow copy; mutations to the
        /// clone do not affect the original.
        /// </summary>
        [Test]
        public void Clone_IsShallowCopy()
        {
            var src   = Source();
            var clone = src.Clone();

            // Verify all keys copied
            Assert.AreEqual(src.Count, clone.Count);
            Assert.AreEqual(1, clone["a"]);
            Assert.AreEqual(2, clone["b"]);

            // Mutate clone, original must be unaffected
            clone.Add("z", 99);
            Assert.IsFalse(src.ContainsKey("z"));
        }
    }
}
