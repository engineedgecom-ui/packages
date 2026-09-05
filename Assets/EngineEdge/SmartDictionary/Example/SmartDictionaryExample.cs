using System.Collections.Generic;
using System.Linq;
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
        [Tooltip("Reactive dictionary with nested C# classes: Key is WaypointNavigation.Key (contains Coordinate), Value is WaypointNavigation.Value (contains Requirement).")]
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

        [Header("6. Nested Class Key & Value (SerializableDictionary)")]
        [Tooltip("Pure serializable dictionary version of nested class key and values.")]
        public SerializableDictionary<WaypointNavigation.Key, WaypointNavigation.Value> waypointRegistry = new SerializableDictionary<WaypointNavigation.Key, WaypointNavigation.Value>
        {
            {
                new WaypointNavigation.Key("Capital City", new WaypointNavigation.Key.Coordinate(0, 0)),
                new WaypointNavigation.Value("Grand Portal", new WaypointNavigation.Value.Requirement(1, 0))
            },
            {
                new WaypointNavigation.Key("Frozen Wastes", new WaypointNavigation.Key.Coordinate(900, 1400)),
                new WaypointNavigation.Value("Ice Pillar", new WaypointNavigation.Value.Requirement(70, 1200))
            }
        };

        [Header("7. Nested Dictionary (Dictionary of Dictionaries)")]
        [Tooltip("A dictionary whose Value is another inner Dictionary (CharacterProfile -> SkillTreeDictionary: string -> int).")]
        public SerializableDictionary<CharacterProfile, SkillTreeDictionary> heroSkillTrees = new SerializableDictionary<CharacterProfile, SkillTreeDictionary>
        {
            {
                new CharacterProfile("Arthur", "Paladin"),
                new SkillTreeDictionary
                {
                    { "Holy Strike", 5 },
                    { "Divine Aura", 3 },
                    { "Lay on Hands", 1 }
                }
            },
            {
                new CharacterProfile("Merlin", "Mage"),
                new SkillTreeDictionary
                {
                    { "Fireball", 10 },
                    { "Frost Nova", 4 },
                    { "Teleport", 2 }
                }
            }
        };

        [Header("8. 6-Layer Deep Nested Key (SerializableDictionary)")]
        [Tooltip("Demonstrates a Key with 6 full levels of nested classes: GalacticRouteKey -> QuadrantData -> StarSystemData -> PlanetOrbitData -> SurfaceSectorData -> SubGridCoordinate.")]
        public SerializableDictionary<GalacticRouteKey, SpaceStationInfo> galacticStations = new SerializableDictionary<GalacticRouteKey, SpaceStationInfo>
        {
            {
                new GalacticRouteKey(
                    "MilkyWay",
                    new QuadrantData(
                        "Alpha-7",
                        new StarSystemData(
                            104,
                            new PlanetOrbitData(
                                "Sol-3",
                                new SurfaceSectorData(
                                    "Sector-A",
                                    new SubGridCoordinate(42, 88)
                                )
                            )
                        )
                    )
                ),
                new SpaceStationInfo("Earth Orbital Defense", 9500, false)
            },
            {
                new GalacticRouteKey(
                    "Andromeda",
                    new QuadrantData(
                        "Omega-9",
                        new StarSystemData(
                            880,
                            new PlanetOrbitData(
                                "Xylar-Prime",
                                new SurfaceSectorData(
                                    "Sector-D",
                                    new SubGridCoordinate(999, 120)
                                )
                            )
                        )
                    )
                ),
                new SpaceStationInfo("Pirate Dreadnought Outpost", 14200, true)
            }
        };

        [Header("9. Set of Classes (ObservableHashSet)")]
        [Tooltip("Reactive set containing custom class objects (CharacterProfile) with duplicate rejection and events.")]
        public ObservableHashSet<CharacterProfile> registeredHeroes = new ObservableHashSet<CharacterProfile>
        {
            new CharacterProfile("Arthur", "Paladin"),
            new CharacterProfile("Merlin", "Mage"),
            new CharacterProfile("Robin", "Ranger"),
            new CharacterProfile("Galahad", "Knight")
        };

        [Header("10. Set of Structs (SerializableHashSet)")]
        [Tooltip("Pure serializable set containing custom structs (CombatStats).")]
        public SerializableHashSet<CombatStats> baseStatTemplates = new SerializableHashSet<CombatStats>
        {
            new CombatStats(500, 50, 50, 0.05f),
            new CombatStats(800, 80, 70, 0.10f),
            new CombatStats(1200, 120, 100, 0.20f)
        };

        [Header("11. Stack of Structs (SerializableStack)")]
        [Tooltip("LIFO stack containing custom structs (CombatStats snapshots).")]
        public SerializableStack<CombatStats> statHistory = new SerializableStack<CombatStats>();

        [Header("12. Queue of Classes (SerializableQueue)")]
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
        private Vector2 _tabScrollPosition;
        private Vector2 _entriesScrollPosition;
        private Vector2 _logScrollPosition;

        private readonly string[] _tabNames = new string[]
        {
            "🛡️ Hero Stats (Class/Struct)",
            "🎒 Inventory (String->Int)",
            "👥 Hero Roster (Set)",
            "📜 Ordered Skills",
            "📦 Stack & Queue",
            "🌌 Nested & 6-Layer"
        };
        private int _selectedTab = 0;

        // Input Fields State for Runtime GUI
        private string _newHeroName = "Galahad";
        private string _newHeroClass = "Knight";
        private int _newHeroHp = 950;
        private int _newHeroAtk = 80;
        private int _newHeroDef = 60;

        private string _newItemName = "Elixir of Life";
        private int _newItemQty = 3;

        private string _rosterHeroName = "Bors";
        private string _rosterHeroClass = "Berserker";

        private string _skillHeroName = "Kay";
        private string _skillHeroClass = "Assassin";
        private string _skillName = "Shadow Step";
        private int _skillMana = 20;

        private int _stackHp = 750;
        private int _stackAtk = 65;
        private string _queueHeroName = "Tristan";
        private string _queueHeroClass = "Archer";

        private string _wpZone = "Crystal Cavern";
        private int _wpX = 450;
        private int _wpY = 820;
        private string _wpBeacon = "Glowstone Spire";
        private int _wpLvl = 25;
        private int _wpGold = 150;

        // ------------------------------------------------------------------ //
        //  Unity Lifecycle & Event Subscriptions
        // ------------------------------------------------------------------ //

        private void Reset()
        {
            ResetAllCollections();
        }

        public void ResetAllCollections()
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
            heroStats.OnCleared += HandleHeroStatsCleared;
            heroStats.OnCountChanged += HandleHeroStatsCountChanged;

            // Subscribe to registered heroes set events (Class in Set)
            registeredHeroes.OnItemAdded += HandleHeroRegistered;
            registeredHeroes.OnItemRemoved += HandleHeroUnregistered;
            registeredHeroes.OnCleared += HandleRosterCleared;
            registeredHeroes.OnCountChanged += HandleRosterCountChanged;

            // Subscribe to waypoint events
            fastTravelWaypoints.OnEntryAdded += HandleWaypointAdded;
            fastTravelWaypoints.OnEntryRemoved += HandleWaypointRemoved;
            fastTravelWaypoints.OnCleared += HandleWaypointCleared;

            // Subscribe to stack & queue events
            statHistory.OnPushed += HandleStatPushed;
            statHistory.OnPopped += HandleStatPopped;
            statHistory.OnCleared += HandleStatCleared;

            matchmakingQueue.OnEnqueued += HandleHeroEnqueued;
            matchmakingQueue.OnDequeued += HandleHeroDequeued;
            matchmakingQueue.OnCleared += HandleQueueCleared;
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
            heroStats.OnCleared -= HandleHeroStatsCleared;
            heroStats.OnCountChanged -= HandleHeroStatsCountChanged;

            registeredHeroes.OnItemAdded -= HandleHeroRegistered;
            registeredHeroes.OnItemRemoved -= HandleHeroUnregistered;
            registeredHeroes.OnCleared -= HandleRosterCleared;
            registeredHeroes.OnCountChanged -= HandleRosterCountChanged;

            fastTravelWaypoints.OnEntryAdded -= HandleWaypointAdded;
            fastTravelWaypoints.OnEntryRemoved -= HandleWaypointRemoved;
            fastTravelWaypoints.OnCleared -= HandleWaypointCleared;

            statHistory.OnPushed -= HandleStatPushed;
            statHistory.OnPopped -= HandleStatPopped;
            statHistory.OnCleared -= HandleStatCleared;

            matchmakingQueue.OnEnqueued -= HandleHeroEnqueued;
            matchmakingQueue.OnDequeued -= HandleHeroDequeued;
            matchmakingQueue.OnCleared -= HandleQueueCleared;
        }

        private void Awake()
        {
            if (statHistory.Count == 0)
            {
                statHistory.Push(new CombatStats(100, 10, 5, 0.05f));
                statHistory.Push(new CombatStats(250, 25, 15, 0.10f));
            }

            if (matchmakingQueue.Count == 0)
            {
                matchmakingQueue.Enqueue(new CharacterProfile("Lancelot", "Knight"));
                matchmakingQueue.Enqueue(new CharacterProfile("Morgana", "Witch"));
            }

            Debug.Log(heroStats.ToStandardJson(prettyPrint: true));
        }

        private void Start()
        {
        }

        // ------------------------------------------------------------------ //
        //  Event Handlers
        // ------------------------------------------------------------------ //

        private void HandleHeroStatsAdded(CharacterProfile hero, CombatStats stats) =>
            LogAction($"Hero Added: <b>{hero}</b> (HP:{stats.health}, ATK:{stats.attackPower}, DEF:{stats.defense})", "<color=#55FF55>[HERO ADDED]</color>");

        private void HandleHeroStatsUpdated(CharacterProfile hero, CombatStats oldStats, CombatStats newStats) =>
            LogAction($"Hero Updated: <b>{hero}</b> ATK: {oldStats.attackPower} -> {newStats.attackPower} | HP: {oldStats.health} -> {newStats.health}", "<color=#FFFF55>[HERO UPDATED]</color>");

        private void HandleHeroStatsRemoved(CharacterProfile hero, CombatStats stats) =>
            LogAction($"Hero Removed: <b>{hero}</b>", "<color=#FF5555>[HERO REMOVED]</color>");

        private void HandleHeroStatsCleared() =>
            LogAction("All heroes cleared from HeroStats dictionary!", "<color=#55FFFF>[HERO CLEARED]</color>");

        private void HandleHeroStatsCountChanged(int count) =>
            LogAction($"HeroStats count changed: {count}", "<color=#AAAAAA>[COUNT CHANGED]</color>");

        private void HandleInventoryAdded(string key, int value) =>
            LogAction($"Added item: '<b>{key}</b>' (Qty: {value})", "<color=#55FF55>[INV ADDED]</color>");

        private void HandleInventoryRemoved(string key, int value) =>
            LogAction($"Removed item: '<b>{key}</b>' (was: {value})", "<color=#FF5555>[INV REMOVED]</color>");

        private void HandleInventoryUpdated(string key, int oldValue, int newValue) =>
            LogAction($"Updated '<b>{key}</b>': {oldValue} -> {newValue}", "<color=#FFFF55>[INV UPDATED]</color>");

        private void HandleInventoryCleared() =>
            LogAction("Inventory was cleared!", "<color=#55FFFF>[INV CLEARED]</color>");

        private void HandleInventoryCountChanged(int count) =>
            LogAction($"Inventory count changed: {count}", "<color=#AAAAAA>[COUNT CHANGED]</color>");

        private void HandleHeroRegistered(CharacterProfile hero) =>
            LogAction($"Hero Registered in Set: <b>{hero}</b>", "<color=#55FF55>[SET ADDED]</color>");

        private void HandleHeroUnregistered(CharacterProfile hero) =>
            LogAction($"Hero Unregistered from Set: <b>{hero}</b>", "<color=#FF5555>[SET REMOVED]</color>");

        private void HandleRosterCleared() =>
            LogAction("Hero Roster Set cleared!", "<color=#55FFFF>[SET CLEARED]</color>");

        private void HandleRosterCountChanged(int count) =>
            LogAction($"Hero Roster count changed: {count}", "<color=#AAAAAA>[COUNT CHANGED]</color>");

        private void HandleWaypointAdded(WaypointNavigation.Key key, WaypointNavigation.Value val) =>
            LogAction($"Waypoint Added: '<b>{key.ZoneName}</b>' -> '{val.BeaconName}'", "<color=#55FF55>[WP ADDED]</color>");

        private void HandleWaypointRemoved(WaypointNavigation.Key key, WaypointNavigation.Value val) =>
            LogAction($"Waypoint Removed: '<b>{key.ZoneName}</b>'", "<color=#FF5555>[WP REMOVED]</color>");

        private void HandleWaypointCleared() =>
            LogAction("Waypoints cleared!", "<color=#55FFFF>[WP CLEARED]</color>");

        private void HandleStatPushed(CombatStats stats) =>
            LogAction($"Stat Pushed to Stack: {stats}", "<color=#55FF55>[STACK PUSH]</color>");

        private void HandleStatPopped(CombatStats stats) =>
            LogAction($"Stat Popped from Stack: {stats}", "<color=#FF5555>[STACK POP]</color>");

        private void HandleStatCleared() =>
            LogAction("Stat Stack cleared!", "<color=#55FFFF>[STACK CLEAR]</color>");

        private void HandleHeroEnqueued(CharacterProfile hero) =>
            LogAction($"Hero Enqueued: <b>{hero}</b>", "<color=#55FF55>[QUEUE ENQUEUE]</color>");

        private void HandleHeroDequeued(CharacterProfile hero) =>
            LogAction($"Hero Dequeued: <b>{hero}</b>", "<color=#FF5555>[QUEUE DEQUEUE]</color>");

        private void HandleQueueCleared() =>
            LogAction("Matchmaking Queue cleared!", "<color=#55FFFF>[QUEUE CLEAR]</color>");

        // ------------------------------------------------------------------ //
        //  Helper & Logging
        // ------------------------------------------------------------------ //

        [System.Serializable]
        private class SmartDictionarySaveData
        {
            public ObservableDictionary<CharacterProfile, CombatStats> heroStats;
            public SerializableDictionary<ItemCategoryKey, ItemModifier> categoryModifiers;
            public SerializableOrderedDictionary<CharacterProfile, SkillData> activeSkills;
            public SerializableBiDictionary<PlayerBadge, string> badgeOwners;
            public ObservableDictionary<WaypointNavigation.Key, WaypointNavigation.Value> fastTravelWaypoints;
            public SerializableDictionary<CharacterProfile, SkillTreeDictionary> heroSkillTrees;
            public ObservableHashSet<CharacterProfile> registeredHeroes;
            public SerializableHashSet<CombatStats> baseStatTemplates;
            public ObservableDictionary<string, int> inventory;
        }

        private void LogAction(string message, string tag = null)
        {
            string time = System.DateTime.Now.ToString("HH:mm:ss");
            string formatted = string.IsNullOrEmpty(tag) ? $"[{time}] {message}" : $"[{time}] {tag} {message}";
            _actionLogs.Add(formatted);
            if (_actionLogs.Count > 60)
            {
                _actionLogs.RemoveAt(0);
            }
        }

        // ------------------------------------------------------------------ //
        //  Interactive OnGUI Overlay
        // ------------------------------------------------------------------ //

        private void OnGUI()
        {
            float width = Mathf.Min(720f, Screen.width - 30f);
            float height = Screen.height - 30f;
            GUILayout.BeginArea(new Rect(15, 15, width, height), GUI.skin.box);

            // ── Top Header ──────────────────────────────────────────────────
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b><size=15>Smart Dictionary Pro — Runtime Testing Suite</size></b>");
            
            bool currentEvents = heroStats.EventsEnabled;
            bool newEvents = GUILayout.Toggle(currentEvents, " Events Enabled", GUILayout.Width(130));
            if (newEvents != currentEvents)
            {
                heroStats.EventsEnabled = newEvents;
                inventory.EventsEnabled = newEvents;
                registeredHeroes.EventsEnabled = newEvents;
                fastTravelWaypoints.EventsEnabled = newEvents;
                LogAction($"Mutation Events {(newEvents ? "ENABLED" : "MUTED")}", newEvents ? "<color=#55FF55>[EVENTS ON]</color>" : "<color=#FFAA33>[EVENTS OFF]</color>");
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("<color=#CCCCCC><size=10>Test real-time adding, removing, updating, reordering, and duplicate rejection across all collection types.</size></color>");
            GUILayout.Space(4);

            // ── Tab Navigation ──────────────────────────────────────────────
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames, GUILayout.Height(28));
            GUILayout.Space(6);

            // ── Main Body ScrollView ────────────────────────────────────────
            _tabScrollPosition = GUILayout.BeginScrollView(_tabScrollPosition, GUILayout.Height(Mathf.Max(260, height - 260)));

            switch (_selectedTab)
            {
                case 0:
                    DrawHeroStatsTab();
                    break;
                case 1:
                    DrawInventoryTab();
                    break;
                case 2:
                    DrawHeroRosterTab();
                    break;
                case 3:
                    DrawOrderedSkillsTab();
                    break;
                case 4:
                    DrawStackQueueTab();
                    break;
                case 5:
                    DrawNestedAndSixLayerTab();
                    break;
            }

            GUILayout.EndScrollView();

            // ── Live Event & Mutation Log ────────────────────────────────────
            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Live Event & Mutation Log ({_actionLogs.Count} messages):</b>");
            if (GUILayout.Button("Clear Log", GUILayout.Width(80), GUILayout.Height(20)))
            {
                _actionLogs.Clear();
            }
            GUILayout.EndHorizontal();

            _logScrollPosition = GUILayout.BeginScrollView(_logScrollPosition, GUILayout.Height(130));
            for (int i = _actionLogs.Count - 1; i >= 0; i--)
            {
                GUILayout.Label(_actionLogs[i]);
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        // ------------------------------------------------------------------ //
        //  Tab 0: Hero Stats (Class Key -> Struct Value)
        // ------------------------------------------------------------------ //

        private void DrawHeroStatsTab()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>1. Add New Hero (Class Key -> Struct Value)</b>");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(45));
            _newHeroName = GUILayout.TextField(_newHeroName, GUILayout.Width(110));
            GUILayout.Label("Class:", GUILayout.Width(45));
            _newHeroClass = GUILayout.TextField(_newHeroClass, GUILayout.Width(110));
            GUILayout.Label("HP:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(_newHeroHp.ToString(), GUILayout.Width(50)), out _newHeroHp);
            GUILayout.Label("ATK:", GUILayout.Width(35));
            int.TryParse(GUILayout.TextField(_newHeroAtk.ToString(), GUILayout.Width(50)), out _newHeroAtk);
            GUILayout.Label("DEF:", GUILayout.Width(35));
            int.TryParse(GUILayout.TextField(_newHeroDef.ToString(), GUILayout.Width(50)), out _newHeroDef);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("TryAdd Hero (Unique)", GUILayout.Height(26)))
            {
                var key = new CharacterProfile(_newHeroName, _newHeroClass);
                var val = new CombatStats(_newHeroHp, _newHeroAtk, _newHeroDef, 0.20f);
                if (heroStats.TryAdd(key, val))
                {
                    LogAction($"TryAdd succeeded for: <b>{key}</b>", "<color=#55FF55>[SUCCESS]</color>");
                }
                else
                {
                    LogAction($"TryAdd failed! Hero <b>{key}</b> already exists in dictionary!", "<color=#FFAA33>[DUPLICATE REJECTED]</color>");
                }
            }

            if (GUILayout.Button("Add or Overwrite", GUILayout.Height(26)))
            {
                var key = new CharacterProfile(_newHeroName, _newHeroClass);
                var val = new CombatStats(_newHeroHp, _newHeroAtk, _newHeroDef, 0.20f);
                heroStats[key] = val;
            }

            if (GUILayout.Button("+ Roll Random Hero", GUILayout.Height(26)))
            {
                string[] names = { "Lancelot", "Gawain", "Percival", "Tristan", "Galahad", "Bors", "Bedivere", "Kay" };
                string[] classes = { "Paladin", "Warrior", "Ranger", "Mage", "Rogue", "Cleric" };
                string name = names[Random.Range(0, names.Length)] + " " + Random.Range(10, 99);
                string cls = classes[Random.Range(0, classes.Length)];
                var key = new CharacterProfile(name, cls);
                var val = new CombatStats(Random.Range(500, 1600), Random.Range(40, 150), Random.Range(20, 90), Random.Range(0.05f, 0.50f));
                heroStats.TryAdd(key, val);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // Live Table
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Current Heroes in Dictionary ({heroStats.Count} entries):</b>");
            if (GUILayout.Button("Clear All", GUILayout.Width(75), GUILayout.Height(20)))
            {
                heroStats.Clear();
            }
            if (GUILayout.Button("Reset Defaults", GUILayout.Width(100), GUILayout.Height(20)))
            {
                heroStats.Clear();
                heroStats.Add(new CharacterProfile("Arthur", "Paladin"), new CombatStats(1200, 85, 95, 0.15f));
                heroStats.Add(new CharacterProfile("Merlin", "Mage"), new CombatStats(650, 140, 30, 0.35f));
                heroStats.Add(new CharacterProfile("Robin", "Ranger"), new CombatStats(800, 110, 50, 0.45f));
            }
            GUILayout.EndHorizontal();

            if (heroStats.Count == 0)
            {
                GUILayout.Label("<color=#888888><i>Dictionary is currently empty. Use the controls above to add heroes!</i></color>");
            }
            else
            {
                CharacterProfile toRemove = null;
                foreach (var pair in heroStats.ToList())
                {
                    var hero = pair.Key;
                    var stats = pair.Value;

                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label($"<b>{hero.HeroName}</b> <color=#AAAAAA>({hero.HeroClass})</color>\n<size=10>HP:{stats.health} | ATK:{stats.attackPower} | DEF:{stats.defense} | CRIT:{stats.critChance:P0}</size>", GUILayout.Width(320));

                    if (GUILayout.Button("+15 ATK", GUILayout.Width(65), GUILayout.Height(26)))
                    {
                        stats.attackPower += 15;
                        heroStats[hero] = stats;
                    }
                    if (GUILayout.Button("-50 HP", GUILayout.Width(60), GUILayout.Height(26)))
                    {
                        stats.health = Mathf.Max(0, stats.health - 50);
                        heroStats[hero] = stats;
                    }

                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(26)))
                    {
                        toRemove = hero;
                    }
                    GUI.backgroundColor = Color.white;

                    GUILayout.EndHorizontal();
                }

                if (toRemove != null)
                {
                    heroStats.Remove(toRemove);
                }
            }
        }

        // ------------------------------------------------------------------ //
        //  Tab 1: Inventory (ObservableDictionary<string, int>)
        // ------------------------------------------------------------------ //

        private void DrawInventoryTab()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>1. Add / Update Inventory Item (String -> Int)</b>");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Item Name:", GUILayout.Width(75));
            _newItemName = GUILayout.TextField(_newItemName, GUILayout.Width(160));
            GUILayout.Label("Quantity:", GUILayout.Width(60));
            int.TryParse(GUILayout.TextField(_newItemQty.ToString(), GUILayout.Width(60)), out _newItemQty);

            if (GUILayout.Button("TryAdd (Unique)", GUILayout.Height(24)))
            {
                if (inventory.TryAdd(_newItemName, _newItemQty))
                {
                    LogAction($"TryAdd succeeded: '<b>{_newItemName}</b>' (Qty: {_newItemQty})", "<color=#55FF55>[SUCCESS]</color>");
                }
                else
                {
                    LogAction($"TryAdd failed! '<b>{_newItemName}</b>' already in inventory (Qty: {inventory[_newItemName]})!", "<color=#FFAA33>[DUPLICATE REJECTED]</color>");
                }
            }
            if (GUILayout.Button("Set / Overwrite", GUILayout.Height(24)))
            {
                inventory[_newItemName] = _newItemQty;
            }
            if (GUILayout.Button("+ Roll Item", GUILayout.Height(24)))
            {
                string[] items = { "Dragon Scale", "Phoenix Feather", "Ruby Gem", "Elixir of Mana", "Thunder Bow", "Shadow Cloak" };
                string name = items[Random.Range(0, items.Length)];
                inventory.AddOrUpdate(name, _ => Random.Range(1, 10), (_, oldVal) => oldVal + Random.Range(1, 5));
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // Live Table
            GUILayout.BeginHorizontal();
            int totalQty = inventory.SumValues();
            GUILayout.Label($"<b>Inventory Items ({inventory.Count} items, Total Quantity: {totalQty}):</b>");
            if (GUILayout.Button("Clear All", GUILayout.Width(75), GUILayout.Height(20)))
            {
                inventory.Clear();
            }
            if (GUILayout.Button("Reset Defaults", GUILayout.Width(100), GUILayout.Height(20)))
            {
                inventory.Clear();
                inventory.Add("Health Potion", 15);
                inventory.Add("Mana Potion", 8);
                inventory.Add("Iron Sword", 1);
                inventory.Add("Gold Coins", 250);
                inventory.Add("Magic Scroll", 3);
            }
            GUILayout.EndHorizontal();

            if (inventory.Count == 0)
            {
                GUILayout.Label("<color=#888888><i>Inventory is empty. Add some items above!</i></color>");
            }
            else
            {
                string toRemove = null;
                foreach (var pair in inventory.ToList())
                {
                    string item = pair.Key;
                    int qty = pair.Value;

                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label($"<b>{item}</b>", GUILayout.Width(220));
                    GUILayout.Label($"Qty: <b>{qty}</b>", GUILayout.Width(100));

                    if (GUILayout.Button("+1", GUILayout.Width(45), GUILayout.Height(24)))
                    {
                        inventory[item] = qty + 1;
                    }
                    if (GUILayout.Button("-1", GUILayout.Width(45), GUILayout.Height(24)))
                    {
                        if (qty > 1) inventory[item] = qty - 1;
                        else toRemove = item;
                    }

                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(24)))
                    {
                        toRemove = item;
                    }
                    GUI.backgroundColor = Color.white;

                    GUILayout.EndHorizontal();
                }

                if (toRemove != null)
                {
                    inventory.Remove(toRemove);
                }
            }
        }

        // ------------------------------------------------------------------ //
        //  Tab 2: Hero Roster (ObservableHashSet<CharacterProfile>)
        // ------------------------------------------------------------------ //

        private void DrawHeroRosterTab()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>1. Register Hero into Unique Set (ObservableHashSet)</b>");
            GUILayout.Label("<color=#CCCCCC><size=10>HashSets automatically reject duplicate elements via IEquatable value equality.</size></color>");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(45));
            _rosterHeroName = GUILayout.TextField(_rosterHeroName, GUILayout.Width(120));
            GUILayout.Label("Class:", GUILayout.Width(45));
            _rosterHeroClass = GUILayout.TextField(_rosterHeroClass, GUILayout.Width(120));

            if (GUILayout.Button("Add to Set (TryAdd)", GUILayout.Height(24)))
            {
                var hero = new CharacterProfile(_rosterHeroName, _rosterHeroClass);
                if (registeredHeroes.Add(hero))
                {
                    LogAction($"Successfully registered <b>{hero}</b> in HashSet", "<color=#55FF55>[SET SUCCESS]</color>");
                }
                else
                {
                    LogAction($"HashSet rejected duplicate: <b>{hero}</b> is already registered in the set!", "<color=#FFAA33>[DUPLICATE REJECTED]</color>");
                }
            }

            if (GUILayout.Button("+ Roll Hero", GUILayout.Height(24)))
            {
                string[] names = { "Arthur", "Merlin", "Robin", "Lancelot", "Gawain", "Morgana" };
                string[] classes = { "Paladin", "Mage", "Ranger", "Knight", "Berserker", "Witch" };
                var hero = new CharacterProfile(names[Random.Range(0, names.Length)], classes[Random.Range(0, classes.Length)]);
                if (registeredHeroes.Add(hero))
                {
                    LogAction($"Registered random hero: <b>{hero}</b>", "<color=#55FF55>[SET SUCCESS]</color>");
                }
                else
                {
                    LogAction($"Random roll <b>{hero}</b> was a duplicate and was rejected by HashSet!", "<color=#FFAA33>[DUPLICATE REJECTED]</color>");
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // Live Members Table
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Registered Set Members ({registeredHeroes.Count} unique heroes):</b>");
            if (GUILayout.Button("Clear All", GUILayout.Width(75), GUILayout.Height(20)))
            {
                registeredHeroes.Clear();
            }
            if (GUILayout.Button("Reset Defaults", GUILayout.Width(100), GUILayout.Height(20)))
            {
                registeredHeroes.Clear();
                registeredHeroes.Add(new CharacterProfile("Arthur", "Paladin"));
                registeredHeroes.Add(new CharacterProfile("Merlin", "Mage"));
                registeredHeroes.Add(new CharacterProfile("Robin", "Ranger"));
                registeredHeroes.Add(new CharacterProfile("Galahad", "Knight"));
            }
            GUILayout.EndHorizontal();

            if (registeredHeroes.Count == 0)
            {
                GUILayout.Label("<color=#888888><i>Set is empty. Register heroes above!</i></color>");
            }
            else
            {
                CharacterProfile toRemove = null;
                foreach (var hero in registeredHeroes.ToList())
                {
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label($"🛡️ <b>{hero.HeroName}</b> <color=#AAAAAA>({hero.HeroClass})</color>", GUILayout.Width(380));

                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("Remove from Set", GUILayout.Width(130), GUILayout.Height(24)))
                    {
                        toRemove = hero;
                    }
                    GUI.backgroundColor = Color.white;

                    GUILayout.EndHorizontal();
                }

                if (toRemove != null)
                {
                    registeredHeroes.Remove(toRemove);
                }
            }
        }

        // ------------------------------------------------------------------ //
        //  Tab 3: Ordered Skills (SerializableOrderedDictionary)
        // ------------------------------------------------------------------ //

        private void DrawOrderedSkillsTab()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>1. Add Skill (SerializableOrderedDictionary)</b>");
            GUILayout.Label("<color=#CCCCCC><size=10>Tracks deterministic insertion order. Test Reordering (MoveToFront / MoveToBack) and Index Access.</size></color>");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Hero:", GUILayout.Width(40));
            _skillHeroName = GUILayout.TextField(_skillHeroName, GUILayout.Width(90));
            GUILayout.Label("Class:", GUILayout.Width(45));
            _skillHeroClass = GUILayout.TextField(_skillHeroClass, GUILayout.Width(90));
            GUILayout.Label("Skill:", GUILayout.Width(40));
            _skillName = GUILayout.TextField(_skillName, GUILayout.Width(100));
            GUILayout.Label("Mana:", GUILayout.Width(40));
            int.TryParse(GUILayout.TextField(_skillMana.ToString(), GUILayout.Width(40)), out _skillMana);

            if (GUILayout.Button("Add to End", GUILayout.Height(24)))
            {
                var hero = new CharacterProfile(_skillHeroName, _skillHeroClass);
                var skill = new SkillData(_skillName, _skillMana, 5f);
                if (activeSkills.TryAdd(hero, skill))
                {
                    LogAction($"Added ordered skill: <b>{hero.HeroName}</b> -> {skill.skillName}", "<color=#55FF55>[ORDERED ADD]</color>");
                }
                else
                {
                    LogAction($"Hero <b>{hero.HeroName}</b> already has a skill! Overwriting...", "<color=#FFFF55>[ORDERED UPDATE]</color>");
                    activeSkills[hero] = skill;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // Live Table in Insertion Order
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Active Skills in Insertion Order ({activeSkills.Count} entries):</b>");
            if (GUILayout.Button("Clear All", GUILayout.Width(75), GUILayout.Height(20)))
            {
                activeSkills.Clear();
            }
            if (GUILayout.Button("Reset Defaults", GUILayout.Width(100), GUILayout.Height(20)))
            {
                activeSkills.Clear();
                activeSkills.Add(new CharacterProfile("Arthur", "Paladin"), new SkillData("Holy Shield", 25, 12f));
                activeSkills.Add(new CharacterProfile("Merlin", "Mage"), new SkillData("Meteor Strike", 60, 20f));
                activeSkills.Add(new CharacterProfile("Robin", "Ranger"), new SkillData("Rain of Arrows", 35, 8f));
            }
            GUILayout.EndHorizontal();

            if (activeSkills.Count == 0)
            {
                GUILayout.Label("<color=#888888><i>No skills registered in ordered dictionary.</i></color>");
            }
            else
            {
                CharacterProfile toRemove = null;
                for (int i = 0; i < activeSkills.Count; i++)
                {
                    var pair = activeSkills.GetAt(i);
                    var hero = pair.Key;
                    var skill = pair.Value;

                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label($"<b>#{i}</b> <b>{hero.HeroName}</b> <color=#AAAAAA>({hero.HeroClass})</color> => <i>{skill.skillName}</i> (Mana: {skill.manaCost})", GUILayout.Width(340));

                    if (GUILayout.Button("↑ Front", GUILayout.Width(60), GUILayout.Height(24)))
                    {
                        activeSkills.MoveToFront(hero);
                        LogAction($"Moved <b>{hero.HeroName}</b> to Front (#0)", "<color=#55FFFF>[REORDER]</color>");
                        break;
                    }
                    if (GUILayout.Button("↓ Back", GUILayout.Width(60), GUILayout.Height(24)))
                    {
                        activeSkills.MoveToBack(hero);
                        LogAction($"Moved <b>{hero.HeroName}</b> to Back (#{activeSkills.Count - 1})", "<color=#55FFFF>[REORDER]</color>");
                        break;
                    }

                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("Remove", GUILayout.Width(65), GUILayout.Height(24)))
                    {
                        toRemove = hero;
                    }
                    GUI.backgroundColor = Color.white;

                    GUILayout.EndHorizontal();
                }

                if (toRemove != null)
                {
                    activeSkills.TryRemove(toRemove);
                }
            }
        }

        // ------------------------------------------------------------------ //
        //  Tab 4: Stack & Queue (LIFO / FIFO)
        // ------------------------------------------------------------------ //

        private void DrawStackQueueTab()
        {
            GUILayout.BeginHorizontal();

            // ── Left: Stack (LIFO) ──
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(Screen.width > 700 ? 330 : 280));
            GUILayout.Label("<b>🥞 LIFO Stack (SerializableStack)</b>");
            GUILayout.Label($"Current items: <b>{statHistory.Count}</b>");

            GUILayout.BeginHorizontal();
            GUILayout.Label("HP:", GUILayout.Width(25));
            int.TryParse(GUILayout.TextField(_stackHp.ToString(), GUILayout.Width(45)), out _stackHp);
            GUILayout.Label("ATK:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(_stackAtk.ToString(), GUILayout.Width(45)), out _stackAtk);
            if (GUILayout.Button("Push", GUILayout.Height(22)))
            {
                statHistory.Push(new CombatStats(_stackHp, _stackAtk, 40, 0.15f));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pop (Top)", GUILayout.Height(24)))
            {
                if (statHistory.TryPop(out var popped))
                {
                    LogAction($"Popped from Stack: {popped}. Left: {statHistory.Count}", "<color=#FF5555>[STACK POP]</color>");
                }
                else
                {
                    LogAction("Stack is empty!", "<color=#FFAA33>[EMPTY]</color>");
                }
            }
            if (GUILayout.Button("Peek", GUILayout.Height(24)))
            {
                if (statHistory.TryPeek(out var top))
                {
                    LogAction($"Peek Stack Top: {top}", "<color=#55FFFF>[STACK PEEK]</color>");
                }
            }
            if (GUILayout.Button("Clear", GUILayout.Height(24)))
            {
                statHistory.Clear();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("<b>Stack Items (Top to Bottom):</b>");
            int sIdx = 0;
            foreach (var item in statHistory)
            {
                GUILayout.Label($"  [#{sIdx++}] HP:{item.health}, ATK:{item.attackPower}");
                if (sIdx >= 6) { GUILayout.Label($"  ... +{statHistory.Count - 6} more"); break; }
            }
            GUILayout.EndVertical();

            // ── Right: Queue (FIFO) ──
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(Screen.width > 700 ? 330 : 280));
            GUILayout.Label("<b>🚶 FIFO Queue (SerializableQueue)</b>");
            GUILayout.Label($"Current in queue: <b>{matchmakingQueue.Count}</b>");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Hero:", GUILayout.Width(35));
            _queueHeroName = GUILayout.TextField(_queueHeroName, GUILayout.Width(80));
            GUILayout.Label("Class:", GUILayout.Width(40));
            _queueHeroClass = GUILayout.TextField(_queueHeroClass, GUILayout.Width(70));
            if (GUILayout.Button("Enqueue", GUILayout.Height(22)))
            {
                matchmakingQueue.Enqueue(new CharacterProfile(_queueHeroName, _queueHeroClass));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Dequeue (Front)", GUILayout.Height(24)))
            {
                if (matchmakingQueue.TryDequeue(out var dequeued))
                {
                    LogAction($"Dequeued from Queue: <b>{dequeued}</b>. Left: {matchmakingQueue.Count}", "<color=#FF5555>[QUEUE DEQUEUE]</color>");
                }
                else
                {
                    LogAction("Queue is empty!", "<color=#FFAA33>[EMPTY]</color>");
                }
            }
            if (GUILayout.Button("Peek", GUILayout.Height(24)))
            {
                if (matchmakingQueue.TryPeek(out var front))
                {
                    LogAction($"Peek Queue Front: <b>{front}</b>", "<color=#55FFFF>[QUEUE PEEK]</color>");
                }
            }
            if (GUILayout.Button("Clear", GUILayout.Height(24)))
            {
                matchmakingQueue.Clear();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("<b>Queue Items (Front to Back):</b>");
            int qIdx = 0;
            foreach (var item in matchmakingQueue)
            {
                GUILayout.Label($"  [#{qIdx++}] <b>{item.HeroName}</b> ({item.HeroClass})");
                if (qIdx >= 6) { GUILayout.Label($"  ... +{matchmakingQueue.Count - 6} more"); break; }
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        // ------------------------------------------------------------------ //
        //  Tab 5: Nested & 6-Layer Collections
        // ------------------------------------------------------------------ //

        private void DrawNestedAndSixLayerTab()
        {
            // ── Section A: Nested Waypoints ──
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>1. Nested Class Key & Value (fastTravelWaypoints)</b>");
            GUILayout.Label("<color=#CCCCCC><size=10>Key contains inner Coordinate(X, Y) | Value contains inner Requirement(Lvl, Gold)</size></color>");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Zone:", GUILayout.Width(38));
            _wpZone = GUILayout.TextField(_wpZone, GUILayout.Width(100));
            GUILayout.Label("X:", GUILayout.Width(15));
            int.TryParse(GUILayout.TextField(_wpX.ToString(), GUILayout.Width(35)), out _wpX);
            GUILayout.Label("Y:", GUILayout.Width(15));
            int.TryParse(GUILayout.TextField(_wpY.ToString(), GUILayout.Width(35)), out _wpY);
            GUILayout.Label("Beacon:", GUILayout.Width(50));
            _wpBeacon = GUILayout.TextField(_wpBeacon, GUILayout.Width(90));
            GUILayout.Label("Lvl:", GUILayout.Width(25));
            int.TryParse(GUILayout.TextField(_wpLvl.ToString(), GUILayout.Width(30)), out _wpLvl);
            GUILayout.Label("Gold:", GUILayout.Width(35));
            int.TryParse(GUILayout.TextField(_wpGold.ToString(), GUILayout.Width(35)), out _wpGold);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("TryAdd Waypoint", GUILayout.Height(24)))
            {
                var k = new WaypointNavigation.Key(_wpZone, new WaypointNavigation.Key.Coordinate(_wpX, _wpY));
                var v = new WaypointNavigation.Value(_wpBeacon, new WaypointNavigation.Value.Requirement(_wpLvl, _wpGold));
                if (fastTravelWaypoints.TryAdd(k, v))
                {
                    LogAction($"Added nested waypoint: '{_wpZone}' -> '{_wpBeacon}'", "<color=#55FF55>[WP SUCCESS]</color>");
                }
                else
                {
                    LogAction($"Waypoint with exact zone & coordinate already exists!", "<color=#FFAA33>[DUPLICATE REJECTED]</color>");
                }
            }

            if (GUILayout.Button("Test Value-Equality Lookup", GUILayout.Height(24)))
            {
                var queryKey = new WaypointNavigation.Key("Dragon Peak", new WaypointNavigation.Key.Coordinate(750, 920));
                if (fastTravelWaypoints.TryGetValue(queryKey, out var val))
                {
                    LogAction($"Match Found for '{queryKey}': Beacon = '{val.BeaconName}' ({val.Req})", "<color=#55FF55>[EQUALITY MATCH]</color>");
                }
                else
                {
                    LogAction($"Lookup for '{queryKey}' not found.", "<color=#FF5555>[NOT FOUND]</color>");
                }
            }
            GUILayout.EndHorizontal();

            // List waypoints with Remove button
            WaypointNavigation.Key toRemoveWp = null;
            foreach (var pair in fastTravelWaypoints.ToList())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"📍 <b>{pair.Key.ZoneName}</b> ({pair.Key.Coord.X}, {pair.Key.Coord.Y}) => '{pair.Value.BeaconName}' (Lvl {pair.Value.Req.MinLevel}, {pair.Value.Req.GoldCost}g)", GUILayout.Width(460));

                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(20)))
                {
                    toRemoveWp = pair.Key;
                }
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
            }
            if (toRemoveWp != null) fastTravelWaypoints.Remove(toRemoveWp);

            GUILayout.EndVertical();

            GUILayout.Space(6);

            // ── Section B: 6-Layer Deep Galactic Key ──
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>2. 6-Layer Deep Nested Key (GalacticRouteKey -> SpaceStationInfo)</b>");
            GUILayout.Label("<color=#CCCCCC><size=10>Tests 6 full levels of nested classes: Galaxy -> Quadrant -> StarSystem -> Orbit -> Sector -> SubGrid.</size></color>");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Test 6-Layer Key Match (O(1) Lookup)", GUILayout.Height(26)))
            {
                var deepKey = new GalacticRouteKey(
                    "MilkyWay",
                    new QuadrantData(
                        "Alpha-7",
                        new StarSystemData(
                            104,
                            new PlanetOrbitData(
                                "Sol-3",
                                new SurfaceSectorData(
                                    "Sector-A",
                                    new SubGridCoordinate(42, 88)
                                )
                            )
                        )
                    )
                );

                if (galacticStations.TryGetValue(deepKey, out var station))
                {
                    LogAction($"6-Layer Key Match: Found station '{station.StationName}' (DEF: {station.DefenseRating})!", "<color=#55FF55>[6-LAYER MATCH]</color>");
                }
                else
                {
                    LogAction("6-layer key not found!", "<color=#FF5555>[6-LAYER MISS]</color>");
                }
            }

            if (GUILayout.Button("Test 6-Layer Mismatch Rejection", GUILayout.Height(26)))
            {
                // Has SubGrid (42, 89) instead of (42, 88)
                var mismatchKey = new GalacticRouteKey(
                    "MilkyWay",
                    new QuadrantData(
                        "Alpha-7",
                        new StarSystemData(
                            104,
                            new PlanetOrbitData(
                                "Sol-3",
                                new SurfaceSectorData(
                                    "Sector-A",
                                    new SubGridCoordinate(42, 89)
                                )
                            )
                        )
                    )
                );

                if (galacticStations.TryGetValue(mismatchKey, out _))
                {
                    LogAction("ERROR: Mismatch key should NOT have matched!", "<color=#FF5555>[FAIL]</color>");
                }
                else
                {
                    LogAction("Correct: Mismatch key (Y:89 vs Y:88) rejected by 6-layer hash comparison!", "<color=#55FFFF>[REJECTED CORRECTLY]</color>");
                }
            }
            GUILayout.EndHorizontal();

            // List 6-layer stations with remove button
            GalacticRouteKey toRemoveGal = null;
            foreach (var pair in galacticStations.ToList())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"🚀 <b>{pair.Key.GalaxyName}</b> • {pair.Key.Quadrant.QuadrantCode} • System #{pair.Key.Quadrant.StarSystem.SystemCode} => '<b>{pair.Value.StationName}</b>' (DEF:{pair.Value.DefenseRating})", GUILayout.Width(460));

                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(20)))
                {
                    toRemoveGal = pair.Key;
                }
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
            }
            if (toRemoveGal != null) galacticStations.Remove(toRemoveGal);

            GUILayout.EndVertical();
        }
    }
}
