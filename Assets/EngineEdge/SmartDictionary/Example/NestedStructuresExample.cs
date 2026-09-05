using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EngineEdge.SmartDictionary;

namespace EngineEdge.SmartDictionary.Example
{
    /// <summary>
    /// Demonstrates complex data structures with Smart Dictionary Pro:
    /// 1. Hierarchical nested inner classes (`WaypointNavigation.Key` & `Value`).
    /// 2. Nested Dictionaries (`SerializableDictionary<CharacterProfile, SkillTreeDictionary>`).
    /// 3. Extreme 6-layer deep class composition (`GalacticRouteKey` -> `SpaceStationInfo`).
    /// </summary>
    [AddComponentMenu("EngineEdge/SmartDictionary/Examples/Nested Structures Example")]
    public class NestedStructuresExample : MonoBehaviour
    {
        [Header("1. Nested Class Key & Value (Waypoint Navigation)")]
        [Tooltip("Key is WaypointNavigation.Key (contains Coordinate), Value is WaypointNavigation.Value (contains Requirement).")]
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

        [Header("2. Nested Dictionary (Dictionary of Dictionaries)")]
        [Tooltip("Hero Profile -> SkillTreeDictionary (Skill Name -> Level).")]
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

        [Header("3. Extreme 6-Layer Deep Key Hierarchy")]
        [Tooltip("GalacticRouteKey (6 nested layers) -> SpaceStationInfo")]
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

        [Header("Runtime GUI Overlay")]
        [SerializeField] private bool showOnScreenGui = true;

        private readonly List<string> _actionLogs = new List<string>();
        private Vector2 _scrollPos;

        private void OnEnable()
        {
            fastTravelWaypoints.OnEntryAdded += (k, v) => Log($"[Waypoint Added] Zone: {k.ZoneName} -> Beacon: {v.BeaconName}");
            fastTravelWaypoints.OnEntryRemoved += (k, v) => Log($"[Waypoint Removed] Zone: {k.ZoneName}");
        }

        private void Awake()
        {
            Debug.Log(JsonUtility.ToJson(this.heroSkillTrees, true));
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
            GUILayout.Label("<b><size=14>🌌 Nested Structures Example</size></b>");
            GUILayout.Label("<color=#CCCCCC><size=10>Nested Classes, Dict-of-Dicts, and 6-Layer Deep Keys</size></color>");
            GUILayout.Space(6);

            GUILayout.Label($"<b>1. Fast Travel Waypoints ({fastTravelWaypoints.Count}):</b>");
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(150));
            foreach (var pair in fastTravelWaypoints.ToList())
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"<b>Zone:</b> {pair.Key.ZoneName} at {pair.Key.Coord}");
                GUILayout.Label($"<b>Beacon:</b> {pair.Value.BeaconName} ({pair.Value.Req})");
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

            GUILayout.Space(6);
            GUILayout.Label($"<b>2. Hero Skill Trees ({heroSkillTrees.Count} heroes):</b>");
            foreach (var heroPair in heroSkillTrees)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"<b>Hero:</b> {heroPair.Key.HeroName} ({heroPair.Key.HeroClass})");
                foreach (var skill in heroPair.Value)
                {
                    GUILayout.Label($"  • {skill.Key}: Level {skill.Value}");
                }
                GUILayout.EndVertical();
            }

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
        private class NestedJsonContainer
        {
            public ObservableDictionary<WaypointNavigation.Key, WaypointNavigation.Value> waypoints;
            public SerializableDictionary<CharacterProfile, SkillTreeDictionary> heroSkillTrees;
            public SerializableDictionary<GalacticRouteKey, SpaceStationInfo> galacticStations;
        }

        /// <summary>
        /// Serializes deeply nested structures to JSON using JsonUtility and outputs formatted results to Debug.Log.
        /// </summary>
        [ContextMenu("Log JSON to Console (JsonUtility)")]
        public void LogJsonToConsole()
        {
            var container = new NestedJsonContainer
            {
                waypoints = this.fastTravelWaypoints,
                heroSkillTrees = this.heroSkillTrees,
                galacticStations = this.galacticStations
            };

            string json = JsonUtility.ToJson(container, prettyPrint: true);
            Debug.Log(json);
        }
    }
}
