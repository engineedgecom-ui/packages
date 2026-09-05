// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  ClassAndStructTests.cs
//  NUnit tests verifying all collections with custom Class Keys and Struct Values.
// ============================================================

using NUnit.Framework;
using EngineEdge.SmartDictionary.Example;

namespace EngineEdge.SmartDictionary.Tests
{
    [TestFixture]
    public class ClassAndStructTests
    {
        // ------------------------------------------------------------------ //
        //  1. SerializableDictionary: Class Key -> Struct Value
        // ------------------------------------------------------------------ //

        [Test]
        public void SerializableDictionary_ClassKey_StructValue_Works()
        {
            var dict = new SerializableDictionary<CharacterProfile, CombatStats>();

            var hero = new CharacterProfile("Arthur", "Paladin");
            var stats = new CombatStats(1000, 80, 60, 0.15f);

            dict.Add(hero, stats);

            // Lookup using a distinct new instance with identical values (value-equality test)
            var lookupKey = new CharacterProfile("Arthur", "Paladin");
            Assert.IsTrue(dict.ContainsKey(lookupKey));
            Assert.AreEqual(1000, dict[lookupKey].health);
            Assert.AreEqual(80, dict[lookupKey].attackPower);

            // Update struct value
            var updatedStats = dict[lookupKey];
            updatedStats.attackPower = 95;
            dict[lookupKey] = updatedStats;

            Assert.AreEqual(95, dict[lookupKey].attackPower);

            // Remove
            Assert.IsTrue(dict.Remove(lookupKey));
            Assert.IsFalse(dict.ContainsKey(lookupKey));
        }

        // ------------------------------------------------------------------ //
        //  2. ObservableDictionary: Class Key -> Struct Value with Events
        // ------------------------------------------------------------------ //

        [Test]
        public void ObservableDictionary_ClassKey_StructValue_FiresEvents()
        {
            var dict = new ObservableDictionary<CharacterProfile, CombatStats>();
            CharacterProfile addedHero = null;
            CombatStats addedStats = default;
            bool updatedFired = false;

            dict.OnEntryAdded += (h, s) =>
            {
                addedHero = h;
                addedStats = s;
            };

            dict.OnEntryUpdated += (h, oldS, newS) =>
            {
                updatedFired = true;
                Assert.AreEqual(80, oldS.attackPower);
                Assert.AreEqual(110, newS.attackPower);
            };

            var hero = new CharacterProfile("Merlin", "Mage");
            var stats = new CombatStats(500, 80, 20, 0.30f);

            dict.Add(hero, stats);

            Assert.IsNotNull(addedHero);
            Assert.AreEqual("Merlin", addedHero.HeroName);
            Assert.AreEqual(80, addedStats.attackPower);

            // Update
            var lookupKey = new CharacterProfile("Merlin", "Mage");
            dict[lookupKey] = new CombatStats(500, 110, 20, 0.30f);
            Assert.IsTrue(updatedFired);
        }

        // ------------------------------------------------------------------ //
        //  3. Sets with Classes & Structs
        // ------------------------------------------------------------------ //

        [Test]
        public void SerializableHashSet_ClassElements_RejectsDuplicates()
        {
            var set = new SerializableHashSet<CharacterProfile>();

            Assert.IsTrue(set.Add(new CharacterProfile("Robin", "Ranger")));
            // Distinct instance with same data should be rejected by IEquatable
            Assert.IsFalse(set.Add(new CharacterProfile("Robin", "Ranger")));
            Assert.AreEqual(1, set.Count);

            Assert.IsTrue(set.Contains(new CharacterProfile("Robin", "Ranger")));
        }

        [Test]
        public void ObservableHashSet_StructElements_FiresEvents()
        {
            var set = new ObservableHashSet<CombatStats>();
            bool addedFired = false;

            set.OnItemAdded += s => addedFired = true;

            var stat = new CombatStats(100, 10, 5, 0.05f);
            Assert.IsTrue(set.Add(stat));
            Assert.IsTrue(addedFired);

            // Duplicate struct rejected
            Assert.IsFalse(set.Add(new CombatStats(100, 10, 5, 0.05f)));
            Assert.AreEqual(1, set.Count);
        }

        // ------------------------------------------------------------------ //
        //  4. OrderedDictionary with Class Key & Class Value
        // ------------------------------------------------------------------ //

        [Test]
        public void OrderedDictionary_ClassKey_PreservesInsertionOrder()
        {
            var ordered = new SerializableOrderedDictionary<CharacterProfile, SkillData>();

            var h1 = new CharacterProfile("A", "Warrior");
            var h2 = new CharacterProfile("B", "Ranger");
            var h3 = new CharacterProfile("C", "Mage");

            ordered.Add(h1, new SkillData("Slash", 5, 1f));
            ordered.Add(h2, new SkillData("Shot", 10, 2f));
            ordered.Add(h3, new SkillData("Cast", 15, 3f));

            Assert.AreEqual("A", ordered.GetAt(0).Key.HeroName);
            Assert.AreEqual("B", ordered.GetAt(1).Key.HeroName);
            Assert.AreEqual("C", ordered.GetAt(2).Key.HeroName);
        }

        // ------------------------------------------------------------------ //
        //  5. BiDictionary with Struct Key
        // ------------------------------------------------------------------ //

        [Test]
        public void BiDictionary_StructKey_BidirectionalLookup()
        {
            var biDict = new SerializableBiDictionary<PlayerBadge, string>();

            var badge = new PlayerBadge(1, "Champion");
            biDict.Add(badge, "Player1");

            Assert.AreEqual("Player1", biDict.GetByKey(badge));
            Assert.AreEqual(badge, biDict.GetByValue("Player1"));
        }

        // ------------------------------------------------------------------ //
        //  6. Stack & Queue with Structs and Classes
        // ------------------------------------------------------------------ //

        [Test]
        public void Stack_StructElements_LIFO()
        {
            var stack = new SerializableStack<CombatStats>();
            stack.Push(new CombatStats(100, 10, 5, 0f));
            stack.Push(new CombatStats(200, 20, 10, 0f));

            Assert.AreEqual(200, stack.Pop().health);
            Assert.AreEqual(100, stack.Pop().health);
        }

        [Test]
        public void Queue_ClassElements_FIFO()
        {
            var queue = new SerializableQueue<CharacterProfile>();
            queue.Enqueue(new CharacterProfile("Hero1", "Warrior"));
            queue.Enqueue(new CharacterProfile("Hero2", "Mage"));

            Assert.AreEqual("Hero1", queue.Dequeue().HeroName);
            Assert.AreEqual("Hero2", queue.Dequeue().HeroName);
        }
    }
}
