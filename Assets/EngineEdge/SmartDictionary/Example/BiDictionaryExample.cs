using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EngineEdge.SmartDictionary;

namespace EngineEdge.SmartDictionary.Example
{
    /// <summary>
    /// Demonstrates <see cref="SerializableBiDictionary{TKey, TValue}"/> for two-way,
    /// bijective O(1) lookups in both directions (Key -> Value and Value -> Key).
    /// </summary>
    [AddComponentMenu("EngineEdge/SmartDictionary/Examples/BiDictionary Example")]
    public class BiDictionaryExample : MonoBehaviour
    {
        [Header("1. Player Name <-> Unique ID (string <-> int)")]
        [Tooltip("Maps player usernames to unique server IDs and vice-versa.")]
        public SerializableBiDictionary<string, int> playerIds = new SerializableBiDictionary<string, int>
        {
            { "Player_Alpha", 1001 },
            { "Player_Bravo", 1002 },
            { "Player_Charlie", 1003 }
        };

        [Header("2. Struct Key <-> String Value")]
        [Tooltip("Maps player achievement badges (PlayerBadge struct) to player names.")]
        public SerializableBiDictionary<PlayerBadge, string> badgeOwners = new SerializableBiDictionary<PlayerBadge, string>
        {
            { new PlayerBadge(101, "Grandmaster"), "Player_Alpha" },
            { new PlayerBadge(102, "Sharpshooter"), "Player_Bravo" },
            { new PlayerBadge(103, "Iron Wall"),    "Player_Charlie" }
        };

        [Header("Runtime GUI Overlay")]
        [SerializeField] private bool showOnScreenGui = true;

        private readonly List<string> _actionLogs = new List<string>();
        private Vector2 _scrollPos;
        private string _newName = "Player_Delta";
        private int _newId = 1004;
        private string _searchName = "Player_Alpha";
        private int _searchId = 1002;

        private void Awake()
        {
            Debug.Log(JsonUtility.ToJson(new BiDictJsonContainer
            {
                playerIds = this.playerIds,
                badgeOwners = this.badgeOwners
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
            GUILayout.Label("<b><size=14>🔄 BiDictionary Example</size></b>");
            GUILayout.Label("<color=#CCCCCC><size=10>Two-Way O(1) Lookup: Key ↔ Value</size></color>");
            GUILayout.Space(6);

            GUILayout.Label($"<b>1. Add New Pair ({playerIds.Count} entries):</b>");
            GUILayout.BeginHorizontal();
            _newName = GUILayout.TextField(_newName, GUILayout.Width(140));
            int.TryParse(GUILayout.TextField(_newId.ToString(), GUILayout.Width(70)), out _newId);
            if (GUILayout.Button("TryAdd"))
            {
                if (playerIds.TryAdd(_newName, _newId))
                    Log($"Added '{_newName}' <-> ID {_newId}");
                else
                    Log($"Rejected duplicate! Key '{_newName}' or ID '{_newId}' already exists in BiDictionary.");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Forward / Inverse search testers
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>2. Two-Way Search Testers:</b>");
            GUILayout.BeginHorizontal();
            _searchName = GUILayout.TextField(_searchName, GUILayout.Width(120));
            if (GUILayout.Button("Find ID (Forward)"))
            {
                if (playerIds.TryGetByKey(_searchName, out int id))
                    Log($"Forward Result: '{_searchName}' -> ID: {id}");
                else
                    Log($"Forward Result: Name '{_searchName}' not found.");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            int.TryParse(GUILayout.TextField(_searchId.ToString(), GUILayout.Width(120)), out _searchId);
            if (GUILayout.Button("Find Name (Inverse)"))
            {
                if (playerIds.TryGetByValue(_searchId, out string name))
                    Log($"Inverse Result: ID {_searchId} -> Name: '{name}'");
                else
                    Log($"Inverse Result: ID {_searchId} not found.");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(150));
            string toRemove = null;
            foreach (var pair in playerIds.ToList())
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"<b>{pair.Key}</b> ↔ ID: {pair.Value}", GUILayout.Width(280));
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    toRemove = pair.Key;
                }
                GUILayout.EndHorizontal();
            }
            if (toRemove != null)
            {
                playerIds.TryRemove(toRemove);
                Log($"Removed pair with key '{toRemove}'.");
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
        private class BiDictJsonContainer
        {
            public SerializableBiDictionary<string, int> playerIds;
            public SerializableBiDictionary<PlayerBadge, string> badgeOwners;
        }

        /// <summary>
        /// Serializes bi-dictionaries to JSON using JsonUtility and outputs formatted results to Debug.Log.
        /// </summary>
        [ContextMenu("Log JSON to Console (JsonUtility)")]
        public void LogJsonToConsole()
        {
            var container = new BiDictJsonContainer
            {
                playerIds = this.playerIds,
                badgeOwners = this.badgeOwners
            };

            string json = JsonUtility.ToJson(container, prettyPrint: true);
            Debug.Log(json);
        }
    }
}
