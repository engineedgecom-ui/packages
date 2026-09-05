using System.Collections.Generic;
using UnityEngine;
using EngineEdge.SmartDictionary;

namespace EngineEdge.SmartDictionary.Example
{
    /// <summary>
    /// Comprehensive interactive example script demonstrating Smart Dictionary Pro features:
    /// Inspector rendering, LINQ queries, event listeners, safe access API, persistence,
    /// and an on-screen runtime GUI for instant testing in Play Mode.
    /// </summary>
    [AddComponentMenu("EngineEdge/SmartDictionary/Smart Dictionary Example")]
    public class SmartDictionaryExample : MonoBehaviour
    {
        // ------------------------------------------------------------------ //
        //  Inspector-Visible Collections
        // ------------------------------------------------------------------ //

        [Header("Item Inventory (Observable: string -> int)")]
        [Tooltip("Reactive dictionary with events. Supports duplicates warning, search filtering, and UnityEvents toolbar in Inspector.")]
        public ObservableDictionary<string, int> inventory = new ObservableDictionary<string, int>
        {
            { "Health Potion", 15 },
            { "Mana Potion", 8 },
            { "Iron Sword", 1 },
            { "Gold Coins", 250 },
            { "Magic Scroll", 3 }
        };

        [Header("Character Stats (string -> float)")]
        [Tooltip("Player attribute modifiers.")]
        public SerializableDictionary<string, float> characterStats = new SerializableDictionary<string, float>
        {
            { "MovementSpeed", 6.5f },
            { "AttackDamage", 42.0f },
            { "CriticalChance", 0.25f },
            { "Defense", 18.0f }
        };

        [Header("Faction Colors (string -> Color)")]
        [Tooltip("Team or faction theme colors.")]
        public SerializableDictionary<string, Color> factionColors = new SerializableDictionary<string, Color>
        {
            { "Knights", new Color(0.2f, 0.6f, 1f, 1f) },
            { "Bandits", new Color(0.9f, 0.2f, 0.2f, 1f) },
            { "Wizards", new Color(0.6f, 0.2f, 0.9f, 1f) }
        };

        [Header("Bonus Collections")]
        [Tooltip("Bonus collection types included in the package.")]
        public SerializableHashSet<string> unlockedAchievements = new SerializableHashSet<string>
        {
            "First Blood",
            "Treasure Hunter",
            "Master Crafter"
        };

        public SerializableOrderedDictionary<string, int> leaderboard = new SerializableOrderedDictionary<string, int>
        {
            { "Player_Alpha", 9500 },
            { "Player_Bravo", 8200 },
            { "Player_Charlie", 7100 }
        };

        // ------------------------------------------------------------------ //
        //  Runtime State & Event Log
        // ------------------------------------------------------------------ //

        private readonly List<string> _actionLogs = new List<string>();
        private Vector2 _logScrollPosition;

        // ------------------------------------------------------------------ //
        //  Unity Lifecycle & Event Subscriptions
        // ------------------------------------------------------------------ //

        private void Reset()
        {
            inventory = new ObservableDictionary<string, int>
            {
                { "Health Potion", 15 },
                { "Mana Potion", 8 },
                { "Iron Sword", 1 },
                { "Gold Coins", 250 },
                { "Magic Scroll", 3 }
            };

            characterStats = new SerializableDictionary<string, float>
            {
                { "MovementSpeed", 6.5f },
                { "AttackDamage", 42.0f },
                { "CriticalChance", 0.25f },
                { "Defense", 18.0f }
            };

            factionColors = new SerializableDictionary<string, Color>
            {
                { "Knights", new Color(0.2f, 0.6f, 1f, 1f) },
                { "Bandits", new Color(0.9f, 0.2f, 0.2f, 1f) },
                { "Wizards", new Color(0.6f, 0.2f, 0.9f, 1f) }
            };

            unlockedAchievements = new SerializableHashSet<string>
            {
                "First Blood",
                "Treasure Hunter",
                "Master Crafter"
            };

            leaderboard = new SerializableOrderedDictionary<string, int>
            {
                { "Player_Alpha", 9500 },
                { "Player_Bravo", 8200 },
                { "Player_Charlie", 7100 }
            };
        }

        private void OnEnable()
        {
            // Subscribe to dictionary mutation events
            inventory.OnEntryAdded += HandleInventoryAdded;
            inventory.OnEntryRemoved += HandleInventoryRemoved;
            inventory.OnEntryUpdated += HandleInventoryUpdated;
            inventory.OnCleared += HandleInventoryCleared;
            inventory.OnCountChanged += HandleInventoryCountChanged;
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent leaks
            inventory.OnEntryAdded -= HandleInventoryAdded;
            inventory.OnEntryRemoved -= HandleInventoryRemoved;
            inventory.OnEntryUpdated -= HandleInventoryUpdated;
            inventory.OnCleared -= HandleInventoryCleared;
            inventory.OnCountChanged -= HandleInventoryCountChanged;
        }

