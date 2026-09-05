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

            var obs = target as ObservableDictionary<TKey, TValue>;
            bool prevEvents = obs?.EventsEnabled ?? true;
            if (obs != null) obs.EventsEnabled = false;

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

            if (obs != null) obs.EventsEnabled = prevEvents;
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

        // ── Standard Cross-Language JSON Serialization (Data Only) ────────────

        /// <summary>
        /// Serializes any Smart Dictionary or key-value collection into standard, cross-language
        /// JSON Object format (<c>{"Key": Value}</c>) containing pure data only (no Unity internal fields or events).
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="source">Dictionary to serialize.</param>
        /// <param name="prettyPrint">Whether to format with indentation.</param>
        /// <returns>Standard JSON Object string.</returns>
        public static string ToStandardJson<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source,
            bool prettyPrint = true)
        {
            if (source == null) return "null";

            var sb = new System.Text.StringBuilder();
            sb.Append(prettyPrint ? "{\n" : "{");

            bool first = true;
            foreach (var kvp in source)
            {
                if (!first)
                {
                    sb.Append(prettyPrint ? ",\n" : ",");
                }
                first = false;

                string keyStr = FormatJsonKey(kvp.Key);
                string valStr = SerializeValue(kvp.Value, indentLevel: 1, prettyPrint: prettyPrint);

                if (prettyPrint)
                {
                    sb.Append("  \"").Append(EscapeJsonString(keyStr)).Append("\": ").Append(valStr);
                }
                else
                {
                    sb.Append("\"").Append(EscapeJsonString(keyStr)).Append("\":").Append(valStr);
                }
            }

            sb.Append(prettyPrint ? "\n}" : "}");
            return sb.ToString();
        }

        /// <summary>
        /// Serializes any collection, set, stack, or queue into standard, cross-language
        /// JSON Array format (<c>[Item1, Item2, ...]</c>) containing pure data only.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="source">Collection to serialize.</param>
        /// <param name="prettyPrint">Whether to format with indentation.</param>
        /// <returns>Standard JSON Array string.</returns>
        public static string ToStandardJson<T>(
            this IEnumerable<T> source,
            bool prettyPrint = true)
        {
            if (source == null) return "null";

            var sb = new System.Text.StringBuilder();
            sb.Append(prettyPrint ? "[\n" : "[");

            bool first = true;
            foreach (var item in source)
            {
                if (!first)
                {
                    sb.Append(prettyPrint ? ",\n" : ",");
                }
                first = false;

                string itemStr = SerializeValue(item, indentLevel: 1, prettyPrint: prettyPrint);
                if (prettyPrint)
                {
                    sb.Append("  ").Append(itemStr);
                }
                else
                {
                    sb.Append(itemStr);
                }
            }

            sb.Append(prettyPrint ? "\n]" : "]");
            return sb.ToString();
        }

        // ── Internal Pure Data JSON Helpers ───────────────────────────────────

        private static string FormatJsonKey(object key)
        {
            if (key == null) return "null";
            return key.ToString();
        }

        private static string SerializeValue(object val, int indentLevel, bool prettyPrint)
        {
            if (val == null) return "null";

            if (val is string s)
                return "\"" + EscapeJsonString(s) + "\"";

            if (val is bool b)
                return b ? "true" : "false";

            if (val is byte || val is sbyte || val is short || val is ushort ||
                val is int || val is uint || val is long || val is ulong)
                return val.ToString();

            if (val is float f)
                return f.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (val is double d)
                return d.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (val is decimal dec)
                return dec.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (val is Enum e)
                return "\"" + e.ToString() + "\"";

            // Check if value is a dictionary (Dictionary of Dictionaries)
            if (val is System.Collections.IEnumerable enumerable && !(val is string))
            {
                var dictKvpList = new List<KeyValuePair<string, object>>();
                bool isDictionary = false;

                foreach (var item in enumerable)
                {
                    if (item == null) continue;
                    var itemType = item.GetType();
                    var keyProp = itemType.GetProperty("Key");
                    var valProp = itemType.GetProperty("Value");

                    if (keyProp != null && valProp != null)
                    {
                        isDictionary = true;
                        var k = keyProp.GetValue(item, null);
                        var v = valProp.GetValue(item, null);
                        dictKvpList.Add(new KeyValuePair<string, object>(FormatJsonKey(k), v));
                    }
                    else
                    {
                        break;
                    }
                }

                if (isDictionary)
                {
                    string indent = new string(' ', indentLevel * 2);
                    string innerIndent = new string(' ', (indentLevel + 1) * 2);
                    var sb = new System.Text.StringBuilder();
                    sb.Append(prettyPrint ? "{\n" : "{");

                    for (int i = 0; i < dictKvpList.Count; i++)
                    {
                        if (i > 0) sb.Append(prettyPrint ? ",\n" : ",");
                        var kvp = dictKvpList[i];
                        string valJson = SerializeValue(kvp.Value, indentLevel + 1, prettyPrint);

                        if (prettyPrint)
                        {
                            sb.Append(innerIndent).Append("\"").Append(EscapeJsonString(kvp.Key)).Append("\": ").Append(valJson);
                        }
                        else
                        {
                            sb.Append("\"").Append(EscapeJsonString(kvp.Key)).Append("\":").Append(valJson);
                        }
                    }

                    sb.Append(prettyPrint ? "\n" + indent + "}" : "}");
                    return sb.ToString();
                }
            }

            // For custom classes/structs (Data only: strip delegates, events, non-serialized fields)
            try
            {
                string rawJson = JsonUtility.ToJson(val, prettyPrint);
                if (!string.IsNullOrEmpty(rawJson) && rawJson != "{}" && rawJson != "null")
                {
                    if (prettyPrint && indentLevel > 0)
                    {
                        // Indent multiline JSON object
                        string indent = new string(' ', indentLevel * 2);
                        string[] lines = rawJson.Split('\n');
                        for (int i = 1; i < lines.Length; i++)
                        {
                            lines[i] = indent + lines[i];
                        }
                        return string.Join("\n", lines);
                    }
                    return rawJson;
                }
            }
            catch
            {
                // Fallback to string representation if JsonUtility fails on non-serializable type
            }

            return "\"" + EscapeJsonString(val.ToString()) + "\"";
        }

        private static string EscapeJsonString(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
        }
    }
}
