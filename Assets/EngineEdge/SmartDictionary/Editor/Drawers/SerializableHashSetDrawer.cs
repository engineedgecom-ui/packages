// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  SerializableHashSetDrawer.cs
//  Custom property drawer for SerializableHashSet<T>.
// ============================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EngineEdge.SmartDictionary.Editor
{
    /// <summary>
    /// Custom property drawer for <see cref="SerializableHashSet{T}"/>.
    /// Renders a single-column list with duplicate detection and Undo support.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableHashSet<>), true)]
    public class SerializableHashSetDrawer : PropertyDrawer
    {
        // ------------------------------------------------------------------ //
        //  Constants
        // ------------------------------------------------------------------ //

        private const float AddBtnHeight = 22f;
        private const float Padding      = 3f;
        private const float RemoveWidth  = 22f;

        private static float RowHeight    => EditorGUIUtility.singleLineHeight + 4f;
        private static float HeaderHeight => EditorGUIUtility.singleLineHeight + 2f;

        // ------------------------------------------------------------------ //
        //  Static per-property state
        // ------------------------------------------------------------------ //

        private static readonly Dictionary<string, bool> s_FoldoutStates = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> s_EventsFoldoutStates = new Dictionary<string, bool>();

        // ------------------------------------------------------------------ //
        //  PropertyDrawer overrides
        // ------------------------------------------------------------------ //

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InspectorStyles.EnsureInitialized();

            var items = property.FindPropertyRelative("_serializedItems");
            if (items == null) return HeaderHeight;

            if (!IsExpanded(property))
                return HeaderHeight;

            var rowsH = items.arraySize > 0 
                ? items.arraySize * RowHeight 
                : EditorGUIUtility.singleLineHeight + 4f;

            var totalHeight = HeaderHeight + rowsH + AddBtnHeight + Padding * 2f;

            // Unity Events section (only shown if events are enabled)
            var eventsProp = property.FindPropertyRelative("_eventsEnabled");
            if (eventsProp != null && eventsProp.boolValue)
            {
                totalHeight += EditorGUIUtility.singleLineHeight + Padding;
                if (IsEventsExpanded(property))
                {
                    totalHeight += GetEventsHeight(property);
                }
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InspectorStyles.EnsureInitialized();

            EditorGUI.BeginProperty(position, label, property);

            var items = property.FindPropertyRelative("_serializedItems");
            if (items == null)
            {
                EditorGUI.LabelField(position, label.text, "Missing _serializedItems");
                EditorGUI.EndProperty();
                return;
            }

            float currentY = position.y;

            // ── 1. Header / Foldout ──────────────────────────────────────
            var headerRect = new Rect(position.x, currentY, position.width, HeaderHeight);
            DrawHeader(headerRect, property, items, label);
            currentY += HeaderHeight + Padding;

            if (!IsExpanded(property))
            {
                EditorGUI.EndProperty();
                return;
            }

            var contentRect = EditorGUI.IndentedRect(new Rect(position.x, currentY, position.width, position.height - (currentY - position.y)));
            var origIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // ── 2. Rows ──────────────────────────────────────────────────
            if (items.arraySize == 0)
            {
                var emptyRect = new Rect(contentRect.x, currentY, contentRect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(emptyRect, "— HashSet is empty —", InspectorStyles.EmptyNotice);
                currentY += EditorGUIUtility.singleLineHeight + 4f;
            }
            else
            {
                var dupSet = FindDuplicateIndices(items);
                int removeAt = -1;

                for (var i = 0; i < items.arraySize; i++)
                {
                    var rowRect = new Rect(contentRect.x, currentY, contentRect.width, RowHeight);
                    DrawRow(rowRect, items, i, dupSet, ref removeAt);
                    currentY += RowHeight;
                }

                if (removeAt >= 0)
                {
                    GUI.FocusControl(null);
                    int prevSize = items.arraySize;
                    items.DeleteArrayElementAtIndex(removeAt);
                    if (items.arraySize == prevSize)
                    {
                        items.DeleteArrayElementAtIndex(removeAt);
                    }
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            // ── 3. Add Button ────────────────────────────────────────────
            var addRect = new Rect(contentRect.x, currentY + Padding, contentRect.width, AddBtnHeight);
            DrawAddButton(addRect, property, items);
            currentY += AddBtnHeight + Padding * 2f;

            // ── 4. Unity Events Section (only shown if events are enabled) ──
            var eventsProp = property.FindPropertyRelative("_eventsEnabled");
            if (eventsProp != null && eventsProp.boolValue)
            {
                currentY = DrawEventsSection(contentRect.x, currentY, contentRect.width, property);
            }

            EditorGUI.indentLevel = origIndent;
            EditorGUI.EndProperty();
        }

        // ------------------------------------------------------------------ //
        //  Drawing helpers
        // ------------------------------------------------------------------ //

        private void DrawHeader(Rect rect, SerializedProperty property,
                                SerializedProperty items, GUIContent label)
        {
            var path = property.propertyPath;
            var count = items.arraySize;
            var fieldDisplayName = string.IsNullOrEmpty(label.text) ? "HashSet" : label.text;
            var headerText = $"{fieldDisplayName} ({count} {(count == 1 ? "item" : "items")})";

            var eventsProp = property.FindPropertyRelative("_eventsEnabled");
            float toggleWidth = eventsProp != null ? 66f : 0f;
            var foldoutRect = new Rect(rect.x, rect.y, rect.width - toggleWidth - (toggleWidth > 0 ? 4f : 0f), rect.height);

            bool expanded = IsExpanded(property);
            bool newExpanded = EditorGUI.Foldout(foldoutRect, expanded, new GUIContent(headerText, label.tooltip), true, InspectorStyles.HeaderFoldout);
            s_FoldoutStates[path] = newExpanded;

            if (eventsProp != null)
            {
                var toggleRect = new Rect(rect.xMax - toggleWidth, rect.y + 1f, toggleWidth, rect.height - 2f);
                bool isEnabled = eventsProp.boolValue;

                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = isEnabled ? new Color(0.7f, 1f, 0.7f, 1f) : new Color(1f, 0.6f, 0.6f, 0.8f);

                var content = new GUIContent(
                    isEnabled ? "⚡ Events" : "⚡ Muted",
                    isEnabled 
                        ? "Events: ENABLED\nOnItemAdded, OnItemRemoved, etc. will fire on mutations.\nClick to mute." 
                        : "Events: MUTED\nNo mutation events will fire.\nClick to enable.");

                if (GUI.Button(toggleRect, content, EditorStyles.miniButton))
                {
                    eventsProp.boolValue = !isEnabled;
                    property.serializedObject.ApplyModifiedProperties();
                }

                GUI.backgroundColor = prevBg;
            }
        }

        private void DrawRow(Rect rect, SerializedProperty items, int index,
                             HashSet<int> dupSet, ref int removeAt)
        {
            var bgStyle = (index % 2 == 0) ? InspectorStyles.RowEven : InspectorStyles.RowOdd;
            GUI.Box(rect, GUIContent.none, bgStyle);

            var itemProp    = items.GetArrayElementAtIndex(index);
            var isDuplicate = dupSet.Contains(index);

            var usable = rect.width - RemoveWidth - (isDuplicate ? 20f : 0f) - 6f;

            float x = rect.x + 3f;
            float y = rect.y + 2f;
            float h = EditorGUIUtility.singleLineHeight;

            // Warning icon for duplicates
            if (isDuplicate)
            {
                var warnRect = new Rect(x, y, 16f, h);
                var icon = InspectorStyles.GetWarningIcon();
                if (icon != null)
                    GUI.DrawTexture(warnRect, icon, ScaleMode.ScaleToFit);
                else
                    EditorGUI.LabelField(warnRect, "⚠");
                x += 18f;
            }

            // Item field
            var fieldRect = new Rect(x, y, usable, h);
            EditorGUI.PropertyField(fieldRect, itemProp, GUIContent.none, true);

            // Remove button
            var btnRect = new Rect(rect.xMax - RemoveWidth - 2f, y, RemoveWidth, h);
            if (GUI.Button(btnRect, "−", InspectorStyles.RemoveButton))
                removeAt = index;
        }

        private void DrawAddButton(Rect rect, SerializedProperty property, SerializedProperty items)
        {
            if (GUI.Button(rect, "+ Add Item", InspectorStyles.AddButton))
            {
                GUI.FocusControl(null);
                property.serializedObject.Update();

                // Open foldout so the new item is visible
                s_FoldoutStates[property.propertyPath] = true;

                int newIndex = items.arraySize;
                items.InsertArrayElementAtIndex(newIndex);

                var newItem = items.GetArrayElementAtIndex(newIndex);
                if (newItem != null)
                {
                    AssignUniqueDefaultItem(newItem, items, newIndex);
                }

                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private static void AssignUniqueDefaultItem(SerializedProperty itemProp, SerializedProperty items, int newIndex)
        {
            switch (itemProp.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    var existing = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < items.arraySize; i++)
                    {
                        if (i == newIndex) continue;
                        var p = items.GetArrayElementAtIndex(i);
                        if (p != null && p.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(p.stringValue))
                        {
                            existing.Add(p.stringValue);
                        }
                    }

                    string baseItem = "New Item";
                    string candidate = baseItem;
                    int counter = 1;
                    while (existing.Contains(candidate))
                    {
                        candidate = $"{baseItem} {counter++}";
                    }
                    itemProp.stringValue = candidate;
                    break;
                }
                case SerializedPropertyType.Integer:
                {
                    var existing = new HashSet<long>();
                    for (int i = 0; i < items.arraySize; i++)
                    {
                        if (i == newIndex) continue;
                        var p = items.GetArrayElementAtIndex(i);
                        if (p != null && p.propertyType == SerializedPropertyType.Integer)
                        {
                            existing.Add(p.longValue);
                        }
                    }

                    long candidate = 0;
                    while (existing.Contains(candidate))
                    {
                        candidate++;
                    }
                    itemProp.longValue = candidate;
                    break;
                }
                case SerializedPropertyType.Float:
                {
                    var existing = new HashSet<float>();
                    for (int i = 0; i < items.arraySize; i++)
                    {
                        if (i == newIndex) continue;
                        var p = items.GetArrayElementAtIndex(i);
                        if (p != null && p.propertyType == SerializedPropertyType.Float)
                        {
                            existing.Add(p.floatValue);
                        }
                    }

                    float candidate = 0f;
                    while (existing.Contains(candidate))
                    {
                        candidate += 1f;
                    }
                    itemProp.floatValue = candidate;
                    break;
                }
                case SerializedPropertyType.Enum:
                {
                    var existing = new HashSet<int>();
                    for (int i = 0; i < items.arraySize; i++)
                    {
                        if (i == newIndex) continue;
                        var p = items.GetArrayElementAtIndex(i);
                        if (p != null && p.propertyType == SerializedPropertyType.Enum)
                        {
                            existing.Add(p.enumValueIndex);
                        }
                    }

                    int candidate = 0;
                    int maxEnum = itemProp.enumDisplayNames.Length;
                    while (candidate < maxEnum && existing.Contains(candidate))
                    {
                        candidate++;
                    }
                    if (candidate < maxEnum)
                    {
                        itemProp.enumValueIndex = candidate;
                    }
                    break;
                }
            }
        }

        private static bool IsExpanded(SerializedProperty property)
        {
            var path = property.propertyPath;
            if (!s_FoldoutStates.TryGetValue(path, out var expanded))
            {
                expanded = true;
                s_FoldoutStates[path] = expanded;
            }
            return expanded;
        }

        private HashSet<int> FindDuplicateIndices(SerializedProperty items)
        {
            var seen = new Dictionary<string, int>();
            var dups = new HashSet<int>();

            for (var i = 0; i < items.arraySize; i++)
            {
                var prop = items.GetArrayElementAtIndex(i);
                var s = prop.propertyType == SerializedPropertyType.String
                    ? prop.stringValue
                    : prop.displayName;

                if (string.IsNullOrEmpty(s))
                    continue;

                if (seen.TryGetValue(s, out var firstIdx))
                {
                    dups.Add(firstIdx);
                    dups.Add(i);
                }
                else
                {
                    seen[s] = i;
                }
            }
            return dups;
        }

        // ------------------------------------------------------------------ //
        //  Unity Events Section
        // ------------------------------------------------------------------ //

        private static readonly (string propName, string displayName, string tooltip)[] s_HashSetEvents =
        {
            ("_onItemAddedEvent", "On Item Added", "Invoked whenever a new item is added to the set."),
            ("_onItemRemovedEvent", "On Item Removed", "Invoked whenever an existing item is removed from the set."),
            ("_onClearedEvent", "On Cleared", "Invoked whenever the set is cleared."),
            ("_onCountChangedEvent", "On Count Changed", "Invoked with the new total item count whenever set size changes.")
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
            foreach (var (propName, _, _) in s_HashSetEvents)
            {
                var evtProp = property.FindPropertyRelative(propName);
                if (evtProp != null)
                {
                    height += EditorGUI.GetPropertyHeight(evtProp, true) + 2f;
                }
            }
            return height;
        }

        private float DrawEventsSection(float x, float y, float width, SerializedProperty property)
        {
            var path = property.propertyPath + ".__events";
            bool expanded = IsEventsExpanded(property);

            var foldoutRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
            expanded = EditorGUI.Foldout(foldoutRect, expanded, new GUIContent("Unity Events (Serialized)", "Inspect and hook up UnityEvent listeners in the Inspector."), true, EditorStyles.foldoutHeader);
            s_EventsFoldoutStates[path] = expanded;
            y += EditorGUIUtility.singleLineHeight + Padding;

            if (expanded)
            {
                EditorGUI.indentLevel++;
                foreach (var (propName, displayName, tooltip) in s_HashSetEvents)
                {
                    var evtProp = property.FindPropertyRelative(propName);
                    if (evtProp != null)
                    {
                        float h = EditorGUI.GetPropertyHeight(evtProp, true);
                        var evtRect = new Rect(x, y, width, h);
                        EditorGUI.PropertyField(evtRect, evtProp, new GUIContent(displayName, tooltip), true);
                        y += h + 2f;
                    }
                }
                EditorGUI.indentLevel--;
            }

            return y;
        }
    }
}
