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

    /// <summary>
    /// A strongly-typed sub-dictionary mapping skill names to level integers.
    /// Used to demonstrate a Nested Dictionary (Dictionary inside a Dictionary).
    /// </summary>
    [Serializable]
    public class SkillTreeDictionary : SerializableDictionary<string, int>
    {
        public SkillTreeDictionary() : base() { }
        public SkillTreeDictionary(System.Collections.Generic.IDictionary<string, int> dict) : base(dict) { }
    }

    // =========================================================================
    //  DEEP 6-LAYER NESTED CLASS HIERARCHY (Galactic Route Key)
    // =========================================================================

    /// <summary>
    /// Layer 1 (Root Key): Galactic Route Key.
    /// Contains Layer 2 (<see cref="QuadrantData"/>).
    /// </summary>
    [Serializable]
    public class GalacticRouteKey : IEquatable<GalacticRouteKey>
    {
        [SerializeField] private string _galaxyName;
        [SerializeField] private QuadrantData _quadrant = new QuadrantData();

        public string GalaxyName => _galaxyName;
        public QuadrantData Quadrant => _quadrant;

        public GalacticRouteKey() { }
        public GalacticRouteKey(string galaxy, QuadrantData quadrant)
        {
            _galaxyName = galaxy;
            _quadrant = quadrant ?? new QuadrantData();
        }

        public bool Equals(GalacticRouteKey other) =>
            other != null &&
            string.Equals(_galaxyName, other._galaxyName, StringComparison.OrdinalIgnoreCase) &&
            Equals(_quadrant, other._quadrant);

        public override bool Equals(object obj) => obj is GalacticRouteKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(
            _galaxyName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_galaxyName) : 0,
            _quadrant != null ? _quadrant.GetHashCode() : 0
        );
        public override string ToString() => $"{_galaxyName} -> {_quadrant}";
    }

    /// <summary>
    /// Layer 2: Quadrant Sector within the galaxy.
    /// Contains Layer 3 (<see cref="StarSystemData"/>).
    /// </summary>
    [Serializable]
    public class QuadrantData : IEquatable<QuadrantData>
    {
        [SerializeField] private string _quadrantCode;
        [SerializeField] private StarSystemData _starSystem = new StarSystemData();

        public string QuadrantCode => _quadrantCode;
        public StarSystemData StarSystem => _starSystem;

        public QuadrantData() { }
        public QuadrantData(string quadrantCode, StarSystemData starSystem)
        {
            _quadrantCode = quadrantCode;
            _starSystem = starSystem ?? new StarSystemData();
        }

        public bool Equals(QuadrantData other) =>
            other != null &&
            string.Equals(_quadrantCode, other._quadrantCode, StringComparison.OrdinalIgnoreCase) &&
            Equals(_starSystem, other._starSystem);

        public override bool Equals(object obj) => obj is QuadrantData other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(
            _quadrantCode != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_quadrantCode) : 0,
            _starSystem != null ? _starSystem.GetHashCode() : 0
        );
        public override string ToString() => $"Q[{_quadrantCode}] -> {_starSystem}";
    }

    /// <summary>
    /// Layer 3: Star System within the quadrant.
    /// Contains Layer 4 (<see cref="PlanetOrbitData"/>).
    /// </summary>
    [Serializable]
    public class StarSystemData : IEquatable<StarSystemData>
    {
        [SerializeField] private int _systemCode;
        [SerializeField] private PlanetOrbitData _planet = new PlanetOrbitData();

        public int SystemCode => _systemCode;
        public PlanetOrbitData Planet => _planet;

        public StarSystemData() { }
        public StarSystemData(int systemCode, PlanetOrbitData planet)
        {
            _systemCode = systemCode;
            _planet = planet ?? new PlanetOrbitData();
        }

        public bool Equals(StarSystemData other) =>
            other != null && _systemCode == other._systemCode && Equals(_planet, other._planet);

        public override bool Equals(object obj) => obj is StarSystemData other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_systemCode, _planet != null ? _planet.GetHashCode() : 0);
        public override string ToString() => $"Sys#{_systemCode} -> {_planet}";
    }

    /// <summary>
    /// Layer 4: Planet and Orbit position.
    /// Contains Layer 5 (<see cref="SurfaceSectorData"/>).
    /// </summary>
    [Serializable]
    public class PlanetOrbitData : IEquatable<PlanetOrbitData>
    {
        [SerializeField] private string _planetName;
        [SerializeField] private SurfaceSectorData _sector = new SurfaceSectorData();

        public string PlanetName => _planetName;
        public SurfaceSectorData Sector => _sector;

        public PlanetOrbitData() { }
        public PlanetOrbitData(string planetName, SurfaceSectorData sector)
        {
            _planetName = planetName;
            _sector = sector ?? new SurfaceSectorData();
        }

        public bool Equals(PlanetOrbitData other) =>
            other != null &&
            string.Equals(_planetName, other._planetName, StringComparison.OrdinalIgnoreCase) &&
            Equals(_sector, other._sector);

        public override bool Equals(object obj) => obj is PlanetOrbitData other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(
            _planetName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_planetName) : 0,
            _sector != null ? _sector.GetHashCode() : 0
        );
        public override string ToString() => $"{_planetName} -> {_sector}";
    }

    /// <summary>
    /// Layer 5: Surface Sector on the planet.
    /// Contains Layer 6 (<see cref="SubGridCoordinate"/>).
    /// </summary>
    [Serializable]
    public class SurfaceSectorData : IEquatable<SurfaceSectorData>
    {
        [SerializeField] private string _sectorCode;
        [SerializeField] private SubGridCoordinate _coordinate = new SubGridCoordinate();

        public string SectorCode => _sectorCode;
        public SubGridCoordinate Coordinate => _coordinate;

        public SurfaceSectorData() { }
        public SurfaceSectorData(string sectorCode, SubGridCoordinate coordinate)
        {
            _sectorCode = sectorCode;
            _coordinate = coordinate ?? new SubGridCoordinate();
        }

        public bool Equals(SurfaceSectorData other) =>
            other != null &&
            string.Equals(_sectorCode, other._sectorCode, StringComparison.OrdinalIgnoreCase) &&
            Equals(_coordinate, other._coordinate);

        public override bool Equals(object obj) => obj is SurfaceSectorData other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(
            _sectorCode != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_sectorCode) : 0,
            _coordinate != null ? _coordinate.GetHashCode() : 0
        );
        public override string ToString() => $"{_sectorCode} {_coordinate}";
    }

    /// <summary>
    /// Layer 6: Deepest 2D Sub-Grid Coordinate.
    /// </summary>
    [Serializable]
    public class SubGridCoordinate : IEquatable<SubGridCoordinate>
    {
        [SerializeField] private int _x;
        [SerializeField] private int _y;

        public int X => _x;
        public int Y => _y;

        public SubGridCoordinate() { }
        public SubGridCoordinate(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public bool Equals(SubGridCoordinate other) =>
            other != null && _x == other._x && _y == other._y;

        public override bool Equals(object obj) => obj is SubGridCoordinate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_x, _y);
        public override string ToString() => $"({_x}, {_y})";
    }

    /// <summary>
    /// Value type paired with the 6-layer deep key.
    /// </summary>
    [Serializable]
    public class SpaceStationInfo
    {
        [SerializeField] private string _stationName;
        [SerializeField] private int _defenseRating;
        [SerializeField] private bool _isHostile;

        public string StationName => _stationName;
        public int DefenseRating => _defenseRating;
        public bool IsHostile => _isHostile;

        public SpaceStationInfo() { }
        public SpaceStationInfo(string name, int defense, bool hostile = false)
        {
            _stationName = name;
            _defenseRating = defense;
            _isHostile = hostile;
        }

        public override string ToString() => $"{_stationName} (DEF:{_defenseRating}, Hostile:{_isHostile})";
    }
}
