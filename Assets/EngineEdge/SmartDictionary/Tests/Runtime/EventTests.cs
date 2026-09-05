// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  EventTests.cs
//  NUnit tests for ObservableDictionary and ObservableHashSet event systems.
// ============================================================

using NUnit.Framework;

namespace EngineEdge.SmartDictionary.Tests
{
    /// <summary>
    /// Tests that verify the event system on
    /// <see cref="ObservableDictionary{TKey,TValue}"/> and <see cref="ObservableHashSet{T}"/>.
    /// </summary>
    [TestFixture]
    public class ObservableCollectionEventTests
    {
        // ------------------------------------------------------------------ //
        //  Helpers
        // ------------------------------------------------------------------ //

        /// <summary>Creates a fresh observable dictionary with events enabled.</summary>
        private ObservableDictionary<string, int> NewDict()
            => new ObservableDictionary<string, int>();

        // ------------------------------------------------------------------ //
        //  OnEntryAdded
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>OnEntryAdded</c> fires once when a new key is added.
        /// </summary>
        [Test]
        public void OnEntryAdded_FiredOnAdd()
        {
            var dict     = NewDict();
            var firedKey = string.Empty;
            var firedVal = 0;

            dict.OnEntryAdded += (k, v) =>
            {
                firedKey = k;
                firedVal = v;
            };

            dict.Add("gold", 500);

            Assert.AreEqual("gold", firedKey);
            Assert.AreEqual(500,    firedVal);
        }

        /// <summary>
        /// <c>OnEntryAdded</c> is not invoked when
        /// <c>EventsEnabled</c> is <see langword="false"/>.
        /// </summary>
        [Test]
        public void OnEntryAdded_NotFiredWhenEventsDisabled()
        {
            var dict  = NewDict();
            var fired = false;

            dict.OnEntryAdded  += (k, v) => fired = true;
            dict.EventsEnabled  = false;

            dict.Add("key", 1);

            Assert.IsFalse(fired);
        }

        // ------------------------------------------------------------------ //
        //  OnEntryRemoved
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>OnEntryRemoved</c> fires when an existing entry is removed.
        /// </summary>
        [Test]
        public void OnEntryRemoved_FiredOnRemove()
        {
            var dict     = NewDict();
            var firedKey = string.Empty;

            dict.OnEntryRemoved += (k, v) => firedKey = k;
            dict.Add("silver", 200);
            dict.Remove("silver");

            Assert.AreEqual("silver", firedKey);
        }

        // ------------------------------------------------------------------ //
        //  OnEntryUpdated
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>OnEntryUpdated</c> fires with the correct old and new values.
        /// </summary>
        [Test]
        public void OnEntryUpdated_FiredOnUpdate()
        {
            var dict    = NewDict();
            var oldVal  = -1;
            var newVal  = -1;

            dict.OnEntryUpdated += (k, ov, nv) =>
            {
                oldVal = ov;
                newVal = nv;
            };

            dict.Add("xp", 100);
            dict.AddOrUpdate("xp", 250);

            Assert.AreEqual(100, oldVal);
            Assert.AreEqual(250, newVal);
        }

        // ------------------------------------------------------------------ //
        //  OnCleared
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>OnCleared</c> fires when <c>Clear()</c> is called.
        /// </summary>
        [Test]
        public void OnCleared_FiredOnClear()
        {
            var dict   = NewDict();
            var fired  = false;

            dict.OnCleared += () => fired = true;
            dict.Add("a", 1);
            dict.Clear();

            Assert.IsTrue(fired);
        }

        // ------------------------------------------------------------------ //
        //  OnCountChanged
        // ------------------------------------------------------------------ //

        /// <summary>
        /// <c>OnCountChanged</c> fires with the correct new count after adding.
        /// </summary>
        [Test]
        public void OnCountChanged_FiredWithCorrectCount()
        {
            var dict         = NewDict();
            var reportedCount = -1;

            dict.OnCountChanged += c => reportedCount = c;
            dict.Add("a", 1);

            Assert.AreEqual(1, reportedCount);

            dict.Add("b", 2);
            Assert.AreEqual(2, reportedCount);

            dict.Remove("a");
            Assert.AreEqual(1, reportedCount);
        }

