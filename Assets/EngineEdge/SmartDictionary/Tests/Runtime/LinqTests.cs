// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  LinqTests.cs
//  NUnit tests for LINQ extension methods on SerializableDictionary.
// ============================================================

using System.Collections.Generic;
using NUnit.Framework;

namespace EngineEdge.SmartDictionary.Tests
{
    /// <summary>
    /// Tests covering the LINQ-style extension methods exposed by
    /// <c>SerializableDictionaryExtensions</c>.
    /// </summary>
    [TestFixture]
    public class SerializableDictionaryLinqTests
    {
        // ------------------------------------------------------------------ //
        //  Helpers
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Creates a pre-populated <see cref="SerializableDictionary{TKey,TValue}"/>
        /// with string keys and integer values for use in tests.
        /// </summary>
        private SerializableDictionary<string, int> SampleDict()
        {
            var d = new SerializableDictionary<string, int>();
            d.Add("alice", 80);
            d.Add("bob",   45);
            d.Add("carol", 92);
            d.Add("dave",  30);
            return d;
        }

        // ------------------------------------------------------------------ //
        //  ToSerializableDictionary
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>ToSerializableDictionary</c> correctly builds a dictionary
        /// from a plain <see cref="List{T}"/>.
        /// </summary>
        [Test]
        public void ToSerializableDictionary_FromList_Works()
        {
            var list = new List<string> { "cat", "dog", "fish" };
            var dict = list.ToSerializableDictionary(s => s, s => s.Length);

            Assert.AreEqual(3,   dict.Count);
            Assert.AreEqual(3,   dict["cat"]);
            Assert.AreEqual(3,   dict["dog"]);
            Assert.AreEqual(4,   dict["fish"]);
        }

        // ------------------------------------------------------------------ //
        //  WhereDict
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>WhereDict</c> returns only entries satisfying the predicate.
        /// </summary>
        [Test]
        public void WhereDict_FiltersCorrectly()
        {
            var dict   = SampleDict();
            var result = dict.WhereDict((k, v) => v > 50);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("alice"));
            Assert.IsTrue(result.ContainsKey("carol"));
        }

        // ------------------------------------------------------------------ //
        //  SelectValues
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>SelectValues</c> projects every value through the selector.
        /// </summary>
        [Test]
        public void SelectValues_TransformsValues()
        {
            var dict   = SampleDict();
            var result = dict.SelectValues((k, v) => v.ToString() + "pts");

            Assert.AreEqual("80pts", result["alice"]);
            Assert.AreEqual("45pts", result["bob"]);
        }

        // ------------------------------------------------------------------ //
        //  OrderByKey / OrderByValue
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>OrderByKey</c> returns keys in ascending alphabetical order.
        /// </summary>
        [Test]
        public void OrderByKey_SortsAscending()
        {
            var dict   = SampleDict();
            var sorted = dict.OrderByKey();

            var keys = new List<string>(sorted.Keys);
            Assert.AreEqual("alice", keys[0]);
            Assert.AreEqual("bob",   keys[1]);
            Assert.AreEqual("carol", keys[2]);
            Assert.AreEqual("dave",  keys[3]);
        }

        /// <summary>
        /// <c>OrderByValue</c> returns entries sorted by value ascending.
        /// </summary>
        [Test]
        public void OrderByValue_SortsAscending()
        {
            var dict   = SampleDict();
            var sorted = dict.OrderByValue();

            var keys = new List<string>(sorted.Keys);
            Assert.AreEqual("dave",  keys[0]);
            Assert.AreEqual("bob",   keys[1]);
            Assert.AreEqual("alice", keys[2]);
            Assert.AreEqual("carol", keys[3]);
        }

        // ------------------------------------------------------------------ //
        //  Aggregates
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>SumValues</c> (int overload) returns the correct sum.
        /// </summary>
        [Test]
        public void SumValues_Int_ReturnsCorrectSum()
        {
            var dict = SampleDict();
            Assert.AreEqual(247, dict.SumValues());
        }

        /// <summary>
        /// <c>SumValues</c> (float overload) returns the correct sum.
        /// </summary>
        [Test]
        public void SumValues_Float_ReturnsCorrectSum()
        {
            var dict = new SerializableDictionary<string, float>();
            dict.Add("a", 1.5f);
            dict.Add("b", 2.5f);

            Assert.AreEqual(4.0f, dict.SumValues(), 0.0001f);
        }

        /// <summary>
        /// <c>AverageValues</c> returns the arithmetic mean of all values.
        /// </summary>
        [Test]
        public void AverageValues_ReturnsCorrectAverage()
        {
            var dict    = SampleDict();        // 80+45+92+30 = 247 / 4 = 61.75
            var average = dict.AverageValues();
            Assert.AreEqual(61.75, average, 0.001);
        }

        /// <summary>
        /// <c>MinValue</c> returns the smallest value in the dictionary.
        /// </summary>
        [Test]
        public void MinValue_ReturnsMinimum()
        {
            var dict = SampleDict();
            Assert.AreEqual(30, dict.MinValue());
        }

        /// <summary>
        /// <c>MaxValue</c> returns the largest value in the dictionary.
        /// </summary>
        [Test]
        public void MaxValue_ReturnsMaximum()
        {
            var dict = SampleDict();
            Assert.AreEqual(92, dict.MaxValue());
        }
    }
}
