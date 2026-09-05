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
    [CustomPropertyDrawer(typeof(SerializableStack<>), true)]
    [CustomPropertyDrawer(typeof(SerializableQueue<>), true)]
    public class SerializableHashSetDrawer : PropertyDrawer
    {
        // ------------------------------------------------------------------ //
        //  Constants
        // ------------------------------------------------------------------ //

        private const float AddBtnHeight     = 22f;
        private const float EventsBarHeight  = 22f;
        private const float EventsBoxPadding = 6f;
        private const float Padding          = 3f;
        private const float RemoveWidth      = 22f;

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

            float rowsH = 0f;
            if (items.arraySize == 0)
            {
                rowsH = EditorGUIUtility.singleLineHeight + 4f;
            }
            else
            {
                for (int i = 0; i < items.arraySize; i++)
                {
                    rowsH += GetItemHeight(items.GetArrayElementAtIndex(i));
                }
            }

            var totalHeight = HeaderHeight + rowsH + AddBtnHeight + Padding * 2f;

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
                    var itemProp = items.GetArrayElementAtIndex(i);
                    float rowH = GetItemHeight(itemProp);
                    var rowRect = new Rect(contentRect.x, currentY, contentRect.width, rowH);
                    DrawRow(rowRect, items, i, dupSet, ref removeAt, rowH);
                    currentY += rowH;
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

            // ── 4. Events Section (at the end) ───────────────────────────
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

        private static float GetItemHeight(SerializedProperty itemProp)
        {
            if (itemProp == null) return EditorGUIUtility.singleLineHeight + 4f;
            return EditorGUI.GetPropertyHeight(itemProp, true) + 4f;
        }

        private void DrawHeader(Rect rect, SerializedProperty property,
                                SerializedProperty items, GUIContent label)
        {
            var path = property.propertyPath;
            var count = items.arraySize;
            var fieldDisplayName = string.IsNullOrEmpty(label.text) ? "HashSet" : label.text;
            var headerText = $"{fieldDisplayName} ({count} {(count == 1 ? "item" : "items")})";

            bool expanded = IsExpanded(property);
            bool newExpanded = EditorGUI.Foldout(rect, expanded, new GUIContent(headerText, label.tooltip), true, InspectorStyles.HeaderFoldout);
            s_FoldoutStates[path] = newExpanded;
        }

        private void DrawRow(Rect rect, SerializedProperty items, int index,
                             HashSet<int> dupSet, ref int removeAt, float rowH)
        {
            var bgStyle = (index % 2 == 0) ? InspectorStyles.RowEven : InspectorStyles.RowOdd;
            GUI.Box(rect, GUIContent.none, bgStyle);

            var itemProp    = items.GetArrayElementAtIndex(index);
            var isDuplicate = dupSet.Contains(index);

            var usable = rect.width - RemoveWidth - (isDuplicate ? 20f : 0f) - 6f;

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

            // Item field
            float itemH = EditorGUI.GetPropertyHeight(itemProp, true);
            var fieldRect = new Rect(x, y, usable, itemH);
            var itemLabel = (itemProp.propertyType == SerializedPropertyType.Generic && itemProp.hasVisibleChildren)
                ? new GUIContent(itemProp.displayName)
                : GUIContent.none;
            EditorGUI.PropertyField(fieldRect, itemProp, itemLabel, true);

            // Remove button
            var btnRect = new Rect(rect.xMax - RemoveWidth - 2f, y, RemoveWidth, EditorGUIUtility.singleLineHeight);
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
                case SerializedPropertyType.ObjectReference:
                {
                    itemProp.objectReferenceValue = null;
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
                if (prop == null) continue;

                string s;
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    s = prop.objectReferenceValue != null ? $"{prop.objectReferenceValue.name}_{prop.objectReferenceValue.GetHashCode()}" : "null_obj_ref";
                }
                else if (prop.propertyType == SerializedPropertyType.String)
                {
                    s = prop.stringValue;
                }
                else
                {
                    s = prop.displayName;
                }

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
                ? $"{arrow}  ⚡ Events ({s_HashSetEvents.Length})" 
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

                foreach (var (propName, displayName, tooltip) in s_HashSetEvents)
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
