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
        internal static GUIStyle EventsBar { get; private set; }
        internal static GUIStyle EventsBarTitleButton { get; private set; }
        internal static GUIStyle EventsBadgeActive { get; private set; }
        internal static GUIStyle EventsBadgeMuted { get; private set; }
        internal static GUIStyle EventsContainer { get; private set; }

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

            // Events bar at the end (sleek rounded toolbar header)
            var barBg = EditorGUIUtility.isProSkin
                ? new Color(0.20f, 0.20f, 0.20f, 1f)
                : new Color(0.88f, 0.88f, 0.88f, 1f);
            var barBorder = EditorGUIUtility.isProSkin
                ? new Color(0.14f, 0.14f, 0.14f, 1f)
                : new Color(0.72f, 0.72f, 0.72f, 1f);

            EventsBar = new GUIStyle();
            EventsBar.normal.background = MakeBoxTex(6, 6, barBg, barBorder);
            EventsBar.border = new RectOffset(2, 2, 2, 2);

            EventsBarTitleButton = new GUIStyle(GUIStyle.none)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 0, 0, 0)
            };
            EventsBarTitleButton.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.85f, 0.85f, 0.85f, 1f)
                : new Color(0.18f, 0.18f, 0.18f, 1f);
            EventsBarTitleButton.hover.textColor = EditorGUIUtility.isProSkin
                ? Color.white
                : Color.black;

            // Active badge (soft emerald green pill)
            var activeBg = EditorGUIUtility.isProSkin
                ? new Color(0.12f, 0.30f, 0.16f, 1f)
                : new Color(0.78f, 0.94f, 0.82f, 1f);
            var activeBorder = EditorGUIUtility.isProSkin
                ? new Color(0.20f, 0.50f, 0.28f, 1f)
                : new Color(0.40f, 0.75f, 0.48f, 1f);

            EventsBadgeActive = new GUIStyle(GUIStyle.none)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(4, 4, 1, 1)
            };
            EventsBadgeActive.normal.background = MakeBoxTex(6, 6, activeBg, activeBorder);
            EventsBadgeActive.border = new RectOffset(2, 2, 2, 2);
            EventsBadgeActive.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.45f, 0.95f, 0.55f, 1f)
                : new Color(0.10f, 0.45f, 0.18f, 1f);

            // Muted badge (soft warm amber pill)
            var mutedBg = EditorGUIUtility.isProSkin
                ? new Color(0.28f, 0.22f, 0.15f, 1f)
                : new Color(0.96f, 0.90f, 0.80f, 1f);
            var mutedBorder = EditorGUIUtility.isProSkin
                ? new Color(0.48f, 0.36f, 0.20f, 1f)
                : new Color(0.82f, 0.65f, 0.40f, 1f);

            EventsBadgeMuted = new GUIStyle(GUIStyle.none)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(4, 4, 1, 1)
            };
            EventsBadgeMuted.normal.background = MakeBoxTex(6, 6, mutedBg, mutedBorder);
            EventsBadgeMuted.border = new RectOffset(2, 2, 2, 2);
            EventsBadgeMuted.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(1f, 0.75f, 0.40f, 1f)
                : new Color(0.60f, 0.35f, 0.05f, 1f);

            // Container card for expanded events
            var contBg = EditorGUIUtility.isProSkin
                ? new Color(0.16f, 0.16f, 0.16f, 0.95f)
                : new Color(0.92f, 0.92f, 0.92f, 0.95f);
            var contBorder = EditorGUIUtility.isProSkin
                ? new Color(0.12f, 0.12f, 0.12f, 1f)
                : new Color(0.78f, 0.78f, 0.78f, 1f);

            EventsContainer = new GUIStyle();
            EventsContainer.normal.background = MakeBoxTex(6, 6, contBg, contBorder);
            EventsContainer.border = new RectOffset(2, 2, 2, 2);
            EventsContainer.padding = new RectOffset(6, 6, 6, 6);

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

        private static Texture2D MakeBoxTex(int width, int height, Color fill, Color border)
        {
            var pix = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x == 0 || x == width - 1 || y == 0 || y == height - 1);
                    pix[y * width + x] = isBorder ? border : fill;
                }
            }
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
