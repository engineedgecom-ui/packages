// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  DemoController.cs  (Sample: BasicUsage)
// ============================================================

using UnityEngine;

namespace EngineEdge.SmartDictionary.Samples
{
    /// <summary>
    /// Demonstrates basic usage of <see cref="SerializableDictionary{TKey,TValue}"/>
    /// in a <see cref="MonoBehaviour"/> context.
    /// </summary>
    /// <remarks>
    /// Attach this component to any GameObject in a sample scene.
    /// Edit the dictionaries directly in the Inspector, then press Play.
    /// </remarks>
    [AddComponentMenu("EngineEdge/SmartDictionary/Demo Controller")]
    public class DemoController : MonoBehaviour
    {
        // ------------------------------------------------------------------ //
        //  Serialized fields
        // ------------------------------------------------------------------ //

        /// <summary>Maps item names to their stack counts (observable with events).</summary>
        [Header("Inventory (Observable)")]
        [Tooltip("Item name → quantity")]
        public ObservableDictionary<string, int> itemInventory
            = new ObservableDictionary<string, int>();

        /// <summary>Maps team names to their representative colour.</summary>
        [Header("Team Colours")]
        [Tooltip("Team name → colour")]
        public SerializableDictionary<string, Color> teamColors
            = new SerializableDictionary<string, Color>();

        // ------------------------------------------------------------------ //
        //  Unity lifecycle
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Subscribes to the <c>OnEntryAdded</c> event so newly added items
        /// are logged automatically.
        /// </summary>
        private void OnEnable()
        {
            itemInventory.OnEntryAdded += HandleItemAdded;
        }

        /// <summary>
        /// Unsubscribes from the <c>OnEntryAdded</c> event to prevent
        /// memory leaks and stale delegate references.
        /// </summary>
        private void OnDisable()
        {
            itemInventory.OnEntryAdded -= HandleItemAdded;
        }

        /// <summary>
        /// Runs a brief demonstration of the most common API methods.
        /// </summary>
        private void Start()
        {
            // ── Log all inventory entries ─────────────────────────────────
            Debug.Log("=== Inventory ===");
            foreach (var (item, qty) in itemInventory)
                Debug.Log($"  {item}: {qty}");

            // ── GetOrDefault ──────────────────────────────────────────────
            var arrows = itemInventory.GetOrDefault("arrow", 0);
            Debug.Log($"Arrows in inventory: {arrows}");

            // ── TryAdd ────────────────────────────────────────────────────
            var added = itemInventory.TryAdd("potion", 5);
            Debug.Log(added
                ? "Added 5 potions."
                : "Potion already exists in inventory.");

            // ── AddOrUpdate ───────────────────────────────────────────────
            itemInventory.AddOrUpdate("sword", 1);
            Debug.Log($"Sword count: {itemInventory.GetOrDefault("sword", 0)}");

            // ── Log all team colours ──────────────────────────────────────
            Debug.Log("=== Team Colours ===");
            foreach (var (team, col) in teamColors)
                Debug.Log($"  {team}: {col}");
        }

        // ------------------------------------------------------------------ //
        //  Event handlers
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Called whenever a new entry is added to <see cref="itemInventory"/>.
        /// </summary>
        /// <param name="key">The item name that was added.</param>
        /// <param name="value">The quantity that was added.</param>
        private void HandleItemAdded(string key, int value)
        {
            Debug.Log($"[Event] Item added → {key} x{value}");
        }
    }
}