        private void Start()
        {
            LogAction("=== SmartDictionary Example Started ===");
            LogAction($"Initial inventory items: {inventory.Count}");

            // Demonstrate foreach tuple deconstruction
            foreach (var (itemName, quantity) in inventory)
            {
                Debug.Log($"[Inventory] {itemName}: {quantity}");
            }
        }

        // ------------------------------------------------------------------ //
        //  Event Handlers
        // ------------------------------------------------------------------ //

        private void HandleInventoryAdded(string key, int value)
        {
            LogAction($"[EVENT] Added item: '{key}' (qty: {value})");
        }

        private void HandleInventoryRemoved(string key, int value)
        {
            LogAction($"[EVENT] Removed item: '{key}' (was: {value})");
        }

        private void HandleInventoryUpdated(string key, int oldValue, int newValue)
        {
            LogAction($"[EVENT] Updated '{key}': {oldValue} -> {newValue}");
        }

        private void HandleInventoryCleared()
        {
            LogAction("[EVENT] Inventory was cleared!");
        }

        private void HandleInventoryCountChanged(int count)
        {
            LogAction($"[EVENT] Total unique items count is now: {count}");
        }

        // ------------------------------------------------------------------ //
        //  Helper & Logging
        // ------------------------------------------------------------------ //

        private void LogAction(string message)
        {
            _actionLogs.Add($"[{System.DateTime.Now:HH:mm:ss}] {message}");
            if (_actionLogs.Count > 40)
            {
                _actionLogs.RemoveAt(0);
            }
            Debug.Log($"[SmartDictionaryExample] {message}");
        }

        // ------------------------------------------------------------------ //
        //  Interactive OnGUI Overlay
        // ------------------------------------------------------------------ //

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(15, 15, 380, Screen.height - 30), GUI.skin.box);

            GUILayout.Label("<b><size=14>Smart Dictionary Pro — Live Demo</size></b>", GUI.skin.label);
            GUILayout.Label("Click buttons below to test runtime API & events:\n", GUI.skin.label);

            // 1. Safe Add / Modify
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Gold (+50)"))
            {
                int currentGold = inventory.GetOrDefault("Gold Coins", 0);
                inventory.AddOrUpdate("Gold Coins", currentGold + 50);
            }

            if (GUILayout.Button("Add New Potion"))
            {
                string potionName = "Elixir of Speed " + Random.Range(1, 99);
                inventory.TryAdd(potionName, Random.Range(1, 5));
            }
            GUILayout.EndHorizontal();

            // 2. Safe Remove
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Consume Health Potion"))
            {
                if (inventory.TryGetValue("Health Potion", out int count))
                {
                    if (count > 1)
                        inventory["Health Potion"] = count - 1;
                    else
                        inventory.Remove("Health Potion");
                }
                else
                {
                    LogAction("No Health Potions left in inventory!");
                }
            }

            if (GUILayout.Button("Clear All Items"))
            {
                inventory.Clear();
            }
            GUILayout.EndHorizontal();

            // 3. LINQ queries
            GUILayout.Space(6);
            GUILayout.Label("<b>LINQ Queries:</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Filter (Qty >= 5)"))
            {
                var highQty = inventory.WhereDict((k, v) => v >= 5);
                LogAction($"--- LINQ: Found {highQty.Count} items with qty >= 5 ---");
                foreach (var (k, v) in highQty)
                {
                    LogAction($"  {k}: {v}");
                }
            }

            if (GUILayout.Button("Sum Total Items"))
            {
                int totalQuantity = inventory.SumValues();
                int maxStack = inventory.Count > 0 ? inventory.MaxValue() : 0;
                LogAction($"--- LINQ: Total items count: {totalQuantity} | Max stack: {maxStack} ---");
            }
            GUILayout.EndHorizontal();

            // 4. Persistence
            GUILayout.Space(6);
            GUILayout.Label("<b>Serialization & Persistence:</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save to PlayerPrefs"))
            {
                inventory.ToPlayerPrefs("DemoInventory");
                LogAction("Saved inventory to PlayerPrefs ('DemoInventory')");
            }

            if (GUILayout.Button("Load from PlayerPrefs"))
            {
                inventory.FromPlayerPrefs("DemoInventory");
                LogAction("Loaded inventory from PlayerPrefs ('DemoInventory')");
            }
            GUILayout.EndHorizontal();

            // 5. Live Event Console
            GUILayout.Space(10);
            GUILayout.Label("<b>Live Event & Action Log:</b>");

            _logScrollPosition = GUILayout.BeginScrollView(_logScrollPosition, GUILayout.Height(220));
            for (int i = _actionLogs.Count - 1; i >= 0; i--)
            {
                GUILayout.Label(_actionLogs[i]);
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }
    }
}
