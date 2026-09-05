using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EngineEdge.SmartDictionary
{
    // ──────────────────────────────────────────────────────────────────────────
    // Internal JSON wrapper
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Internal wrapper used to serialize a <see cref="SerializableDictionary{TKey,TValue}"/>
    /// via <see cref="JsonUtility"/>. <see cref="JsonUtility"/> requires a concrete
    /// top-level object — it cannot serialize a raw list directly.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    [Serializable]
    internal class SerializableDictionaryWrapper<TKey, TValue>
    {
        /// <summary>
        /// The flat list of key-value pairs that represents the dictionary contents.
        /// </summary>
        public List<SerializableKeyValuePair<TKey, TValue>> pairs
            = new List<SerializableKeyValuePair<TKey, TValue>>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Extension methods
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Provides JSON serialization, file I/O, and <see cref="PlayerPrefs"/>
    /// persistence extension methods for <see cref="SerializableDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <remarks>
    /// All JSON operations use <see cref="JsonUtility"/> and therefore require
    /// that both <typeparamref name="TKey"/> and <typeparamref name="TValue"/>
    /// are types supported by Unity's serializer (primitives, strings, and
    /// <c>[Serializable]</c> plain classes/structs).
    /// </remarks>
    public static class SmartDictionarySerializationExtensions
    {
        // ── JSON ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Serializes the dictionary to a JSON string using <see cref="JsonUtility"/>.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to serialize.</param>
        /// <returns>A JSON string representing the dictionary's contents.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static string ToJson<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            source.OnBeforeSerialize();

            var wrapper = new SerializableDictionaryWrapper<TKey, TValue>();
            foreach (var kvp in source)
                wrapper.pairs.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));

            return JsonUtility.ToJson(wrapper, prettyPrint: true);
        }

        /// <summary>
        /// Populates the dictionary in place from a JSON string that was previously
        /// produced by <see cref="ToJson{TKey,TValue}"/>.
        /// All existing entries are cleared before population.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="target">The dictionary to populate.</param>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="target"/> or <paramref name="json"/> is <c>null</c>.
        /// </exception>
        public static void FromJson<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> target,
            string                                    json)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (json   == null) throw new ArgumentNullException(nameof(json));

            var wrapper = JsonUtility.FromJson<SerializableDictionaryWrapper<TKey, TValue>>(json);
            if (wrapper == null)
            {
                Debug.LogWarning("[SmartDictionary] FromJson: JSON deserialization returned null.");
                return;
            }

            target.EventsEnabled = false;
            target.Clear();
            foreach (var pair in wrapper.pairs)
            {
                if (pair.Key == null)
                {
                    Debug.LogWarning("[SmartDictionary] FromJson: Skipping entry with null key.");
                    continue;
                }
                target.TryAdd(pair.Key, pair.Value);
            }
            target.EventsEnabled = true;
        }

        // ── File I/O ──────────────────────────────────────────────────────────

        /// <summary>
        /// Serializes the dictionary to JSON and writes it to the file at
        /// <paramref name="filePath"/>, creating the file (and any parent directories)
        /// if they do not already exist.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to save.</param>
        /// <param name="filePath">The absolute or relative path of the output file.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="filePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when an I/O error occurs while writing the file.
        /// </exception>
        public static void SaveToFile<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source,
            string                                    filePath)
        {
            if (source   == null) throw new ArgumentNullException(nameof(source));
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, source.ToJson(), System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Reads a JSON file from <paramref name="filePath"/> and populates the
        /// dictionary in place. All existing entries are cleared first.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="target">The dictionary to populate.</param>
        /// <param name="filePath">The path of the JSON file to read.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="target"/> or <paramref name="filePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown when no file exists at <paramref name="filePath"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when an I/O error occurs while reading the file.
        /// </exception>
        public static void LoadFromFile<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> target,
            string                                    filePath)
        {
            if (target   == null) throw new ArgumentNullException(nameof(target));
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"[SmartDictionary] LoadFromFile: File not found: {filePath}", filePath);

            var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            target.FromJson(json);
        }

        // ── PlayerPrefs ───────────────────────────────────────────────────────

        /// <summary>
        /// Serializes the dictionary to JSON and stores the result in
        /// <see cref="PlayerPrefs"/> under <paramref name="prefsKey"/>.
        /// <see cref="PlayerPrefs.Save"/> is called automatically to flush to disk.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The dictionary to persist.</param>
        /// <param name="prefsKey">The PlayerPrefs key under which the JSON is stored.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="prefsKey"/> is <c>null</c>.
        /// </exception>
        public static void ToPlayerPrefs<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> source,
            string                                    prefsKey)
        {
            if (source   == null) throw new ArgumentNullException(nameof(source));
            if (prefsKey == null) throw new ArgumentNullException(nameof(prefsKey));

            PlayerPrefs.SetString(prefsKey, source.ToJson());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads a dictionary from a JSON string previously stored in
        /// <see cref="PlayerPrefs"/> under <paramref name="prefsKey"/> and
        /// populates <paramref name="target"/> in place.
        /// All existing entries are cleared before population.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="target">The dictionary to populate.</param>
        /// <param name="prefsKey">The PlayerPrefs key to read the JSON from.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="target"/> or <paramref name="prefsKey"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// If <paramref name="prefsKey"/> does not exist in <see cref="PlayerPrefs"/>
        /// the dictionary is simply cleared and a warning is logged — no exception is
        /// thrown.
        /// </remarks>
        public static void FromPlayerPrefs<TKey, TValue>(
            this SerializableDictionary<TKey, TValue> target,
            string                                    prefsKey)
        {
            if (target   == null) throw new ArgumentNullException(nameof(target));
            if (prefsKey == null) throw new ArgumentNullException(nameof(prefsKey));

            if (!PlayerPrefs.HasKey(prefsKey))
            {
                Debug.LogWarning($"[SmartDictionary] FromPlayerPrefs: Key '{prefsKey}' not found in PlayerPrefs.");
                return;
            }

            var json = PlayerPrefs.GetString(prefsKey);
            target.FromJson(json);
        }
    }
}
