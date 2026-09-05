// ============================================================
//  EngineEdge – Smart Dictionary Pro
//  InspectorStyles.cs
//  Lazy-initialised GUIStyle cache for custom drawers.
// ============================================================

using UnityEditor;
using UnityEngine;

namespace EngineEdge.SmartDictionary.Editor
{
    /// <summary>
    /// Provides lazily-initialised, cached <see cref="GUIStyle"/> instances
    /// for a clean, native-feeling Unity Inspector appearance.
    /// </summary>
    internal static class InspectorStyles
    {
        internal static bool IsInitialized { get; private set; }

        internal static GUIStyle HeaderFoldout { get; private set; }
        internal static GUIStyle ColumnHeader { get; private set; }
        internal static GUIStyle RowEven { get; private set; }
        internal static GUIStyle RowOdd { get; private set; }
        internal static GUIStyle RemoveButton { get; private set; }
        internal static GUIStyle AddButton { get; private set; }
        internal static GUIStyle SearchField { get; private set; }
        internal static GUIStyle EmptyNotice { get; private set; }

        internal static void EnsureInitialized()
        {
            if (IsInitialized)
                return;

            // Foldout with bold header text and native triangle
            HeaderFoldout = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            // Mini column headers
            ColumnHeader = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.3f, 0.3f, 0.3f) }
            };

            // Alternating row backgrounds (subtle tint)
            RowEven = new GUIStyle();
            RowEven.normal.background = MakeTex(1, 1, EditorGUIUtility.isProSkin 
                ? new Color(0.24f, 0.24f, 0.24f, 0.6f) 
                : new Color(0.85f, 0.85f, 0.85f, 0.6f));
            RowEven.margin = new RectOffset(0, 0, 0, 0);
            RowEven.padding = new RectOffset(2, 2, 1, 1);

            RowOdd = new GUIStyle();
            RowOdd.normal.background = MakeTex(1, 1, EditorGUIUtility.isProSkin 
                ? new Color(0.18f, 0.18f, 0.18f, 0.6f) 
                : new Color(0.78f, 0.78f, 0.78f, 0.6f));
            RowOdd.margin = new RectOffset(0, 0, 0, 0);
            RowOdd.padding = new RectOffset(2, 2, 1, 1);

            // Per-row remove (-) button
            RemoveButton = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedWidth = 20f,
                fixedHeight = 18f,
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter
            };
            RemoveButton.normal.textColor = new Color(0.95f, 0.4f, 0.4f, 1f);

            // Add button
            AddButton = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                fixedHeight = 22f,
                alignment = TextAnchor.MiddleCenter
            };

            // Search field
            SearchField = new GUIStyle(EditorStyles.toolbarSearchField);

            // Empty state notice
            EmptyNotice = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontStyle = FontStyle.Italic,
                fontSize = 10
            };

            IsInitialized = true;
        }

        internal static Texture2D GetWarningIcon()
        {
            return EditorGUIUtility.FindTexture("d_console.warnicon.sml");
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; i++)
                pix[i] = col;

            var result = new Texture2D(width, height)
            {
                hideFlags = HideFlags.DontSave
            };
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
