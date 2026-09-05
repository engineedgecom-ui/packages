using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EngineEdge.SmartDictionary;

namespace EngineEdge.SmartDictionary.Example
{
    /// <summary>
    /// Demonstrates <see cref="SerializableOrderedDictionary{TKey, TValue}"/> for preserving
    /// insertion sequence, index-based access, and reordering helpers (MoveToFront / MoveToBack).
    /// </summary>
    [AddComponentMenu("EngineEdge/SmartDictionary/Examples/Ordered Dictionary Example")]
    public class OrderedDictionaryExample : MonoBehaviour
    {
        [Header("1. Leaderboard (Ordered string -> int)")]
        [Tooltip("Leaderboard entries with guaranteed insertion sequence.")]
        public SerializableOrderedDictionary<string, int> leaderboard = new SerializableOrderedDictionary<string, int>
        {
            { "Player_Alpha", 9500 },
            { "Player_Bravo", 8200 },
            { "Player_Charlie", 7100 },
            { "Player_Delta", 6400 }
        };

        [Header("2. Active Hero Skills (Ordered Class -> Class)")]
        [Tooltip("Key is CharacterProfile class, Value is SkillData class in active hotbar sequence.")]
        public SerializableOrderedDictionary<CharacterProfile, SkillData> activeSkills = new SerializableOrderedDictionary<CharacterProfile, SkillData>
        {
            { new CharacterProfile("Arthur", "Paladin"), new SkillData("Holy Shield", 25, 12f) },
            { new CharacterProfile("Merlin", "Mage"),    new SkillData("Meteor Strike", 60, 20f) },
            { new CharacterProfile("Robin", "Ranger"),   new SkillData("Rain of Arrows", 35, 8f) }
        };

        [Header("Runtime GUI Overlay")]
        [SerializeField] private bool showOnScreenGui = true;

        private readonly List<string> _actionLogs = new List<string>();
        private Vector2 _scrollPos;
        private string _newPlayerName = "Player_Echo";
        private int _newPlayerScore = 5500;

        private void Awake()
        {
            Debug.Log(activeSkills.ToStandardJson(prettyPrint: true));
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
            GUILayout.Label("<b><size=14>📜 Ordered Dictionary Example</size></b>");
            GUILayout.Label("<color=#CCCCCC><size=10>Guaranteed Insertion Order & Reordering (MoveToFront / MoveToBack)</size></color>");
            GUILayout.Space(6);

            GUILayout.Label($"<b>1. Leaderboard Entries ({leaderboard.Count}):</b>");
            GUILayout.BeginHorizontal();
            _newPlayerName = GUILayout.TextField(_newPlayerName, GUILayout.Width(140));
            int.TryParse(GUILayout.TextField(_newPlayerScore.ToString(), GUILayout.Width(60)), out _newPlayerScore);
            if (GUILayout.Button("Add Entry"))
            {
                if (leaderboard.TryAdd(_newPlayerName, _newPlayerScore))
                    Log($"Added '{_newPlayerName}' ({_newPlayerScore} pts) at end of list.");
                else
                    Log($"Player '{_newPlayerName}' already exists!");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(180));
            string toRemove = null;
            string toFront = null;
            string toBack = null;

            int index = 0;
            foreach (var pair in leaderboard.ToList())
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"<b>#{index + 1} {pair.Key}</b>: {pair.Value} pts", GUILayout.Width(200));

                if (GUILayout.Button("▲ Top", GUILayout.Width(50)))
                {
                    toFront = pair.Key;
                }
                if (GUILayout.Button("▼ Bottom", GUILayout.Width(65)))
                {
                    toBack = pair.Key;
                }
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    toRemove = pair.Key;
                }
                GUILayout.EndHorizontal();
                index++;
            }

            if (toFront != null)
            {
                leaderboard.MoveToFront(toFront);
                Log($"Moved '{toFront}' to front (Index 0).");
            }
            if (toBack != null)
            {
                leaderboard.MoveToBack(toBack);
                Log($"Moved '{toBack}' to back.");
            }
            if (toRemove != null)
            {
                leaderboard.Remove(toRemove);
                Log($"Removed '{toRemove}'.");
            }

            GUILayout.EndScrollView();

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
        private class OrderedJsonContainer
        {
            public SerializableOrderedDictionary<string, int> leaderboard;
            public SerializableOrderedDictionary<CharacterProfile, SkillData> activeSkills;
        }

        /// <summary>
        /// Serializes ordered dictionaries to JSON using JsonUtility and outputs formatted results to Debug.Log.
        /// </summary>
        [ContextMenu("Log JSON to Console (JsonUtility)")]
        public void LogJsonToConsole()
        {
            var container = new OrderedJsonContainer
            {
                leaderboard = this.leaderboard,
                activeSkills = this.activeSkills
            };

            string json = JsonUtility.ToJson(container, prettyPrint: true);
            Debug.Log(json);
        }
    }
}
