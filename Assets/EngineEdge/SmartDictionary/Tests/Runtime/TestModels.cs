// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  TestModels.cs
//  Self-contained test models for Class & Struct collection tests.
// ============================================================

using System;
using UnityEngine;

namespace EngineEdge.SmartDictionary.Tests
{
    /// <summary>
    /// Test class key implementing IEquatable for value equality.
    /// </summary>
    [Serializable]
    public class CharacterProfile : IEquatable<CharacterProfile>
    {
        [SerializeField] private string _heroName;
        [SerializeField] private string _heroClass;
        [SerializeField] private int _level;

        public string HeroName => _heroName;
        public string HeroClass => _heroClass;
        public int Level => _level;

        public CharacterProfile() { }

        public CharacterProfile(string heroName, string heroClass, int level = 1)
        {
            _heroName = heroName;
            _heroClass = heroClass;
            _level = level;
        }

        public bool Equals(CharacterProfile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _heroName == other._heroName && _heroClass == other._heroClass;
        }

        public override bool Equals(object obj)
        {
            return obj is CharacterProfile other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (_heroName != null ? _heroName.GetHashCode() : 0);
                hash = hash * 31 + (_heroClass != null ? _heroClass.GetHashCode() : 0);
                return hash;
            }
        }

        public override string ToString() => $"{_heroName} ({_heroClass})";
    }

    /// <summary>
    /// Test struct value type.
    /// </summary>
    [Serializable]
    public struct CombatStats : IEquatable<CombatStats>
    {
        public int health;
        public int attackPower;
        public int defense;
        public float critChance;

        public CombatStats(int health, int attackPower, int defense, float critChance)
        {
            this.health = health;
            this.attackPower = attackPower;
            this.defense = defense;
            this.critChance = critChance;
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
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + health;
                hash = hash * 31 + attackPower;
                hash = hash * 31 + defense;
                hash = hash * 31 + critChance.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => $"HP:{health} ATK:{attackPower} DEF:{defense}";
    }

    /// <summary>
    /// Test class value type.
    /// </summary>
    [Serializable]
    public class SkillData
    {
        public string skillName;
        public int manaCost;
        public float cooldown;

        public SkillData() { }

        public SkillData(string skillName, int manaCost, float cooldown)
        {
            this.skillName = skillName;
            this.manaCost = manaCost;
            this.cooldown = cooldown;
        }

        public override string ToString() => $"{skillName} (MP:{manaCost}, CD:{cooldown:F1}s)";
    }

    /// <summary>
    /// Test struct key implementing IEquatable.
    /// </summary>
    [Serializable]
    public struct PlayerBadge : IEquatable<PlayerBadge>
    {
        public int badgeId;
        public string rarity;

        public PlayerBadge(int badgeId, string rarity)
        {
            this.badgeId = badgeId;
            this.rarity = rarity;
        }

        public bool Equals(PlayerBadge other)
        {
            return badgeId == other.badgeId && rarity == other.rarity;
        }

        public override bool Equals(object obj) => obj is PlayerBadge other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + badgeId;
                hash = hash * 31 + (rarity != null ? rarity.GetHashCode() : 0);
                return hash;
            }
        }

        public override string ToString() => $"[{rarity}] Badge#{badgeId}";
    }
}
