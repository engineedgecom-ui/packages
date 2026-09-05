using System;
using UnityEngine;

namespace EngineEdge.SmartDictionary.Example
{
    // =========================================================================
    //  CLASSES (Usable as Keys or Values)
    // =========================================================================

    /// <summary>
    /// Example of a custom C# CLASS used as a Dictionary KEY.
    /// <para>
    /// When using a class as a dictionary key, it is best practice to implement
    /// <see cref="IEquatable{T}"/> and override <see cref="object.GetHashCode"/> and
    /// <see cref="object.Equals(object)"/> so that lookups match by data rather than pointer identity.
    /// </para>
    /// </summary>
    [Serializable]
    public class CharacterProfile : IEquatable<CharacterProfile>
    {
        [SerializeField]
        private string _heroName;

        [SerializeField]
        private string _heroClass;

        public string HeroName
        {
            get => _heroName;
            set => _heroName = value;
        }

        public string HeroClass
        {
            get => _heroClass;
            set => _heroClass = value;
        }

        public CharacterProfile()
        {
            _heroName = "Hero";
            _heroClass = "Warrior";
        }

        public CharacterProfile(string heroName, string heroClass)
        {
            _heroName = heroName;
            _heroClass = heroClass;
        }

        public bool Equals(CharacterProfile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_heroName, other._heroName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(_heroClass, other._heroClass, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CharacterProfile);
        }

