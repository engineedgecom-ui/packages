using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EngineEdge.SmartDictionary;

namespace EngineEdge.SmartDictionary.Example
{
    /// <summary>
    /// Demonstrates <see cref="SerializableDictionary{TKey, TValue}"/> and
    /// <see cref="ObservableDictionary{TKey, TValue}"/> across primitive, custom class,
    /// and custom struct types, including reactive events, LINQ extensions, and safe access APIs.
    /// </summary>
    [AddComponentMenu("EngineEdge/SmartDictionary/Examples/Dictionary Example")]
    public class DictionaryExample : MonoBehaviour
    {
        [Header("1. Simple Primitive Dictionary")]
        [Tooltip("Standard serializable dictionary mapping item names to quantities.")]
        public SerializableDictionary<string, int> inventory = new SerializableDictionary<string, int>
        {
            { "Health Potion", 15 },
            { "Mana Potion", 8 },
            { "Iron Sword", 1 },
            { "Gold Coins", 250 },
            { "Magic Scroll", 3 }
        };

        [Header("2. Floating-Point Attributes")]
        [Tooltip("Player attribute modifiers.")]
        public SerializableDictionary<string, float> characterStats = new SerializableDictionary<string, float>
        {
            { "MovementSpeed", 6.5f },
            { "AttackDamage", 42.0f },
            { "CriticalChance", 0.25f },
            { "Defense", 18.0f }
        };

        [Header("3. Class Key -> Struct Value (Lightweight)")]
        [Tooltip("Key is a custom C# class (ItemCategoryKey), Value is a custom struct (ItemModifier).")]
        public SerializableDictionary<ItemCategoryKey, ItemModifier> categoryModifiers = new SerializableDictionary<ItemCategoryKey, ItemModifier>
        {
            { new ItemCategoryKey("Heavy Armor", 3), new ItemModifier(0f, 0.85f, 500) },
            { new ItemCategoryKey("Daggers", 2),     new ItemModifier(25f, 1.25f, 150) },
            { new ItemCategoryKey("Staves", 4),      new ItemModifier(45f, 1.05f, 200) }
        };

        [Header("4. Reactive Observable Dictionary (Events & UnityEvents)")]
        [Tooltip("Key is a custom class (CharacterProfile), Value is a struct (CombatStats). Dispatches C# Action events & Inspector UnityEvents on mutation.")]
        public ObservableDictionary<CharacterProfile, CombatStats> heroStats = new ObservableDictionary<CharacterProfile, CombatStats>
        {
            { new CharacterProfile("Arthur", "Paladin"),  new CombatStats(1200, 85, 95, 0.15f) },
            { new CharacterProfile("Merlin", "Mage"),     new CombatStats(650, 140, 30, 0.35f) },
            { new CharacterProfile("Robin", "Ranger"),    new CombatStats(800, 110, 50, 0.45f) }
        };

        [Header("Runtime GUI Overlay")]
        [SerializeField] private bool showOnScreenGui = true;

        private readonly List<string> _actionLogs = new List<string>();
        private Vector2 _scrollPos;
        private string _newHeroName = "Galahad";
        private string _newHeroClass = "Knight";
        private int _newHeroHp = 950;
        private int _newHeroAtk = 80;

        private void OnEnable()
        {
            heroStats.OnEntryAdded += (hero, stats) => Log($"[Added] Hero: {hero} (HP:{stats.health}, ATK:{stats.attackPower})");
            heroStats.OnEntryUpdated += (hero, oldStats, newStats) => Log($"[Updated] Hero: {hero} ATK: {oldStats.attackPower} -> {newStats.attackPower}");
            heroStats.OnEntryRemoved += (hero, stats) => Log($"[Removed] Hero: {hero}");
            heroStats.OnCleared += () => Log("[Cleared] Hero stats dictionary cleared!");
            heroStats.OnCountChanged += c => Log($"[Count] Hero count: {c}");
        }

        private void Awake()
        {
            Debug.Log(JsonUtility.ToJson(new DictionaryJsonContainer { inventory = this.inventory, heroStats = this.heroStats }, true));
        }

        private void Start()
        {
            // Safe access demonstration
            int potions = inventory.GetOrDefault("Health Potion", 0);

            // LINQ demonstrations
            var highDamageStats = characterStats.WhereDict((k, v) => v > 20f);
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
            GUILayout.Label("<b><size=14>📖 Dictionary Example</size></b>");
            GUILayout.Label("<color=#CCCCCC><size=10>Serializable & Observable Dictionaries (Class/Struct)</size></color>");
            GUILayout.Space(6);

            GUILayout.Label($"<b>Heroes in ObservableDictionary ({heroStats.Count}):</b>");
            GUILayout.BeginHorizontal();
            _newHeroName = GUILayout.TextField(_newHeroName, GUILayout.Width(100));
            _newHeroClass = GUILayout.TextField(_newHeroClass, GUILayout.Width(80));
            int.TryParse(GUILayout.TextField(_newHeroHp.ToString(), GUILayout.Width(50)), out _newHeroHp);
            int.TryParse(GUILayout.TextField(_newHeroAtk.ToString(), GUILayout.Width(50)), out _newHeroAtk);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("TryAdd (Unique)"))
            {
                var key = new CharacterProfile(_newHeroName, _newHeroClass);
                var val = new CombatStats(_newHeroHp, _newHeroAtk, 60, 0.20f);
                if (!heroStats.TryAdd(key, val))
                    Log($"TryAdd rejected duplicate key: {key}");
            }
            if (GUILayout.Button("Add or Overwrite"))
            {
                var key = new CharacterProfile(_newHeroName, _newHeroClass);
                var val = new CombatStats(_newHeroHp, _newHeroAtk, 60, 0.20f);
                heroStats[key] = val;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(180));
            CharacterProfile toRemove = null;
            foreach (var pair in heroStats.ToList())
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"<b>{pair.Key}</b>\n<size=10>HP:{pair.Value.health} | ATK:{pair.Value.attackPower}</size>", GUILayout.Width(240));
                if (GUILayout.Button("+10 ATK", GUILayout.Width(65)))
                {
                    var s = pair.Value;
                    s.attackPower += 10;
                    heroStats[pair.Key] = s;
                }
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    toRemove = pair.Key;
                }
                GUILayout.EndHorizontal();
            }
            if (toRemove != null) heroStats.Remove(toRemove);
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
        private class DictionaryJsonContainer
        {
            public SerializableDictionary<string, int> inventory;
            public ObservableDictionary<CharacterProfile, CombatStats> heroStats;
        }

        /// <summary>
        /// Serializes dictionaries to JSON using JsonUtility and outputs formatted results to Debug.Log.
        /// </summary>
        [ContextMenu("Log JSON to Console (JsonUtility)")]
        public void LogJsonToConsole()
        {
            var container = new DictionaryJsonContainer
            {
                inventory = this.inventory,
                heroStats = this.heroStats
            };

            string json = JsonUtility.ToJson(container, prettyPrint: true);
            Debug.Log(json);
        }
    }
}
