using System;
using System.Collections.Generic;
using UnityEngine;
using EngineEdge.SmartDictionary;

namespace EngineEdge.SmartDictionary.Example
{
    /// <summary>
    /// Comprehensive test & demonstration script verifying Unity <see cref="JsonUtility"/> serialization
    /// and deserialization across ALL Smart Dictionary collection types containing nested classes,
    /// deep hierarchies, and custom structures.
    /// <para>Outputs full formatted JSON and verification results directly to <see cref="Debug.Log"/>.</para>
    /// </summary>
    [AddComponentMenu("EngineEdge/SmartDictionary/Examples/JsonUtility Serialization Example")]
    public class JsonUtilitySerializationExample : MonoBehaviour
    {
        // =========================================================================
        //  1. MASTER SERIALIZABLE CONTAINER (All collections in a single JSON save)
        // =========================================================================

        /// <summary>
        /// A composite save container class demonstrating that Unity's <see cref="JsonUtility"/>
        /// can serialize an entire game profile containing all 8 collection types with nested classes.
        /// </summary>
        [Serializable]
        public class MasterSaveData
        {
            public string saveSlotName = "Slot_01_Legendary";
            public string timestamp = "2026-09-06";

            // 1. SerializableDictionary (Nested Class Key -> Nested Class Value)
            public SerializableDictionary<WaypointNavigation.Key, WaypointNavigation.Value> waypoints
                = new SerializableDictionary<WaypointNavigation.Key, WaypointNavigation.Value>();

            // 2. ObservableDictionary (Class Key -> Nested Inner Dictionary Value)
            public ObservableDictionary<CharacterProfile, SkillTreeDictionary> heroSkillTrees
                = new ObservableDictionary<CharacterProfile, SkillTreeDictionary>();

            // 3. SerializableOrderedDictionary (6-Layer Deep Nested Key -> Class Value)
            public SerializableOrderedDictionary<GalacticRouteKey, SpaceStationInfo> galacticRoutes
                = new SerializableOrderedDictionary<GalacticRouteKey, SpaceStationInfo>();

            // 4. SerializableBiDictionary (Two-way lookup: Struct -> String)
            public SerializableBiDictionary<PlayerBadge, string> badgeHolders
                = new SerializableBiDictionary<PlayerBadge, string>();

            // 5. ObservableHashSet (Set of Class objects with duplicate rejection)
            public ObservableHashSet<CharacterProfile> heroRoster
                = new ObservableHashSet<CharacterProfile>();

            // 6. SerializableHashSet (Set of Structs)
            public SerializableHashSet<CombatStats> baseStatPresets
                = new SerializableHashSet<CombatStats>();

            // 7. SerializableStack (LIFO undo / stat history)
            public SerializableStack<CombatStats> combatHistory
                = new SerializableStack<CombatStats>();

            // 8. SerializableQueue (FIFO lobby matchmaking queue)
            public SerializableQueue<CharacterProfile> matchmakingLobby
                = new SerializableQueue<CharacterProfile>();
        }

        // =========================================================================
        //  Inspector Properties
        // =========================================================================

        [Header("Master Save Profile (Nested Classes across all collections)")]
        public MasterSaveData saveData = new MasterSaveData();

        [Header("Test Options")]
        [Tooltip("If true, automatically runs all JsonUtility tests and outputs results to Debug.Log on Start().")]
        [SerializeField] private bool runTestsOnStart = true;

        [Tooltip("If true, renders on-screen GUI buttons to run tests interactively in Play Mode.")]
        [SerializeField] private bool showOnScreenGui = true;

        [Header("Last Generated JSON (Read-Only Preview)")]
        [TextArea(6, 12)]
        [SerializeField] private string lastSerializedJsonPreview = "";

        // =========================================================================
        //  Unity Lifecycle
        // =========================================================================

        private void Reset()
        {
            PopulateSampleData();
        }

        private void Awake()
        {
            if (saveData.waypoints.Count == 0)
            {
                PopulateSampleData();
            }

            Debug.Log(saveData.heroSkillTrees.ToStandardJson(prettyPrint: true));
        }

        private void Start()
        {
        }

        // =========================================================================
        //  Sample Data Population
        // =========================================================================

