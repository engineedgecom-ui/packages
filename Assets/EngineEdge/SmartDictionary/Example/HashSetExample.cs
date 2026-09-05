using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EngineEdge.SmartDictionary;

namespace EngineEdge.SmartDictionary.Example
{
    /// <summary>
    /// Demonstrates <see cref="SerializableHashSet{T}"/> and <see cref="ObservableHashSet{T}"/>
    /// across primitive, custom struct, and custom class types with automatic duplicate rejection,
    /// reactive mutation events, and LINQ filtering.
    /// </summary>
    [AddComponentMenu("EngineEdge/SmartDictionary/Examples/HashSet Example")]
    public class HashSetExample : MonoBehaviour
    {
        [Header("1. Primitive String HashSet")]
        [Tooltip("Serializable set of unique string identifiers — duplicates are automatically rejected.")]
        public SerializableHashSet<string> unlockedAchievements = new SerializableHashSet<string>
        {
            "First Blood",
            "Treasure Hunter",
            "Master Crafter",
            "Dragon Slayer"
        };

        [Header("2. Struct HashSet")]
        [Tooltip("Serializable set containing custom CombatStats value structs.")]
        public SerializableHashSet<CombatStats> baseStatTemplates = new SerializableHashSet<CombatStats>
        {
            new CombatStats(500, 50, 50, 0.05f),
            new CombatStats(800, 80, 70, 0.10f),
            new CombatStats(1200, 120, 100, 0.20f)
        };

        [Header("3. Reactive Observable HashSet (Classes)")]
        [Tooltip("Reactive set containing custom CharacterProfile class instances with C# Actions and serialized UnityEvents.")]
        public ObservableHashSet<CharacterProfile> registeredHeroes = new ObservableHashSet<CharacterProfile>
        {
            new CharacterProfile("Arthur", "Paladin"),
            new CharacterProfile("Merlin", "Mage"),
            new CharacterProfile("Robin", "Ranger"),
            new CharacterProfile("Galahad", "Knight")
        };

        [Header("Runtime GUI Overlay")]
        [SerializeField] private bool showOnScreenGui = true;

        private readonly List<string> _actionLogs = new List<string>();
        private Vector2 _scrollPos;
        private string _newHeroName = "Percival";
        private string _newHeroClass = "Warrior";
        private string _newAchievement = "Speed Runner";

        private void OnEnable()
        {
            registeredHeroes.OnItemAdded += hero => Log($"[Registered] Hero: {hero}");
            registeredHeroes.OnItemRemoved += hero => Log($"[Unregistered] Hero: {hero}");
            registeredHeroes.OnCleared += () => Log("[Cleared] Hero roster cleared!");
            registeredHeroes.OnCountChanged += c => Log($"[Count] Roster count: {c}");
        }

        private void Awake()
        {
            Debug.Log(JsonUtility.ToJson(new HashSetJsonContainer
            {
                achievements = this.unlockedAchievements,
                baseStats = this.baseStatTemplates,
                roster = this.registeredHeroes
            }, true));
        }

        private void Start()
        {
            // Filter set
            var highHpTemplates = baseStatTemplates.Filter(stats => stats.health >= 800);
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
            GUILayout.Label("<b><size=14>🏷️ HashSet Example</size></b>");
            GUILayout.Label("<color=#CCCCCC><size=10>Serializable & Observable HashSets (Unique Items & Events)</size></color>");
            GUILayout.Space(6);

            // Achievements Quick Test
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"<b>1. Achievements Set ({unlockedAchievements.Count} items):</b>");
            GUILayout.BeginHorizontal();
            _newAchievement = GUILayout.TextField(_newAchievement, GUILayout.Width(200));
            if (GUILayout.Button("Try Add"))
            {
                if (unlockedAchievements.Add(_newAchievement))
                    Log($"Unlocked achievement: '{_newAchievement}'");
                else
                    Log($"Duplicate rejected: '{_newAchievement}' already unlocked!");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // Observable Hero Roster
            GUILayout.Label($"<b>2. Observable Hero Roster ({registeredHeroes.Count} members):</b>");
            GUILayout.BeginHorizontal();
            _newHeroName = GUILayout.TextField(_newHeroName, GUILayout.Width(130));
            _newHeroClass = GUILayout.TextField(_newHeroClass, GUILayout.Width(100));
            if (GUILayout.Button("Register Hero"))
            {
                var hero = new CharacterProfile(_newHeroName, _newHeroClass);
                if (!registeredHeroes.Add(hero))
                    Log($"Duplicate rejected: Hero '{hero}' already registered!");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(160));
            CharacterProfile toRemove = null;
            foreach (var hero in registeredHeroes.ToList())
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"<b>{hero.HeroName}</b> <color=#AAAAAA>({hero.HeroClass})</color>", GUILayout.Width(280));
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    toRemove = hero;
                }
                GUILayout.EndHorizontal();
            }
            if (toRemove != null) registeredHeroes.Remove(toRemove);
            GUILayout.EndScrollView();

            GUILayout.Space(6);
            if (GUILayout.Button("💾 Log JSON to Console (JsonUtility)", GUILayout.Height(28)))
            {
                LogJsonToConsole();
            }

            GUILayout.Space(6);
            GUILayout.Label("<b>Live Mutation Event Log:</b>");
            for (int i = _actionLogs.Count - 1; i >= Mathf.Max(0, _actionLogs.Count - 6); i--)
            {
                GUILayout.Label($"<size=10>{_actionLogs[i]}</size>");
            }
            GUILayout.EndArea();
        }

        [System.Serializable]
        private class HashSetJsonContainer
        {
            public SerializableHashSet<string> achievements;
            public SerializableHashSet<CombatStats> baseStats;
            public ObservableHashSet<CharacterProfile> roster;
        }

        /// <summary>
        /// Serializes hash sets to JSON using JsonUtility and outputs formatted results to Debug.Log.
        /// </summary>
        [ContextMenu("Log JSON to Console (JsonUtility)")]
        public void LogJsonToConsole()
        {
            var container = new HashSetJsonContainer
            {
                achievements = this.unlockedAchievements,
                baseStats = this.baseStatTemplates,
                roster = this.registeredHeroes
            };

            string json = JsonUtility.ToJson(container, prettyPrint: true);
            Debug.Log(json);
        }
    }
}