        public override int GetHashCode()
        {
            int h1 = _heroName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_heroName) : 0;
            int h2 = _heroClass != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_heroClass) : 0;
            return (h1 * 397) ^ h2;
        }

        public override string ToString() => $"{_heroName} ({_heroClass})";
    }

    /// <summary>
    /// Another custom C# CLASS used as a Key (e.g. category grouping).
    /// </summary>
    [Serializable]
    public class ItemCategoryKey : IEquatable<ItemCategoryKey>
    {
        public string categoryName;
        public int tier;

        public ItemCategoryKey()
        {
            categoryName = "Weapons";
            tier = 1;
        }

        public ItemCategoryKey(string name, int tier)
        {
            this.categoryName = name;
            this.tier = tier;
        }

        public bool Equals(ItemCategoryKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(categoryName, other.categoryName, StringComparison.OrdinalIgnoreCase) &&
                   tier == other.tier;
        }

        public override bool Equals(object obj) => Equals(obj as ItemCategoryKey);

        public override int GetHashCode()
        {
            int h1 = categoryName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(categoryName) : 0;
            return (h1 * 397) ^ tier.GetHashCode();
        }

        public override string ToString() => $"{categoryName} (T{tier})";
    }

    /// <summary>
    /// Example of a custom C# CLASS used as a Dictionary VALUE.
    /// </summary>
    [Serializable]
    public class SkillData
    {
        public string skillName;
        public int manaCost;
        public float cooldown;

        public SkillData()
        {
            skillName = "Slash";
            manaCost = 10;
            cooldown = 2.5f;
        }

        public SkillData(string name, int mana, float cd)
        {
            skillName = name;
            manaCost = mana;
            cooldown = cd;
        }

        public override string ToString() => $"{skillName} (Mana: {manaCost}, CD: {cooldown}s)";
    }

    // =========================================================================
    //  STRUCTS (Usable as Keys or Values)
    // =========================================================================

    /// <summary>
    /// Example of a custom C# STRUCT used as a Dictionary VALUE.
    /// Contains multiple value-type fields displayed in the Inspector.
    /// </summary>
    [Serializable]
    public struct CombatStats : IEquatable<CombatStats>
    {
        public int health;
        public int attackPower;
        public int defense;
        [Range(0f, 1f)]
        public float critChance;

        public CombatStats(int hp, int atk, int def, float crit)
        {
            health = hp;
            attackPower = atk;
            defense = def;
            critChance = crit;
        }

        public bool Equals(CombatStats other)
        {
            return health == other.health &&
                   attackPower == other.attackPower &&
                   defense == other.defense &&
                   Mathf.Approximately(critChance, other.critChance);
        }

        public override bool Equals(object obj) => obj is CombatStats other && Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(health, attackPower, defense, critChance);
        }

        public override string ToString() => $"HP:{health} ATK:{attackPower} DEF:{defense} CRIT:{critChance:P0}";
    }

    /// <summary>
    /// Example of a custom C# STRUCT used as a VALUE modifier.
    /// </summary>
    [Serializable]
    public struct ItemModifier
    {
        public float damageBonus;
        public float speedMultiplier;
        public int durability;

        public ItemModifier(float dmg, float spd, int dur)
        {
            damageBonus = dmg;
            speedMultiplier = spd;
            durability = dur;
        }

        public override string ToString() => $"+{damageBonus} dmg, x{speedMultiplier} spd, {durability} dur";
    }

    /// <summary>
    /// Example of a custom C# STRUCT used as a KEY.
    /// </summary>
    [Serializable]
    public struct PlayerBadge : IEquatable<PlayerBadge>
    {
        public int badgeId;
        public string title;

        public PlayerBadge(int id, string title)
        {
            badgeId = id;
            this.title = title;
        }

        public bool Equals(PlayerBadge other) => badgeId == other.badgeId && string.Equals(title, other.title, StringComparison.OrdinalIgnoreCase);
        public override bool Equals(object obj) => obj is PlayerBadge other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(badgeId, title != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(title) : 0);
        public override string ToString() => $"#{badgeId} {title}";
    }

    // =========================================================================
    //  NESTED CLASSES (C# Inner Classes + Hierarchical Composition)
    // =========================================================================

    /// <summary>
    /// Demonstrates nested C# classes used both as Dictionary Keys and Values.
    /// <list type="bullet">
    ///   <item><description><see cref="WaypointNavigation.Key"/> contains an inner <see cref="WaypointNavigation.Key.Coordinate"/> class.</description></item>
    ///   <item><description><see cref="WaypointNavigation.Value"/> contains an inner <see cref="WaypointNavigation.Value.Requirement"/> class.</description></item>
    /// </list>
    /// </summary>
    public static class WaypointNavigation
    {
        /// <summary>
        /// Nested class Key that itself contains an inner <see cref="Coordinate"/> class.
        /// Implements <see cref="IEquatable{Key}"/> for deep value equality.
        /// </summary>
        [Serializable]
        public class Key : IEquatable<Key>
        {
            [SerializeField]
            private string _zoneName;

            [SerializeField]
            private Coordinate _coord = new Coordinate();

            /// <summary>
            /// Deeply nested coordinate class.
            /// </summary>
            [Serializable]
            public class Coordinate : IEquatable<Coordinate>
            {
                [SerializeField] private int _x;
                [SerializeField] private int _y;

                public int X => _x;
                public int Y => _y;

                public Coordinate() { }
                public Coordinate(int x, int y) { _x = x; _y = y; }

                public bool Equals(Coordinate other)
                {
                    if (ReferenceEquals(null, other)) return false;
                    if (ReferenceEquals(this, other)) return true;
                    return _x == other._x && _y == other._y;
                }

                public override bool Equals(object obj) => obj is Coordinate other && Equals(other);
                public override int GetHashCode() => HashCode.Combine(_x, _y);
                public override string ToString() => $"({_x}, {_y})";
            }

            public string ZoneName => _zoneName;
            public Coordinate Coord => _coord;

            public Key() { }
            public Key(string zoneName, Coordinate coord)
            {
                _zoneName = zoneName;
                _coord = coord ?? new Coordinate();
            }

            public bool Equals(Key other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(_zoneName, other._zoneName, StringComparison.OrdinalIgnoreCase) &&
                       Equals(_coord, other._coord);
            }

            public override bool Equals(object obj) => obj is Key other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(
                _zoneName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_zoneName) : 0,
                _coord != null ? _coord.GetHashCode() : 0
            );

            public override string ToString() => $"{_zoneName} {_coord}";
        }

        /// <summary>
        /// Nested class Value that itself contains an inner <see cref="Requirement"/> class.
        /// </summary>
        [Serializable]
        public class Value
        {
            [SerializeField]
            private string _beaconName;

            [SerializeField]
            private Requirement _requirement = new Requirement();

            /// <summary>
            /// Deeply nested requirement class.
            /// </summary>
            [Serializable]
            public class Requirement
            {
                [SerializeField] private int _minLevel;
                [SerializeField] private int _goldCost;

                public int MinLevel => _minLevel;
                public int GoldCost => _goldCost;

                public Requirement() { }
                public Requirement(int minLevel, int goldCost) { _minLevel = minLevel; _goldCost = goldCost; }
                public override string ToString() => $"Lvl {_minLevel}, {_goldCost}g";
            }

            public string BeaconName => _beaconName;
            public Requirement Req => _requirement;

            public Value() { }
            public Value(string beaconName, Requirement requirement)
            {
                _beaconName = beaconName;
                _requirement = requirement ?? new Requirement();
            }

            public override string ToString() => $"{_beaconName} ({_requirement})";
        }
    }
}