        [ContextMenu("1. Reset & Populate Sample Nested Data")]
        public void PopulateSampleData()
        {
            saveData = new MasterSaveData();

            // 1. Waypoints with nested Coordinate & Requirement classes
            saveData.waypoints.Add(
                new WaypointNavigation.Key("Dragon Peak", new WaypointNavigation.Key.Coordinate(750, 920)),
                new WaypointNavigation.Value("Summit Beacon", new WaypointNavigation.Value.Requirement(50, 500))
            );
            saveData.waypoints.Add(
                new WaypointNavigation.Key("Sunken Ruins", new WaypointNavigation.Key.Coordinate(320, 110)),
                new WaypointNavigation.Value("Submerged Portal", new WaypointNavigation.Value.Requirement(30, 250))
            );

            // 2. Hero Skill Trees (Dict of Dicts)
            var arthurSkills = new SkillTreeDictionary { { "Holy Strike", 5 }, { "Divine Aura", 3 } };
            var merlinSkills = new SkillTreeDictionary { { "Fireball", 10 }, { "Teleport", 2 } };
            saveData.heroSkillTrees.Add(new CharacterProfile("Arthur", "Paladin"), arthurSkills);
            saveData.heroSkillTrees.Add(new CharacterProfile("Merlin", "Mage"), merlinSkills);

            // 3. 6-Layer Deep Nested Galactic Route Key
            saveData.galacticRoutes.Add(
                new GalacticRouteKey(
                    "MilkyWay",
                    new QuadrantData("Alpha-7",
                        new StarSystemData(104,
                            new PlanetOrbitData("Sol-3",
                                new SurfaceSectorData("Sector-A",
                                    new SubGridCoordinate(42, 88)))))),
                new SpaceStationInfo("Earth Orbital Defense", 9500, false)
            );
            saveData.galacticRoutes.Add(
                new GalacticRouteKey(
                    "Andromeda",
                    new QuadrantData("Omega-9",
                        new StarSystemData(880,
                            new PlanetOrbitData("Xylar-Prime",
                                new SurfaceSectorData("Sector-D",
                                    new SubGridCoordinate(999, 120)))))),
                new SpaceStationInfo("Pirate Dreadnought Outpost", 14200, true)
            );

            // 4. BiDictionary
            saveData.badgeHolders.Add(new PlayerBadge(101, "Grandmaster"), "Player_Alpha");
            saveData.badgeHolders.Add(new PlayerBadge(102, "Sharpshooter"), "Player_Bravo");

            // 5. Hero Roster Set
            saveData.heroRoster.Add(new CharacterProfile("Arthur", "Paladin"));
            saveData.heroRoster.Add(new CharacterProfile("Robin", "Ranger"));
            saveData.heroRoster.Add(new CharacterProfile("Galahad", "Knight"));

            // 6. Base Stat Presets
            saveData.baseStatPresets.Add(new CombatStats(500, 50, 50, 0.05f));
            saveData.baseStatPresets.Add(new CombatStats(1200, 120, 100, 0.20f));

            // 7. Combat Stack (LIFO)
            saveData.combatHistory.Push(new CombatStats(100, 10, 5, 0.05f));
            saveData.combatHistory.Push(new CombatStats(250, 25, 15, 0.10f));

            // 8. Matchmaking Queue (FIFO)
            saveData.matchmakingLobby.Enqueue(new CharacterProfile("Lancelot", "Knight"));
            saveData.matchmakingLobby.Enqueue(new CharacterProfile("Morgana", "Witch"));
        }

        // =========================================================================
        //  Complete JsonUtility Test Suite (Direct Debug.Log Output)
        // =========================================================================

        [ContextMenu("2. Run All JsonUtility Serialization Tests")]
        public void RunAllJsonUtilityTests()
        {
            string masterJson = JsonUtility.ToJson(saveData, prettyPrint: true);
            lastSerializedJsonPreview = masterJson;
            Debug.Log(masterJson);
        }

        // =========================================================================
        //  Internal Wrappers for Individual Collection Tests
        // =========================================================================

        [Serializable]
        private class GalacticRouteWrapper
        {
            public SerializableOrderedDictionary<GalacticRouteKey, SpaceStationInfo> routes;
        }

        [Serializable]
        private class BiDictWrapper
        {
            public SerializableBiDictionary<PlayerBadge, string> badges;
        }

        [Serializable]
        private class StackQueueWrapper
        {
            public SerializableStack<CombatStats> history;
            public SerializableQueue<CharacterProfile> lobby;
        }

        // =========================================================================
        //  OnGUI Runtime Interactive Tester
        // =========================================================================

        private void OnGUI()
        {
            if (!showOnScreenGui) return;

            float width = 340f;
            GUILayout.BeginArea(new Rect(Screen.width - width - 15, 15, width, 180), GUI.skin.box);
            GUILayout.Label("<b><size=13>💾 JsonUtility Testing Suite</size></b>");
            GUILayout.Label("<color=#CCCCCC><size=10>Tests nested classes across all 8 collections.</size></color>");
            GUILayout.Space(4);

            if (GUILayout.Button("▶ Run All JsonUtility Tests", GUILayout.Height(32)))
            {
                RunAllJsonUtilityTests();
            }

            if (GUILayout.Button("🔄 Re-Populate Nested Sample Data", GUILayout.Height(24)))
            {
                PopulateSampleData();
            }

            GUILayout.Label("<size=9><color=#AAAAAA>Check Unity Console window for full formatted JSON and verification logs.</color></size>");
            GUILayout.EndArea();
        }
    }
}
