// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  SaveLoadDemo.cs  (Sample: SaveLoad)
// ============================================================

using System.IO;
using UnityEngine;

namespace EngineEdge.SmartDictionary.Samples
{
    /// <summary>
    /// Demonstrates JSON file and <see cref="PlayerPrefs"/> serialization of
    /// <see cref="SerializableDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <remarks>
    /// Attach this component to a GameObject in the <c>SaveLoad</c> sample scene,
    /// populate <see cref="saveData"/> in the Inspector, then press Play and use
    /// the on-screen buttons to save and load.
    /// </remarks>
    [AddComponentMenu("EngineEdge/SmartDictionary/Save Load Demo")]
    public class SaveLoadDemo : MonoBehaviour
    {
        // ------------------------------------------------------------------ //
        //  Serialized fields
        // ------------------------------------------------------------------ //

        /// <summary>
        /// The dictionary that will be saved and loaded.
        /// Fill it with string → int entries in the Inspector.
        /// </summary>
        [Header("Save Data")]
        [Tooltip("Key → value pairs to persist")]
        [SerializeField]
        private SerializableDictionary<string, int> saveData
            = new SerializableDictionary<string, int>();

        // ------------------------------------------------------------------ //
        //  Private helpers
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Full file system path used for JSON save / load.
        /// Stored in <see cref="Application.persistentDataPath"/> so it works
        /// on all platforms.
        /// </summary>
        private string SaveFilePath =>
            Path.Combine(Application.persistentDataPath, "smartdict_savedata.json");

        // ------------------------------------------------------------------ //
        //  Public API
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Serializes <see cref="saveData"/> to a JSON file on disk and logs
        /// the path to the Console.
        /// </summary>
        public void SaveToJson()
        {
            saveData.SaveToFile(SaveFilePath);
            Debug.Log($"[SaveLoad] Dictionary saved to: {SaveFilePath}");
        }

        /// <summary>
        /// Deserializes <see cref="saveData"/> from the JSON file on disk.
        /// Logs a warning if the file does not exist.
        /// </summary>
        public void LoadFromJson()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning($"[SaveLoad] Save file not found: {SaveFilePath}");
                return;
            }

            saveData.LoadFromFile(SaveFilePath);
            Debug.Log($"[SaveLoad] Dictionary loaded from: {SaveFilePath}");
        }

        /// <summary>
        /// Persists <see cref="saveData"/> to <see cref="PlayerPrefs"/>
        /// under the key <c>"SaveData"</c>.
        /// </summary>
        public void SaveToPlayerPrefs()
        {
            saveData.ToPlayerPrefs("SaveData");
            Debug.Log("[SaveLoad] Dictionary saved to PlayerPrefs (key: 'SaveData').");
        }

        /// <summary>
        /// Restores <see cref="saveData"/> from <see cref="PlayerPrefs"/>
        /// using the key <c>"SaveData"</c>.
        /// </summary>
        public void LoadFromPlayerPrefs()
        {
            saveData.FromPlayerPrefs("SaveData");
            Debug.Log("[SaveLoad] Dictionary loaded from PlayerPrefs (key: 'SaveData').");
        }

        // ------------------------------------------------------------------ //
        //  GUI
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Renders simple on-screen buttons for each save / load operation.
        /// </summary>
        private void OnGUI()
        {
            const float btnW = 220f;
            const float btnH = 40f;
            const float pad  = 10f;

            float x = pad;
            float y = pad;

            if (GUI.Button(new Rect(x, y, btnW, btnH), "Save to JSON"))
                SaveToJson();

            y += btnH + pad;
            if (GUI.Button(new Rect(x, y, btnW, btnH), "Load from JSON"))
                LoadFromJson();

            y += btnH + pad;
            if (GUI.Button(new Rect(x, y, btnW, btnH), "Save to PlayerPrefs"))
                SaveToPlayerPrefs();

            y += btnH + pad;
            if (GUI.Button(new Rect(x, y, btnW, btnH), "Load from PlayerPrefs"))
                LoadFromPlayerPrefs();
        }
    }
}
