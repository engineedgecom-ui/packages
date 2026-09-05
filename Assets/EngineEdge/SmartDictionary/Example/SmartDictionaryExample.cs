using System.Collections.Generic;
using UnityEngine;
using EngineEdge.SmartDictionary;

namespace EngineEdge.SmartDictionary.Example
{
    /// <summary>
    /// Comprehensive interactive example script demonstrating Smart Dictionary Pro features:
    /// Inspector rendering of primitive, class, and struct types across all 8 collection types,
    /// LINQ queries, event listeners, safe access API, persistence,
    /// and an on-screen runtime GUI for instant testing in Play Mode.
    /// </summary>
    [AddComponentMenu("EngineEdge/SmartDictionary/Smart Dictionary Example")]
    public class SmartDictionaryExample : MonoBehaviour
    {
        // ------------------------------------------------------------------ //
        //  Class Key & Struct Value Collections (Primary Showcase)
        // ------------------------------------------------------------------ //

        [Header("1. Class Key -> Struct Value (ObservableDictionary)")]
        [Tooltip("Key is a custom C# CLASS (CharacterProfile), Value is a custom STRUCT (CombatStats). Includes reactive events and UnityEvents!")]
        public ObservableDictionary<CharacterProfile, CombatStats> heroStats = new ObservableDictionary<CharacterProfile, CombatStats>
        {
            { new CharacterProfile("Arthur", "Paladin"),  new CombatStats(1200, 85, 95, 0.15f) },
            { new CharacterProfile("Merlin", "Mage"),     new CombatStats(650, 140, 30, 0.35f) },
            { new CharacterProfile("Robin", "Ranger"),    new CombatStats(800, 110, 50, 0.45f) }
        };

        [Header("2. Class Key -> Struct Value (SerializableDictionary)")]
        [Tooltip("Key is a custom CLASS (ItemCategoryKey), Value is a custom STRUCT (ItemModifier) — pure lightweight collection.")]
        public SerializableDictionary<ItemCategoryKey, ItemModifier> categoryModifiers = new SerializableDictionary<ItemCategoryKey, ItemModifier>
        {
            { new ItemCategoryKey("Heavy Armor", 3), new ItemModifier(0f, 0.85f, 500) },
            { new ItemCategoryKey("Daggers", 2),     new ItemModifier(25f, 1.25f, 150) },
            { new ItemCategoryKey("Staves", 4),      new ItemModifier(45f, 1.05f, 200) }
        };

        [Header("3. Class Key -> Class Value (OrderedDictionary)")]
        [Tooltip("Insertion-order preserved dictionary: Key is class (CharacterProfile), Value is class (SkillData).")]
        public SerializableOrderedDictionary<CharacterProfile, SkillData> activeSkills = new SerializableOrderedDictionary<CharacterProfile, SkillData>
        {
            { new CharacterProfile("Arthur", "Paladin"), new SkillData("Holy Shield", 25, 12f) },
            { new CharacterProfile("Merlin", "Mage"),    new SkillData("Meteor Strike", 60, 20f) },
            { new CharacterProfile("Robin", "Ranger"),   new SkillData("Rain of Arrows", 35, 8f) }
        };

        [Header("4. Struct Key -> String Value (BiDictionary)")]
        [Tooltip("Two-way lookup dictionary: Key is struct (PlayerBadge), Value is player name string.")]
        public SerializableBiDictionary<PlayerBadge, string> badgeOwners = new SerializableBiDictionary<PlayerBadge, string>
        {
            { new PlayerBadge(101, "Grandmaster"), "Player_Alpha" },
            { new PlayerBadge(102, "Sharpshooter"), "Player_Bravo" },
            { new PlayerBadge(103, "Iron Wall"),    "Player_Charlie" }
        };

        [Header("5. Nested Class Key & Value (ObservableDictionary)")]
        [Tooltip("Demonstrates deeply nested C# classes: Key is WaypointNavigation.Key (contains Coordinate), Value is WaypointNavigation.Value (contains Requirement).")]
        public ObservableDictionary<WaypointNavigation.Key, WaypointNavigation.Value> fastTravelWaypoints = new ObservableDictionary<WaypointNavigation.Key, WaypointNavigation.Value>
        {
            {
                new WaypointNavigation.Key("Sanctuary", new WaypointNavigation.Key.Coordinate(100, 250)),
                new WaypointNavigation.Value("Ancient Obelisk", new WaypointNavigation.Value.Requirement(1, 0))
            },
            {
                new WaypointNavigation.Key("Dragon Peak", new WaypointNavigation.Key.Coordinate(750, 920)),
                new WaypointNavigation.Value("Summit Beacon", new WaypointNavigation.Value.Requirement(50, 500))
            },
            {
                new WaypointNavigation.Key("Sunken Ruins", new WaypointNavigation.Key.Coordinate(320, 110)),
                new WaypointNavigation.Value("Submerged Portal", new WaypointNavigation.Value.Requirement(30, 250))
            }
        };

