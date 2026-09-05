// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  SerializableDictionaryDrawer.cs
//  Custom property drawer for SerializableDictionary<TKey, TValue>.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EngineEdge.SmartDictionary.Editor
{
    /// <summary>
    /// Custom property drawer for <see cref="SerializableDictionary{TKey,TValue}"/>.
    /// Renders a native-feeling key/value table with search filtering, duplicate warning badges,
    /// and full Undo/Redo support.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    [CustomPropertyDrawer(typeof(SerializableOrderedDictionary<,>), true)]
    [CustomPropertyDrawer(typeof(SerializableBiDictionary<,>), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        // ------------------------------------------------------------------ //
        //  Layout metrics
        // ------------------------------------------------------------------ //

        private const float AddBtnHeight     = 22f;
        private const float EventsBarHeight  = 22f;
        private const float EventsBoxPadding = 6f;
        private const float ColumnHeaderH    = 16f;
        private const float Padding          = 3f;
        private const float KeyRatio         = 0.42f;
        private const float ValRatio         = 0.53f;
        private const float RemoveWidth      = 22f;

        private static float RowHeight       => EditorGUIUtility.singleLineHeight + 4f;
        private static float SearchBarHeight => EditorGUIUtility.singleLineHeight + 4f;
        private static float HeaderHeight    => EditorGUIUtility.singleLineHeight + 2f;

        // ------------------------------------------------------------------ //
        //  Static per-property state
        // ------------------------------------------------------------------ //

        private static readonly Dictionary<string, bool> s_FoldoutStates       = new Dictionary<string, bool>();
        private static readonly Dictionary<string, string> s_SearchStrings     = new Dictionary<string, string>();
        private static readonly Dictionary<string, bool> s_EventsFoldoutStates = new Dictionary<string, bool>();

        // ------------------------------------------------------------------ //
        //  PropertyDrawer overrides
        // ------------------------------------------------------------------ //

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InspectorStyles.EnsureInitialized();

            var pairs = property.FindPropertyRelative("_serializedPairs");
            if (pairs == null) return HeaderHeight;

            if (!IsExpanded(property))
                return HeaderHeight;

            var totalHeight = HeaderHeight + Padding;

            // Search bar (shown if > 3 items or searching)
            var search = GetSearch(property.propertyPath);
            bool showSearch = pairs.arraySize > 3 || !string.IsNullOrEmpty(search);
            if (showSearch)
                totalHeight += SearchBarHeight;

            if (pairs.arraySize == 0)
            {
                totalHeight += EditorGUIUtility.singleLineHeight + 4f; // "Empty" label
            }
            else
            {
                totalHeight += ColumnHeaderH; // Column headers
                var indices = GetFilteredIndices(pairs, search);
                for (int i = 0; i < indices.Count; i++)
                {
                    var pairProp = pairs.GetArrayElementAtIndex(indices[i]);
                    totalHeight += GetRowHeight(pairProp);
                }
            }

            totalHeight += AddBtnHeight + Padding * 2f;

            // Events section at the end (always available when expanded)
            var eventsProp = property.FindPropertyRelative("_eventsEnabled");
            if (eventsProp != null)
            {
                totalHeight += EventsBarHeight + Padding;
                if (eventsProp.boolValue && IsEventsExpanded(property))
                {
                    totalHeight += GetEventsHeight(property) + EventsBoxPadding * 2f + Padding;
                }
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InspectorStyles.EnsureInitialized();

            EditorGUI.BeginProperty(position, label, property);

            var pairs = property.FindPropertyRelative("_serializedPairs");
            if (pairs == null)
            {
                EditorGUI.LabelField(position, label.text, "Missing _serializedPairs");
                EditorGUI.EndProperty();
                return;
            }

            float currentY = position.y;

            // ── 1. Foldout Header ────────────────────────────────────────
            var headerRect = new Rect(position.x, currentY, position.width, HeaderHeight);
            DrawHeader(headerRect, property, pairs, label);
            currentY += HeaderHeight + Padding;

            if (!IsExpanded(property))
            {
                EditorGUI.EndProperty();
                return;
            }

            // Indent content slightly inside the foldout
            var contentRect = EditorGUI.IndentedRect(new Rect(position.x, currentY, position.width, position.height - (currentY - position.y)));
            var origIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0; // reset indent inside custom column layout

            var search = GetSearch(property.propertyPath);
            bool showSearch = pairs.arraySize > 3 || !string.IsNullOrEmpty(search);

            // ── 2. Search Bar (when relevant) ─────────────────────────────
            if (showSearch)
            {
                var searchRect = new Rect(contentRect.x, currentY, contentRect.width, SearchBarHeight - 2f);
                DrawSearchBar(searchRect, property);
                currentY += SearchBarHeight;
            }

            // ── 3. Rows or Empty Notice ──────────────────────────────────
            if (pairs.arraySize == 0)
            {
                var emptyRect = new Rect(contentRect.x, currentY, contentRect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(emptyRect, "— Dictionary is empty —", InspectorStyles.EmptyNotice);
                currentY += EditorGUIUtility.singleLineHeight + 4f;
            }
            else
            {
                // Column headers
                GetColumnRatios(pairs, out float keyRatio, out float valRatio);
                var colHeaderRect = new Rect(contentRect.x, currentY, contentRect.width, ColumnHeaderH);
                DrawColumnHeaders(colHeaderRect, keyRatio, valRatio);
                currentY += ColumnHeaderH;

                var indices = GetFilteredIndices(pairs, search);
                var dupSet  = FindDuplicateKeyIndices(pairs);
                int removeAt = -1;

                for (var i = 0; i < indices.Count; i++)
                {
                    var realIdx = indices[i];
                    var pairProp = pairs.GetArrayElementAtIndex(realIdx);
                    float rowH = GetRowHeight(pairProp);
                    var rowRect = new Rect(contentRect.x, currentY, contentRect.width, rowH);
                    DrawRow(rowRect, pairs, realIdx, i, dupSet, ref removeAt, rowH, keyRatio, valRatio);
                    currentY += rowH;
                }

                if (removeAt >= 0)
                {
                    GUI.FocusControl(null);
                    int prevSize = pairs.arraySize;
                    pairs.DeleteArrayElementAtIndex(removeAt);
                    if (pairs.arraySize == prevSize)
                    {
                        pairs.DeleteArrayElementAtIndex(removeAt);
                    }
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            // ── 4. Add Entry Button ──────────────────────────────────────
            var addRect = new Rect(contentRect.x, currentY + Padding, contentRect.width, AddBtnHeight);
            DrawAddButton(addRect, property, pairs);
            currentY += AddBtnHeight + Padding * 2f;

            // ── 5. Events Section (at the end) ───────────────────────────
            var eventsProp = property.FindPropertyRelative("_eventsEnabled");
            if (eventsProp != null)
            {
                currentY = DrawEventsSection(contentRect.x, currentY, contentRect.width, property);
            }

            EditorGUI.indentLevel = origIndent;
            EditorGUI.EndProperty();
        }

        // ------------------------------------------------------------------ //
        //  Drawing helpers
        // ------------------------------------------------------------------ //

        private void DrawHeader(Rect rect, SerializedProperty property, SerializedProperty pairs, GUIContent label)
        {
            var path = property.propertyPath;
            var count = pairs.arraySize;
            var fieldDisplayName = string.IsNullOrEmpty(label.text) ? "Dictionary" : label.text;
            var headerText = $"{fieldDisplayName} ({count} {(count == 1 ? "entry" : "entries")})";

            bool expanded = IsExpanded(property);
            bool newExpanded = EditorGUI.Foldout(rect, expanded, new GUIContent(headerText, label.tooltip), true, InspectorStyles.HeaderFoldout);
            s_FoldoutStates[path] = newExpanded;
        }

        private void DrawSearchBar(Rect rect, SerializedProperty property)
        {
            var path = property.propertyPath;
            var search = GetSearch(path);

            var labelWidth = 40f;
            var labelRect = new Rect(rect.x, rect.y + 1f, labelWidth, rect.height);
            var fieldRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height - 2f);

            EditorGUI.LabelField(labelRect, "Filter:", EditorStyles.miniLabel);
            var newSearch = EditorGUI.TextField(fieldRect, search, InspectorStyles.SearchField);
            if (newSearch != search)
                s_SearchStrings[path] = newSearch;
        }

        private static void GetColumnRatios(SerializedProperty pairs, out float keyRatio, out float valRatio)
        {
            if (pairs != null && pairs.arraySize > 0)
            {
                var firstPair = pairs.GetArrayElementAtIndex(0);
                var keyProp = firstPair.FindPropertyRelative("_key") ?? firstPair.FindPropertyRelative("Key");
                var valProp = firstPair.FindPropertyRelative("_value") ?? firstPair.FindPropertyRelative("Value");

                bool keyIsGeneric = keyProp != null && keyProp.propertyType == SerializedPropertyType.Generic && keyProp.hasVisibleChildren;
                bool valIsGeneric = valProp != null && valProp.propertyType == SerializedPropertyType.Generic && valProp.hasVisibleChildren;

                if (keyIsGeneric && valIsGeneric)
                {
                    keyRatio = 0.48f;
                    valRatio = 0.48f;
                    return;
                }
                if (keyIsGeneric)
                {
                    keyRatio = 0.58f;
                    valRatio = 0.38f;
                    return;
                }
                if (valIsGeneric)
                {
                    keyRatio = 0.38f;
                    valRatio = 0.58f;
                    return;
                }
            }

            keyRatio = KeyRatio;
            valRatio = ValRatio;
        }

        private void DrawColumnHeaders(Rect rect, float keyRatio, float valRatio)
        {
            var usable = rect.width - RemoveWidth;
            var keyWidth = usable * keyRatio;
            var valWidth = usable * valRatio;

            var keyHeaderRect = new Rect(rect.x + 4f, rect.y, keyWidth, rect.height);
            var valHeaderRect = new Rect(rect.x + 4f + keyWidth, rect.y, valWidth, rect.height);

            EditorGUI.LabelField(keyHeaderRect, "Key", InspectorStyles.ColumnHeader);
            EditorGUI.LabelField(valHeaderRect, "Value", InspectorStyles.ColumnHeader);
        }

        private static float GetRowHeight(SerializedProperty pairProp)
        {
            if (pairProp == null) return EditorGUIUtility.singleLineHeight + 4f;

            var keyProp = pairProp.FindPropertyRelative("_key") ?? pairProp.FindPropertyRelative("Key");
            var valProp = pairProp.FindPropertyRelative("_value") ?? pairProp.FindPropertyRelative("Value");

            float keyH = keyProp != null ? EditorGUI.GetPropertyHeight(keyProp, true) : EditorGUIUtility.singleLineHeight;
            float valH = valProp != null ? EditorGUI.GetPropertyHeight(valProp, true) : EditorGUIUtility.singleLineHeight;

            return Mathf.Max(keyH, valH) + 4f;
        }

        private void DrawRow(Rect rect, SerializedProperty pairs, int realIndex, int visualIndex,
                             HashSet<int> dupSet, ref int removeAt, float rowH,
                             float keyRatio, float valRatio)
        {
            var bgStyle = (visualIndex % 2 == 0) ? InspectorStyles.RowEven : InspectorStyles.RowOdd;
            GUI.Box(rect, GUIContent.none, bgStyle);

            var pairProp = pairs.GetArrayElementAtIndex(realIndex);
            // Support both _key (private field) and Key (public property)
            var keyProp = pairProp.FindPropertyRelative("_key") ?? pairProp.FindPropertyRelative("Key");
            var valueProp = pairProp.FindPropertyRelative("_value") ?? pairProp.FindPropertyRelative("Value");

            var isDuplicate = dupSet.Contains(realIndex);

            var usable = rect.width - RemoveWidth - (isDuplicate ? 20f : 0f) - 6f;
            var keyWidth = usable * keyRatio;
            var valWidth = usable * valRatio;

            float x = rect.x + 3f;
            float y = rect.y + 2f;

            // Warning icon for duplicates
            if (isDuplicate)
            {
                var warnRect = new Rect(x, y, 16f, EditorGUIUtility.singleLineHeight);
                var icon = InspectorStyles.GetWarningIcon();
                if (icon != null)
                    GUI.DrawTexture(warnRect, icon, ScaleMode.ScaleToFit);
                else
                    EditorGUI.LabelField(warnRect, "⚠");
                x += 18f;
            }

            var origLabelWidth = EditorGUIUtility.labelWidth;
            var origIndent = EditorGUI.indentLevel;

            // Key field
            if (keyProp != null)
            {
                float keyH = EditorGUI.GetPropertyHeight(keyProp, true);
                var keyRect = new Rect(x, y, keyWidth - 4f, keyH);
                
                GUIContent keyLabel;
                if (keyProp.propertyType == SerializedPropertyType.Generic && keyProp.hasVisibleChildren)
                {
                    var summary = GetPropertySummary(keyProp);
                    keyLabel = new GUIContent(!string.IsNullOrEmpty(summary) ? summary : keyProp.displayName);
                }
                else
                {
                    keyLabel = GUIContent.none;
                }

                EditorGUI.indentLevel = 0;
                EditorGUIUtility.labelWidth = Mathf.Clamp((keyWidth - 4f) * 0.40f, 50f, 110f);
                EditorGUI.PropertyField(keyRect, keyProp, keyLabel, true);
            }
            else
            {
                var errRect = new Rect(x, y, keyWidth - 4f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(errRect, "(No key)");
            }
            x += keyWidth;

            // Value field
            if (valueProp != null)
            {
                float valH = EditorGUI.GetPropertyHeight(valueProp, true);
                var valRect = new Rect(x, y, valWidth - 4f, valH);

                GUIContent valLabel;
                if (valueProp.propertyType == SerializedPropertyType.Generic && valueProp.hasVisibleChildren)
                {
                    var summary = GetPropertySummary(valueProp);
                    valLabel = new GUIContent(!string.IsNullOrEmpty(summary) ? summary : valueProp.displayName);
                }
                else
                {
                    valLabel = GUIContent.none;
                }

                EditorGUI.indentLevel = 0;
                EditorGUIUtility.labelWidth = Mathf.Clamp((valWidth - 4f) * 0.40f, 50f, 110f);
                EditorGUI.PropertyField(valRect, valueProp, valLabel, true);
            }
            else
            {
                var errRect = new Rect(x, y, valWidth - 4f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(errRect, "(No value)");
            }

            EditorGUIUtility.labelWidth = origLabelWidth;
            EditorGUI.indentLevel = origIndent;

            // Remove button (top aligned)
            var btnRect = new Rect(rect.xMax - RemoveWidth - 2f, y, RemoveWidth, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(btnRect, "−", InspectorStyles.RemoveButton))
            {
                removeAt = realIndex;
            }
        }

        private void DrawAddButton(Rect rect, SerializedProperty property, SerializedProperty pairs)
        {
            if (GUI.Button(rect, "+ Add Entry", InspectorStyles.AddButton))
            {
                GUI.FocusControl(null);
                property.serializedObject.Update();

                // Open foldout and clear search so the newly added entry is immediately visible
                s_FoldoutStates[property.propertyPath] = true;
                s_SearchStrings[property.propertyPath] = string.Empty;

                int newIndex = pairs.arraySize;
                pairs.InsertArrayElementAtIndex(newIndex);

                var newPair = pairs.GetArrayElementAtIndex(newIndex);
                if (newPair != null)
                {
                    var keyProp = newPair.FindPropertyRelative("_key") ?? newPair.FindPropertyRelative("Key");
                    if (keyProp != null)
                    {
                        AssignUniqueDefaultKey(keyProp, pairs, newIndex);
                    }
                }

                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private static void AssignUniqueDefaultKey(SerializedProperty keyProp, SerializedProperty pairs, int newIndex)
        {
            switch (keyProp.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < pairs.arraySize; i++)
                    {
                        if (i == newIndex) continue;
                        var p = pairs.GetArrayElementAtIndex(i);
                        var k = p.FindPropertyRelative("_key") ?? p.FindPropertyRelative("Key");
                        if (k != null && k.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(k.stringValue))
                        {
                            existing.Add(k.stringValue);
                        }
                    }

                    string baseKey = "New Key";
                    string candidate = baseKey;
                    int counter = 1;
                    while (existing.Contains(candidate))
                    {
                        candidate = $"{baseKey} {counter++}";
                    }
                    keyProp.stringValue = candidate;
                    break;
                }
                case SerializedPropertyType.Integer:
                {
                    var existing = new HashSet<long>();
                    for (int i = 0; i < pairs.arraySize; i++)
                    {
                        if (i == newIndex) continue;
                        var p = pairs.GetArrayElementAtIndex(i);
                        var k = p.FindPropertyRelative("_key") ?? p.FindPropertyRelative("Key");
                        if (k != null && k.propertyType == SerializedPropertyType.Integer)
                        {
                            existing.Add(k.longValue);
                        }
                    }

                    long candidate = 0;
                    while (existing.Contains(candidate))
                    {
                        candidate++;
                    }
                    keyProp.longValue = candidate;
                    break;
                }
                case SerializedPropertyType.Float:
                {
                    var existing = new HashSet<float>();
                    for (int i = 0; i < pairs.arraySize; i++)
                    {
                        if (i == newIndex) continue;
                        var p = pairs.GetArrayElementAtIndex(i);
                        var k = p.FindPropertyRelative("_key") ?? p.FindPropertyRelative("Key");
                        if (k != null && k.propertyType == SerializedPropertyType.Float)
                        {
                            existing.Add(k.floatValue);
                        }
                    }

                    float candidate = 0f;
                    while (existing.Contains(candidate))
                    {
                        candidate += 1f;
                    }
                    keyProp.floatValue = candidate;
                    break;
                }
                case SerializedPropertyType.Enum:
                {
                    var existing = new HashSet<int>();
                    for (int i = 0; i < pairs.arraySize; i++)
                    {
                        if (i == newIndex) continue;
                        var p = pairs.GetArrayElementAtIndex(i);
                        var k = p.FindPropertyRelative("_key") ?? p.FindPropertyRelative("Key");
                        if (k != null && k.propertyType == SerializedPropertyType.Enum)
                        {
                            existing.Add(k.enumValueIndex);
                        }
                    }

                    int candidate = 0;
                    int maxEnum = keyProp.enumDisplayNames.Length;
                    while (candidate < maxEnum && existing.Contains(candidate))
                    {
                        candidate++;
                    }
                    if (candidate < maxEnum)
                    {
                        keyProp.enumValueIndex = candidate;
                    }
                    break;
                }
                case SerializedPropertyType.ObjectReference:
                {
                    keyProp.objectReferenceValue = null;
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ //
        //  Utilities
        // ------------------------------------------------------------------ //

        private static bool IsExpanded(SerializedProperty property)
        {
            var path = property.propertyPath;
            if (!s_FoldoutStates.TryGetValue(path, out var expanded))
            {
                // Expanded by default
                expanded = true;
                s_FoldoutStates[path] = expanded;
            }
            return expanded;
        }

        private static string GetSearch(string propertyPath)
        {
            return s_SearchStrings.TryGetValue(propertyPath, out var s) ? s : string.Empty;
        }

        private static List<int> GetFilteredIndices(SerializedProperty pairs, string search)
        {
            var result = new List<int>(pairs.arraySize);
            for (var i = 0; i < pairs.arraySize; i++)
            {
                if (string.IsNullOrEmpty(search))
                {
                    result.Add(i);
                    continue;
                }

                var pair = pairs.GetArrayElementAtIndex(i);
                var keyProp = pair.FindPropertyRelative("_key") ?? pair.FindPropertyRelative("Key");
                var keyStr = keyProp != null ? SerializedPropertyToString(keyProp) : string.Empty;

                if (keyStr.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    result.Add(i);
            }
            return result;
        }

        private static HashSet<int> FindDuplicateKeyIndices(SerializedProperty pairs)
        {
            var seen = new Dictionary<string, int>();
            var dups = new HashSet<int>();

            for (var i = 0; i < pairs.arraySize; i++)
            {
                var pair = pairs.GetArrayElementAtIndex(i);
                var keyProp = pair.FindPropertyRelative("_key") ?? pair.FindPropertyRelative("Key");
                if (keyProp == null) continue;

                string keyIdentifier;
                if (keyProp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (keyProp.objectReferenceValue == null)
                    {
                        keyIdentifier = "null_obj_ref";
                    }
                    else
                    {
                        keyIdentifier = $"{keyProp.objectReferenceValue.name}_{keyProp.objectReferenceValue.GetHashCode()}";
                    }
                }
                else
                {
                    keyIdentifier = SerializedPropertyToString(keyProp);
                }

                if (string.IsNullOrEmpty(keyIdentifier))
                    continue;

                if (seen.TryGetValue(keyIdentifier, out var firstIdx))
                {
                    dups.Add(firstIdx);
                    dups.Add(i);
                }
                else
                {
                    seen[keyIdentifier] = i;
                }
            }
            return dups;
        }

        private static string GetPropertySummary(SerializedProperty prop)
        {
            if (prop == null) return string.Empty;

            if (prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                return prop.objectReferenceValue != null ? prop.objectReferenceValue.name : "(None)";
            }

            if (prop.propertyType != SerializedPropertyType.Generic || !prop.hasVisibleChildren)
            {
                return SerializedPropertyToString(prop);
            }

            var copy = prop.Copy();
            var end = copy.GetEndProperty();
            var parts = new List<string>();
            bool enter = true;

            while (copy.NextVisible(enter) && !SerializedProperty.EqualContents(copy, end))
            {
                enter = false;
                switch (copy.propertyType)
                {
                    case SerializedPropertyType.String:
                        if (!string.IsNullOrEmpty(copy.stringValue))
                            parts.Add(copy.stringValue);
                        break;
                    case SerializedPropertyType.Integer:
                        parts.Add(copy.intValue.ToString());
                        break;
                    case SerializedPropertyType.Float:
                        parts.Add(copy.floatValue.ToString("0.##"));
                        break;
                    case SerializedPropertyType.Enum:
                        parts.Add(copy.enumDisplayNames.Length > copy.enumValueIndex
                            ? copy.enumDisplayNames[copy.enumValueIndex]
                            : copy.enumValueIndex.ToString());
                        break;
                    case SerializedPropertyType.ObjectReference:
                        if (copy.objectReferenceValue != null)
                            parts.Add(copy.objectReferenceValue.name);
                        break;
                }

                if (parts.Count >= 2) break;
            }

            return parts.Count > 0 ? string.Join(" • ", parts) : prop.displayName;
        }

        private static string SerializedPropertyToString(SerializedProperty prop)
        {
            if (prop == null) return string.Empty;

            switch (prop.propertyType)
            {
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Integer:
                    return prop.intValue.ToString();
                case SerializedPropertyType.Float:
                    return prop.floatValue.ToString("G");
                case SerializedPropertyType.Boolean:
                    return prop.boolValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue != null
                        ? $"{prop.objectReferenceValue.name}_{prop.objectReferenceValue.GetHashCode()}"
                        : "null_obj_ref";
                case SerializedPropertyType.Enum:
                    return prop.enumDisplayNames.Length > prop.enumValueIndex
                        ? prop.enumDisplayNames[prop.enumValueIndex]
                        : prop.enumValueIndex.ToString();
                case SerializedPropertyType.Generic:
                {
                    if (!prop.hasVisibleChildren) return prop.type;
                    var sb = new System.Text.StringBuilder();
                    var iterator = prop.Copy();
                    var endProperty = iterator.GetEndProperty();
                    bool enterChildren = true;
                    while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
                    {
                        enterChildren = false;
                        sb.Append(iterator.name).Append(':').Append(SerializedPropertyToString(iterator)).Append(';');
                    }
                    return sb.Length > 0 ? sb.ToString() : prop.type;
                }
                default:
                    return prop.displayName;
            }
        }

        // ------------------------------------------------------------------ //
        //  Unity Events Section
        // ------------------------------------------------------------------ //

        private static readonly (string propName, string displayName, string tooltip)[] s_DictionaryEvents =
        {
            ("_onEntryAddedEvent", "On Entry Added", "Invoked whenever a new entry is added to the dictionary."),
            ("_onEntryRemovedEvent", "On Entry Removed", "Invoked whenever an existing entry is removed from the dictionary."),
            ("_onEntryUpdatedEvent", "On Entry Updated", "Invoked whenever an existing entry's value is changed/updated."),
            ("_onClearedEvent", "On Cleared", "Invoked whenever the dictionary is cleared."),
            ("_onCountChangedEvent", "On Count Changed", "Invoked with the new total entry count whenever dictionary size changes.")
        };

        private static bool IsEventsExpanded(SerializedProperty property)
        {
            var path = property.propertyPath + ".__events";
            if (!s_EventsFoldoutStates.TryGetValue(path, out var expanded))
            {
                expanded = false;
                s_EventsFoldoutStates[path] = expanded;
            }
            return expanded;
        }

        private static float GetEventsHeight(SerializedProperty property)
        {
            float height = 0f;
            foreach (var (propName, _, _) in s_DictionaryEvents)
            {
                var evtProp = property.FindPropertyRelative(propName);
                if (evtProp != null)
                {
                    height += EditorGUI.GetPropertyHeight(evtProp, true) + 4f;
                }
            }
            return height;
        }

        private float DrawEventsSection(float x, float y, float width, SerializedProperty property)
        {
            var eventsProp = property.FindPropertyRelative("_eventsEnabled");
            if (eventsProp == null) return y;

            bool isEnabled = eventsProp.boolValue;
            var path = property.propertyPath + ".__events";
            bool isExpanded = IsEventsExpanded(property);

            var barRect = new Rect(x, y, width, EventsBarHeight);
            y += EventsBarHeight + Padding;

            // 1. Draw Background for the Events Bar
            GUI.Box(barRect, GUIContent.none, InspectorStyles.EventsBar);

            // 2. Status Badge on the right (clickable pill button to toggle enabled/muted)
            float badgeWidth = 66f;
            float badgeHeight = 18f;
            var badgeRect = new Rect(barRect.xMax - badgeWidth - 3f, barRect.y + (EventsBarHeight - badgeHeight) * 0.5f, badgeWidth, badgeHeight);

            var badgeContent = new GUIContent(
                isEnabled ? "● Active" : "○ Muted",
                isEnabled
                    ? "Events: ACTIVE\nClick to mute all mutation events."
                    : "Events: MUTED\nClick to activate mutation events.");

            var badgeStyle = isEnabled ? InspectorStyles.EventsBadgeActive : InspectorStyles.EventsBadgeMuted;

            if (GUI.Button(badgeRect, badgeContent, badgeStyle))
            {
                eventsProp.boolValue = !isEnabled;
                if (!isEnabled)
                {
                    s_EventsFoldoutStates[path] = true;
                }
                property.serializedObject.ApplyModifiedProperties();
                GUI.FocusControl(null);
            }

            // 3. Left title button to toggle expand/collapse
            var foldoutClickRect = new Rect(barRect.x, barRect.y, barRect.width - badgeWidth - 6f, barRect.height);
            var arrow = isExpanded ? "▼" : "▶";
            string titleText = isEnabled 
                ? $"{arrow}  ⚡ Events ({s_DictionaryEvents.Length})" 
                : $"{arrow}  ⚡ Events";

            var titleContent = new GUIContent(titleText, "Click to expand or collapse event listeners.");

            if (GUI.Button(foldoutClickRect, titleContent, InspectorStyles.EventsBarTitleButton))
            {
                s_EventsFoldoutStates[path] = !isExpanded;
                GUI.FocusControl(null);
            }

            // 4. If enabled and expanded, draw listeners inside a clean framed container
            if (isEnabled && isExpanded)
            {
                float listenersHeight = GetEventsHeight(property);
                float boxHeight = listenersHeight + EventsBoxPadding * 2f;
                var boxRect = new Rect(x, y, width, boxHeight);

                GUI.Box(boxRect, GUIContent.none, InspectorStyles.EventsContainer);

                float innerY = y + EventsBoxPadding;
                float innerX = x + EventsBoxPadding;
                float innerW = width - EventsBoxPadding * 2f;

                foreach (var (propName, displayName, tooltip) in s_DictionaryEvents)
                {
                    var evtProp = property.FindPropertyRelative(propName);
                    if (evtProp != null)
                    {
                        float h = EditorGUI.GetPropertyHeight(evtProp, true);
                        var evtRect = new Rect(innerX, innerY, innerW, h);
                        EditorGUI.PropertyField(evtRect, evtProp, new GUIContent(displayName, tooltip), true);
                        innerY += h + 4f;
                    }
                }

                y += boxHeight + Padding;
            }

            return y;
        }
    }
}