        /// <summary>
        /// EventsEnabled defaults to true and can be toggled.
        /// </summary>
        [Test]
        public void EventsEnabled_DefaultIsTrue_AndCanBeToggled()
        {
            var dict = NewDict();
            Assert.IsTrue(dict.EventsEnabled);

            dict.EventsEnabled = false;
            Assert.IsFalse(dict.EventsEnabled);

            dict.EventsEnabled = true;
            Assert.IsTrue(dict.EventsEnabled);
        }

        // ------------------------------------------------------------------ //
        //  Serialized UnityEvent Tests
        // ------------------------------------------------------------------ //

        [Test]
        public void UnityEvents_FiredWhenEnabled()
        {
            var dict = NewDict();
            bool addedFired = false;
            bool removedFired = false;
            bool updatedFired = false;
            bool clearedFired = false;
            int countReported = -1;

            dict.OnEntryAddedEvent.AddListener(() => addedFired = true);
            dict.OnEntryRemovedEvent.AddListener(() => removedFired = true);
            dict.OnEntryUpdatedEvent.AddListener(() => updatedFired = true);
            dict.OnClearedEvent.AddListener(() => clearedFired = true);
            dict.OnCountChangedEvent.AddListener(c => countReported = c);

            dict.Add("k1", 10);
            Assert.IsTrue(addedFired);
            Assert.AreEqual(1, countReported);

            dict["k1"] = 20;
            Assert.IsTrue(updatedFired);

            dict.Remove("k1");
            Assert.IsTrue(removedFired);
            Assert.AreEqual(0, countReported);

            dict.Add("k2", 30);
            dict.Clear();
            Assert.IsTrue(clearedFired);
            Assert.AreEqual(0, countReported);
        }

        [Test]
        public void UnityEvents_NotFiredWhenDisabled()
        {
            var dict = NewDict();
            dict.EventsEnabled = false;

            bool anyFired = false;
            dict.OnEntryAddedEvent.AddListener(() => anyFired = true);
            dict.OnEntryRemovedEvent.AddListener(() => anyFired = true);
            dict.OnEntryUpdatedEvent.AddListener(() => anyFired = true);
            dict.OnClearedEvent.AddListener(() => anyFired = true);
            dict.OnCountChangedEvent.AddListener(c => anyFired = true);

            dict.Add("k1", 10);
            dict["k1"] = 20;
            dict.Remove("k1");
            dict.Add("k2", 30);
            dict.Clear();

            Assert.IsFalse(anyFired, "UnityEvents should not fire when EventsEnabled is false.");
        }

        [Test]
        public void HashSet_UnityEvents_FiredWhenEnabled_AndMutedWhenDisabled()
        {
            var set = new ObservableHashSet<string>();
            bool added = false;
            bool removed = false;
            bool cleared = false;
            int count = -1;

            set.OnItemAddedEvent.AddListener(() => added = true);
            set.OnItemRemovedEvent.AddListener(() => removed = true);
            set.OnClearedEvent.AddListener(() => cleared = true);
            set.OnCountChangedEvent.AddListener(c => count = c);

            set.Add("item1");
            Assert.IsTrue(added);
            Assert.AreEqual(1, count);

            set.Remove("item1");
            Assert.IsTrue(removed);
            Assert.AreEqual(0, count);

            set.Add("item2");
            set.Clear();
            Assert.IsTrue(cleared);
            Assert.AreEqual(0, count);

            // Test muted
            set.EventsEnabled = false;
            bool mutedFired = false;
            set.OnItemAddedEvent.AddListener(() => mutedFired = true);
            set.Add("item3");
            Assert.IsFalse(mutedFired, "HashSet UnityEvents should not fire when EventsEnabled is false.");
        }
    }
}