        [Header("6. Set of Classes (ObservableHashSet)")]
        [Tooltip("Reactive set containing custom class objects (CharacterProfile) with duplicate rejection and events.")]
        public ObservableHashSet<CharacterProfile> registeredHeroes = new ObservableHashSet<CharacterProfile>
        {
            new CharacterProfile("Arthur", "Paladin"),
            new CharacterProfile("Merlin", "Mage"),
            new CharacterProfile("Robin", "Ranger"),
            new CharacterProfile("Galahad", "Knight")
        };

        [Header("7. Set of Structs (SerializableHashSet)")]
        [Tooltip("Pure serializable set containing custom structs (CombatStats).")]
        public SerializableHashSet<CombatStats> baseStatTemplates = new SerializableHashSet<CombatStats>
        {
            new CombatStats(500, 50, 50, 0.05f),
            new CombatStats(800, 80, 70, 0.10f),
            new CombatStats(1200, 120, 100, 0.20f)
        };

        [Header("8. Stack of Structs (SerializableStack)")]
        [Tooltip("LIFO stack containing custom structs (CombatStats snapshots).")]
        public SerializableStack<CombatStats> statHistory = new SerializableStack<CombatStats>();

        [Header("9. Queue of Classes (SerializableQueue)")]
        [Tooltip("FIFO message queue containing custom class instances (CharacterProfile).")]
        public SerializableQueue<CharacterProfile> matchmakingQueue = new SerializableQueue<CharacterProfile>();

        // ------------------------------------------------------------------ //
        //  Standard Collections (Primitives)
        // ------------------------------------------------------------------ //

        [Header("Standard Inventory (Observable: string -> int)")]
        [Tooltip("Configure item names and quantities.")]
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
            heroStats = new ObservableDictionary<CharacterProfile, CombatStats>
            {
                { new CharacterProfile("Arthur", "Paladin"),  new CombatStats(1200, 85, 95, 0.15f) },
                { new CharacterProfile("Merlin", "Mage"),     new CombatStats(650, 140, 30, 0.35f) },
                { new CharacterProfile("Robin", "Ranger"),    new CombatStats(800, 110, 50, 0.45f) }
            };

            categoryModifiers = new SerializableDictionary<ItemCategoryKey, ItemModifier>
            {
                { new ItemCategoryKey("Heavy Armor", 3), new ItemModifier(0f, 0.85f, 500) },
                { new ItemCategoryKey("Daggers", 2),     new ItemModifier(25f, 1.25f, 150) },
                { new ItemCategoryKey("Staves", 4),      new ItemModifier(45f, 1.05f, 200) }
            };

            activeSkills = new SerializableOrderedDictionary<CharacterProfile, SkillData>
            {
                { new CharacterProfile("Arthur", "Paladin"), new SkillData("Holy Shield", 25, 12f) },
                { new CharacterProfile("Merlin", "Mage"),    new SkillData("Meteor Strike", 60, 20f) },
                { new CharacterProfile("Robin", "Ranger"),   new SkillData("Rain of Arrows", 35, 8f) }
            };

            badgeOwners = new SerializableBiDictionary<PlayerBadge, string>
            {
                { new PlayerBadge(101, "Grandmaster"), "Player_Alpha" },
                { new PlayerBadge(102, "Sharpshooter"), "Player_Bravo" },
                { new PlayerBadge(103, "Iron Wall"),    "Player_Charlie" }
            };

            registeredHeroes = new ObservableHashSet<CharacterProfile>
            {
                new CharacterProfile("Arthur", "Paladin"),
                new CharacterProfile("Merlin", "Mage"),
                new CharacterProfile("Robin", "Ranger"),
                new CharacterProfile("Galahad", "Knight")
            };

            baseStatTemplates = new SerializableHashSet<CombatStats>
            {
                new CombatStats(500, 50, 50, 0.05f),
                new CombatStats(800, 80, 70, 0.10f),
                new CombatStats(1200, 120, 100, 0.20f)
            };

