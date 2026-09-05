// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  GameConfig.cs  (Sample: BasicUsage)
// ============================================================

using UnityEngine;

namespace EngineEdge.SmartDictionary.Samples
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> that holds game configuration data
    /// using <see cref="SerializableDictionary{TKey,TValue}"/> fields.
    /// </summary>
    /// <remarks>
    /// Create an asset via the Unity menu:
    /// <c>Assets → Create → EngineEdge → SmartDictionary → GameConfig</c>.
    /// </remarks>
    [CreateAssetMenu(
        fileName = "GameConfig",
        menuName = "EngineEdge/SmartDictionary/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        // ------------------------------------------------------------------ //
        //  Serialized data
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Maps an item name to its shop price (in gold).
        /// </summary>
        [Header("Economy")]
        [Tooltip("Item name → price in gold")]
        [SerializeField]
        private SerializableDictionary<string, float> itemPrices
            = new SerializableDictionary<string, float>();

        /// <summary>
        /// Maps an enemy type name to its base health value.
        /// </summary>
        [Header("Combat")]
        [Tooltip("Enemy type → base HP")]
        [SerializeField]
        private SerializableDictionary<string, int> enemyHealthValues
            = new SerializableDictionary<string, int>();

        /// <summary>
        /// Maps an item name to its inventory icon sprite.
        /// </summary>
        [Header("Art")]
        [Tooltip("Item name → sprite")]
        [SerializeField]
        private SerializableDictionary<string, Sprite> itemIcons
            = new SerializableDictionary<string, Sprite>();

        // ------------------------------------------------------------------ //
        //  Public API
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns the price for <paramref name="itemName"/>, or
        /// <c>0f</c> if the item is not configured.
        /// </summary>
        /// <param name="itemName">The item to look up.</param>
        /// <returns>The item price, or <c>0f</c> as the default.</returns>
        public float GetItemPrice(string itemName)
            => itemPrices.GetOrDefault(itemName, 0f);

        /// <summary>
        /// Returns the base health value for <paramref name="enemyType"/>,
        /// or <c>100</c> if the enemy type is not configured.
        /// </summary>
        /// <param name="enemyType">The enemy type to look up.</param>
        /// <returns>The base health, or <c>100</c> as a sensible default.</returns>
        public int GetEnemyHealth(string enemyType)
            => enemyHealthValues.GetOrDefault(enemyType, 100);

        /// <summary>
        /// Returns the icon sprite for <paramref name="itemName"/>,
        /// or <see langword="null"/> if none is configured.
        /// </summary>
        /// <param name="itemName">The item to look up.</param>
        /// <returns>The item's <see cref="Sprite"/>, or <see langword="null"/>.</returns>
        public Sprite GetItemIcon(string itemName)
            => itemIcons.GetOrDefault(itemName, null);
    }
}
