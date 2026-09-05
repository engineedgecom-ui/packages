using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EngineEdge.SmartDictionary;

namespace EngineEdge.SmartDictionary.Example
{
    /// <summary>
    /// Demonstrates <see cref="SerializableStack{T}"/> (LIFO) and <see cref="SerializableQueue{T}"/> (FIFO)
    /// across primitive, struct, and class types with C# Action events.
    /// </summary>
    [AddComponentMenu("EngineEdge/SmartDictionary/Examples/Stack & Queue Example")]
    public class StackAndQueueExample : MonoBehaviour
    {
        [Header("1. Serializable Stack (LIFO: Action History)")]
        [Tooltip("Last-In-First-Out undo stack of player actions.")]
        public SerializableStack<string> actionStack = new SerializableStack<string>();

        [Header("2. Serializable Stack (LIFO: Combat Stat History)")]
        [Tooltip("Snapshots of player stats over time.")]
        public SerializableStack<CombatStats> statSnapshots = new SerializableStack<CombatStats>();

        [Header("3. Serializable Queue (FIFO: Matchmaking Lobby)")]
        [Tooltip("First-In-First-Out matchmaking queue of CharacterProfile class objects.")]
        public SerializableQueue<CharacterProfile> matchmakingQueue = new SerializableQueue<CharacterProfile>();

        [Header("4. Serializable Queue (FIFO: Message Logs)")]
        [Tooltip("FIFO message broadcast queue.")]
        public SerializableQueue<string> messageQueue = new SerializableQueue<string>();

        [Header("Runtime GUI Overlay")]
        [SerializeField] private bool showOnScreenGui = true;

        private readonly List<string> _actionLogs = new List<string>();
        private string _newAction = "Cast Spell";
        private string _newQueueHeroName = "Kay";
        private string _newQueueHeroClass = "Assassin";

        private void OnEnable()
        {
            actionStack.OnPushed += item => Log($"[Stack Pushed] Action: '{item}'");
            actionStack.OnPopped += item => Log($"[Stack Popped] Action: '{item}'");
            actionStack.OnCleared += () => Log("[Stack Cleared]");

            matchmakingQueue.OnEnqueued += hero => Log($"[Queue Enqueued] Hero: {hero}");
            matchmakingQueue.OnDequeued += hero => Log($"[Queue Dequeued] Hero: {hero}");
            matchmakingQueue.OnCleared += () => Log("[Queue Cleared]");
        }

        private void Awake()
        {
            if (actionStack.Count == 0)
            {
                actionStack.Push("Move North");
                actionStack.Push("Open Chest");
                actionStack.Push("Equip Sword");
            }

            if (matchmakingQueue.Count == 0)
            {
                matchmakingQueue.Enqueue(new CharacterProfile("Arthur", "Paladin"));
                matchmakingQueue.Enqueue(new CharacterProfile("Merlin", "Mage"));
            }

            Debug.Log(JsonUtility.ToJson(new StackQueueJsonContainer
            {
                actionStack = this.actionStack,
                statSnapshots = this.statSnapshots,
                matchmakingQueue = this.matchmakingQueue
            }, true));
        }

        private void Log(string message)
        {
            string entry = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            _actionLogs.Add(entry);
            if (_actionLogs.Count > 30) _actionLogs.RemoveAt(0);
        }

        private void OnGUI()
        {
            if (!showOnScreenGui) return;

            GUILayout.BeginArea(new Rect(15, 15, 420, Screen.height - 30), GUI.skin.box);
            GUILayout.Label("<b><size=14>📦 Stack & Queue Example</size></b>");
            GUILayout.Label("<color=#CCCCCC><size=10>LIFO Stack (Undo/History) & FIFO Queue (Lobby/Messages)</size></color>");
            GUILayout.Space(6);

            // ── Stack Section ───────────────────────────────────────────────
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"<b>1. Action Stack ({actionStack.Count} items - Top: {(actionStack.Count > 0 ? actionStack.Peek() : "None")}):</b>");
            GUILayout.BeginHorizontal();
            _newAction = GUILayout.TextField(_newAction, GUILayout.Width(160));
            if (GUILayout.Button("Push"))
            {
                actionStack.Push(_newAction);
            }
            if (GUILayout.Button("Pop") && actionStack.Count > 0)
            {
                actionStack.Pop();
            }
            if (GUILayout.Button("Clear"))
            {
                actionStack.Clear();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // ── Queue Section ───────────────────────────────────────────────
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"<b>2. Matchmaking Queue ({matchmakingQueue.Count} in line - Front: {(matchmakingQueue.Count > 0 ? matchmakingQueue.Peek().ToString() : "None")}):</b>");
            GUILayout.BeginHorizontal();
            _newQueueHeroName = GUILayout.TextField(_newQueueHeroName, GUILayout.Width(100));
            _newQueueHeroClass = GUILayout.TextField(_newQueueHeroClass, GUILayout.Width(80));
            if (GUILayout.Button("Enqueue"))
            {
                matchmakingQueue.Enqueue(new CharacterProfile(_newQueueHeroName, _newQueueHeroClass));
            }
            if (GUILayout.Button("Dequeue") && matchmakingQueue.Count > 0)
            {
                matchmakingQueue.Dequeue();
            }
            if (GUILayout.Button("Clear"))
            {
                matchmakingQueue.Clear();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6);
            if (GUILayout.Button("💾 Log JSON to Console (JsonUtility)", GUILayout.Height(28)))
            {
                LogJsonToConsole();
            }

            GUILayout.Space(6);
            GUILayout.Label("<b>Live Activity Log:</b>");
            for (int i = _actionLogs.Count - 1; i >= Mathf.Max(0, _actionLogs.Count - 6); i--)
            {
                GUILayout.Label($"<size=10>{_actionLogs[i]}</size>");
            }
            GUILayout.EndArea();
        }

        [System.Serializable]
        private class StackQueueJsonContainer
        {
            public SerializableStack<string> actionStack;
            public SerializableStack<CombatStats> statSnapshots;
            public SerializableQueue<CharacterProfile> matchmakingQueue;
        }

        /// <summary>
        /// Serializes stack and queue to JSON using JsonUtility and outputs formatted results to Debug.Log.
        /// </summary>
        [ContextMenu("Log JSON to Console (JsonUtility)")]
        public void LogJsonToConsole()
        {
            var container = new StackQueueJsonContainer
            {
                actionStack = this.actionStack,
                statSnapshots = this.statSnapshots,
                matchmakingQueue = this.matchmakingQueue
            };

            string json = JsonUtility.ToJson(container, prettyPrint: true);
            Debug.Log(json);
        }
    }
}