            statHistory = new SerializableStack<CombatStats>();
            matchmakingQueue = new SerializableQueue<CharacterProfile>();

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
            // Subscribe to inventory events
            inventory.OnEntryAdded += HandleInventoryAdded;
            inventory.OnEntryRemoved += HandleInventoryRemoved;
            inventory.OnEntryUpdated += HandleInventoryUpdated;
            inventory.OnCleared += HandleInventoryCleared;
            inventory.OnCountChanged += HandleInventoryCountChanged;

            // Subscribe to hero stats events (Class Key -> Struct Value)
            heroStats.OnEntryAdded += HandleHeroStatsAdded;
            heroStats.OnEntryUpdated += HandleHeroStatsUpdated;
            heroStats.OnEntryRemoved += HandleHeroStatsRemoved;

            // Subscribe to registered heroes set events (Class in Set)
            registeredHeroes.OnItemAdded += HandleHeroRegistered;
            registeredHeroes.OnItemRemoved += HandleHeroUnregistered;
        }

        private void OnDisable()
        {
            inventory.OnEntryAdded -= HandleInventoryAdded;
            inventory.OnEntryRemoved -= HandleInventoryRemoved;
            inventory.OnEntryUpdated -= HandleInventoryUpdated;
            inventory.OnCleared -= HandleInventoryCleared;
            inventory.OnCountChanged -= HandleInventoryCountChanged;

            heroStats.OnEntryAdded -= HandleHeroStatsAdded;
            heroStats.OnEntryUpdated -= HandleHeroStatsUpdated;
            heroStats.OnEntryRemoved -= HandleHeroStatsRemoved;

            registeredHeroes.OnItemAdded -= HandleHeroRegistered;
            registeredHeroes.OnItemRemoved -= HandleHeroUnregistered;
        }

        private void Start()
        {
            LogAction("=== SmartDictionary Example Started ===");
            LogAction($"Loaded {heroStats.Count} Heroes (Class Key -> Struct Value)");
            LogAction($"Loaded {categoryModifiers.Count} Category Modifiers (Class Key -> Struct Value)");
            LogAction($"Loaded {registeredHeroes.Count} Registered Heroes in Set");

            // Populate initial stack & queue for demonstration
            statHistory.Push(new CombatStats(100, 10, 5, 0.05f));
            statHistory.Push(new CombatStats(250, 25, 15, 0.10f));

            matchmakingQueue.Enqueue(new CharacterProfile("Lancelot", "Knight"));
            matchmakingQueue.Enqueue(new CharacterProfile("Morgana", "Witch"));

            // Demonstrate foreach with class key & struct value
            foreach (var (hero, stats) in heroStats)
            {
                Debug.Log($"[HeroStats] {hero} => {stats}");
            }
        }

        // ------------------------------------------------------------------ //
        //  Event Handlers
        // ------------------------------------------------------------------ //

        private void HandleHeroStatsAdded(CharacterProfile hero, CombatStats stats)
        {
            LogAction($"[EVENT] Hero Added: {hero} with stats: {stats}");
        }

        private void HandleHeroStatsUpdated(CharacterProfile hero, CombatStats oldStats, CombatStats newStats)
        {
            LogAction($"[EVENT] Hero Updated: {hero} ATK changed from {oldStats.attackPower} -> {newStats.attackPower}");
        }

        private void HandleHeroStatsRemoved(CharacterProfile hero, CombatStats stats)
        {
            LogAction($"[EVENT] Hero Removed: {hero} (had stats: {stats})");
        }

        private void HandleHeroRegistered(CharacterProfile hero)
        {
            LogAction($"[EVENT] Hero Registered in Set: {hero}");
        }

        private void HandleHeroUnregistered(CharacterProfile hero)
        {
            LogAction($"[EVENT] Hero Unregistered from Set: {hero}");
        }

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
            LogAction($"[EVENT] Total unique items count: {count}");
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
            GUILayout.BeginArea(new Rect(15, 15, 420, Screen.height - 30), GUI.skin.box);

            GUILayout.Label("<b><size=14>Smart Dictionary Pro — Live Demo</size></b>", GUI.skin.label);
            GUILayout.Label("Demonstrating Class Keys, Struct Values, and All Collections:\n", GUI.skin.label);

