// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  AdvancedCollectionsDemo.cs  (Sample: AdvancedCollections)
// ============================================================

using UnityEngine;

namespace EngineEdge.SmartDictionary.Samples
{
    /// <summary>
    /// Demonstrates the five bonus serializable collection types included in
    /// Smart Dictionary Pro.
    /// </summary>
    /// <remarks>
    /// Attach this component to a GameObject in the <c>AdvancedCollections</c>
    /// sample scene, populate the fields in the Inspector, then press Play.
    /// </remarks>
    [AddComponentMenu("EngineEdge/SmartDictionary/Advanced Collections Demo")]
    public class AdvancedCollectionsDemo : MonoBehaviour
    {
        // ------------------------------------------------------------------ //
        //  Serialized fields
        // ------------------------------------------------------------------ //

        /// <summary>
        /// A set of unique tag strings.  Duplicate entries are rejected.
        /// </summary>
        [Header("HashSet")]
        [Tooltip("Unique string tags — duplicates are ignored")]
        [SerializeField]
        private SerializableHashSet<string> uniqueTags
            = new SerializableHashSet<string>();

        /// <summary>
        /// A last-in, first-out history of action names.
        /// </summary>
        [Header("Stack")]
        [Tooltip("Action history (LIFO)")]
        [SerializeField]
        private SerializableStack<string> actionHistory
            = new SerializableStack<string>();

        /// <summary>
        /// A first-in, first-out queue of pending message strings.
        /// </summary>
        [Header("Queue")]
        [Tooltip("Pending messages (FIFO)")]
        [SerializeField]
        private SerializableQueue<string> messageQueue
            = new SerializableQueue<string>();

        /// <summary>
        /// A dictionary that preserves insertion order.
        /// </summary>
        [Header("Ordered Dictionary")]
        [Tooltip("Player name → score (insertion order preserved)")]
        [SerializeField]
        private SerializableOrderedDictionary<string, int> orderedScores
            = new SerializableOrderedDictionary<string, int>();

        /// <summary>
        /// A bidirectional dictionary allowing lookup by key or value.
        /// </summary>
        [Header("BiDictionary")]
        [Tooltip("Player name ↔ unique integer player ID")]
        [SerializeField]
        private SerializableBiDictionary<string, int> playerIdMap
            = new SerializableBiDictionary<string, int>();

        // ------------------------------------------------------------------ //
        //  Unity lifecycle
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Runs demonstrations for each collection type and logs the results.
        /// </summary>
        private void Start()
        {
            DemoHashSet();
            DemoStack();
            DemoQueue();
            DemoOrderedDictionary();
            DemoBiDictionary();
        }

        // ------------------------------------------------------------------ //
        //  Demos
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Demonstrates <see cref="SerializableHashSet{T}"/>: adding items,
        /// duplicate rejection, and contains check.
        /// </summary>
        private void DemoHashSet()
        {
            Debug.Log("=== SerializableHashSet ===");

            uniqueTags.Add("enemy");
            uniqueTags.Add("interactable");
            var duplicateAdded = uniqueTags.Add("enemy"); // should be false

            Debug.Log($"Tags count: {uniqueTags.Count}");
            Debug.Log($"Duplicate 'enemy' added: {duplicateAdded}");
            Debug.Log($"Contains 'interactable': {uniqueTags.Contains("interactable")}");
        }

        /// <summary>
        /// Demonstrates <see cref="SerializableStack{T}"/>: push, peek, and pop.
        /// </summary>
        private void DemoStack()
        {
            Debug.Log("=== SerializableStack ===");

            actionHistory.Push("Move");
            actionHistory.Push("Jump");
            actionHistory.Push("Attack");

            Debug.Log($"Stack count  : {actionHistory.Count}");
            Debug.Log($"Peek (top)   : {actionHistory.Peek()}");
            Debug.Log($"Pop          : {actionHistory.Pop()}");
            Debug.Log($"After pop    : {actionHistory.Count}");
        }

        /// <summary>
        /// Demonstrates <see cref="SerializableQueue{T}"/>: enqueue, peek, dequeue.
        /// </summary>
        private void DemoQueue()
        {
            Debug.Log("=== SerializableQueue ===");

            messageQueue.Enqueue("Hello, world!");
            messageQueue.Enqueue("Second message");
            messageQueue.Enqueue("Third message");

            Debug.Log($"Queue count  : {messageQueue.Count}");
            Debug.Log($"Peek (front) : {messageQueue.Peek()}");
            Debug.Log($"Dequeue      : {messageQueue.Dequeue()}");
            Debug.Log($"After dequeue: {messageQueue.Count}");
        }

        /// <summary>
        /// Demonstrates <see cref="SerializableOrderedDictionary{TKey,TValue}"/>:
        /// insertion-order preserved iteration and index-based access.
        /// </summary>
        private void DemoOrderedDictionary()
        {
            Debug.Log("=== SerializableOrderedDictionary ===");

            orderedScores.Add("charlie", 300);
            orderedScores.Add("alice",   100);
            orderedScores.Add("bob",     200);

            Debug.Log("Insertion order:");
            foreach (var (name, score) in orderedScores)
                Debug.Log($"  {name}: {score}");

            Debug.Log($"Entry at index 0: {orderedScores.GetAt(0)}");
        }

        /// <summary>
        /// Demonstrates <see cref="SerializableBiDictionary{TKey,TValue}"/>:
        /// forward (key→value) and inverse (value→key) lookups.
        /// </summary>
        private void DemoBiDictionary()
        {
            Debug.Log("=== SerializableBiDictionary ===");

            playerIdMap.Add("alice", 1001);
            playerIdMap.Add("bob",   1002);

            Debug.Log($"alice → ID   : {playerIdMap["alice"]}");
            Debug.Log($"ID 1002 → name: {playerIdMap.GetByValue(1002)}");
        }
    }
}
