// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  LinqEventsDemo.cs  (Sample: LinqAndEvents)
// ============================================================

using UnityEngine;

namespace EngineEdge.SmartDictionary.Samples
{
    /// <summary>
    /// Demonstrates LINQ-style extension methods and the event system on
    /// <see cref="SerializableDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <remarks>
    /// Attach this component to a GameObject in the <c>LinqAndEvents</c> sample
    /// scene, populate <see cref="scores"/> in the Inspector, then press Play.
    /// </remarks>
    [AddComponentMenu("EngineEdge/SmartDictionary/Linq Events Demo")]
    public class LinqEventsDemo : MonoBehaviour
    {
        // ------------------------------------------------------------------ //
        //  Serialized fields
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Player name → score mapping (observable with events). Populate this in the Inspector
        /// before entering Play mode.
        /// </summary>
        [Header("Scores (Observable)")]
        [Tooltip("Player name → integer score")]
        [SerializeField]
        private ObservableDictionary<string, int> scores
            = new ObservableDictionary<string, int>();

        // ------------------------------------------------------------------ //
        //  Unity lifecycle
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Subscribes to the <c>OnEntryUpdated</c> event.
        /// </summary>
        private void OnEnable()
        {
            scores.OnEntryUpdated += HandleScoreUpdated;
        }

        /// <summary>
        /// Unsubscribes from the <c>OnEntryUpdated</c> event.
        /// </summary>
        private void OnDisable()
        {
            scores.OnEntryUpdated -= HandleScoreUpdated;
        }

        /// <summary>
        /// Runs a series of LINQ demonstrations against the <see cref="scores"/>
        /// dictionary and logs results to the Console.
        /// </summary>
        private void Start()
        {
            // ── Deconstruct foreach ───────────────────────────────────────
            Debug.Log("=== All Scores ===");
            foreach (var (name, score) in scores)
                Debug.Log($"  {name}: {score}");

            // ── WhereDict — high scorers ─────────────────────────────────
            var highScores = scores.WhereDict((k, v) => v > 50);
            Debug.Log($"High scorers (>50): {highScores.Count}");
            foreach (var (name, score) in highScores)
                Debug.Log($"  {name}: {score}");

            // ── SelectValues — double all scores ─────────────────────────
            var doubled = scores.SelectValues((k, v) => v * 2);
            Debug.Log("=== Doubled Scores ===");
            foreach (var (name, score) in doubled)
                Debug.Log($"  {name}: {score}");

            // ── OrderByKey — alphabetical ─────────────────────────────────
            var byName = scores.OrderByKey();
            Debug.Log("=== Alphabetical Order ===");
            foreach (var (name, score) in byName)
                Debug.Log($"  {name}: {score}");

            // ── Aggregates ────────────────────────────────────────────────
            var total   = scores.SumValues();
            var highest = scores.MaxValue();
            Debug.Log($"Total score : {total}");
            Debug.Log($"Highest score : {highest}");

            // ── Trigger an update to demonstrate OnEntryUpdated ───────────
            if (scores.Count > 0)
            {
                // Find the first key and update its value
                string firstKey = default;
                foreach (var (k, _) in scores) { firstKey = k; break; }
                if (firstKey != null)
                    scores.AddOrUpdate(firstKey, scores[firstKey] + 10);
            }
        }

        // ------------------------------------------------------------------ //
        //  Event handlers
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Called whenever an existing score is updated.
        /// </summary>
        /// <param name="key">The player whose score changed.</param>
        /// <param name="oldValue">The previous score.</param>
        /// <param name="newValue">The updated score.</param>
        private void HandleScoreUpdated(string key, int oldValue, int newValue)
        {
            Debug.Log($"[Event] Score updated — {key}: {oldValue} → {newValue}");
        }
    }
}