            // 1. Class Key -> Struct Value Actions
            GUILayout.Label("<b>Class Key -> Struct Value Actions:</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add New Hero (Class Key)"))
            {
                string[] heroNames = { "Percival", "Kay", "Tristan", "Galahad", "Bors" };
                string[] heroClasses = { "Knight", "Ranger", "Berserker", "Cleric" };
                string name = heroNames[Random.Range(0, heroNames.Length)] + " " + Random.Range(10, 99);
                string cls = heroClasses[Random.Range(0, heroClasses.Length)];

                var hero = new CharacterProfile(name, cls);
                var stats = new CombatStats(Random.Range(500, 1500), Random.Range(40, 120), Random.Range(20, 80), Random.Range(0.05f, 0.40f));
                heroStats.TryAdd(hero, stats);
            }

            if (GUILayout.Button("Buff Arthur (+25 ATK)"))
            {
                var arthurKey = new CharacterProfile("Arthur", "Paladin");
                if (heroStats.TryGetValue(arthurKey, out var currentStats))
                {
                    currentStats.attackPower += 25;
                    heroStats[arthurKey] = currentStats; // triggers OnEntryUpdated event
                }
                else
                {
                    LogAction("Arthur not found in heroStats!");
                }
            }
            GUILayout.EndHorizontal();

            // 2. Bonus Collections (Stack, Queue, BiDictionary)
            GUILayout.Space(6);
            GUILayout.Label("<b>Bonus Collections (Stack, Queue, BiDict):</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Push Stat to Stack"))
            {
                var snapshot = new CombatStats(Random.Range(500, 2000), Random.Range(50, 150), Random.Range(30, 90), 0.25f);
                statHistory.Push(snapshot);
                LogAction($"Pushed to Stack. Total: {statHistory.Count}. Top: {snapshot}");
            }

            if (GUILayout.Button("Pop Stat from Stack"))
            {
                if (statHistory.TryPop(out var popped))
                {
                    LogAction($"Popped from Stack: {popped}. Left: {statHistory.Count}");
                }
                else
                {
                    LogAction("Stack is empty!");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Enqueue Hero to Queue"))
            {
                var newHero = new CharacterProfile("Recruit " + Random.Range(1, 99), "Novice");
                matchmakingQueue.Enqueue(newHero);
                LogAction($"Enqueued {newHero}. Queue length: {matchmakingQueue.Count}");
            }

            if (GUILayout.Button("Dequeue Matchmaking"))
            {
                if (matchmakingQueue.TryDequeue(out var dequeued))
                {
                    LogAction($"Dequeued from Queue: {dequeued}. Left: {matchmakingQueue.Count}");
                }
                else
                {
                    LogAction("Matchmaking queue is empty!");
                }
            }
            GUILayout.EndHorizontal();

            // 3. BiDictionary & LINQ
            GUILayout.Space(6);
            GUILayout.Label("<b>BiDictionary & LINQ Queries:</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Lookup BiDict Owner"))
            {
                var badge = new PlayerBadge(101, "Grandmaster");
                if (badgeOwners.TryGetByKey(badge, out var owner))
                {
                    LogAction($"BiDict: Badge '{badge.title}' belongs to '{owner}'");
                }
                if (badgeOwners.TryGetByValue("Player_Alpha", out var pBadge))
                {
                    LogAction($"BiDict Reverse: 'Player_Alpha' owns badge '{pBadge}'");
                }
            }

            if (GUILayout.Button("Sum Inventory"))
            {
                int totalQuantity = inventory.SumValues();
                LogAction($"--- LINQ: Total inventory quantity: {totalQuantity} items ---");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Lookup Nested Key Waypoint"))
            {
                // Tests value equality lookup using a new nested instance with identical data
                var queryKey = new WaypointNavigation.Key("Dragon Peak", new WaypointNavigation.Key.Coordinate(750, 920));
                if (fastTravelWaypoints.TryGetValue(queryKey, out var dest))
                {
                    LogAction($"Nested Key Found: '{queryKey}' => Beacon: '{dest.BeaconName}' ({dest.Req})");
                }
                else
                {
                    LogAction($"Nested Key '{queryKey}' not found!");
                }
            }
            GUILayout.EndHorizontal();

            // 4. Live Event Console
            GUILayout.Space(10);
            GUILayout.Label("<b>Live Event & Action Log:</b>");

            _logScrollPosition = GUILayout.BeginScrollView(_logScrollPosition, GUILayout.Height(200));
            for (int i = _actionLogs.Count - 1; i >= 0; i--)
            {
                GUILayout.Label(_actionLogs[i]);
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }
    }
}
