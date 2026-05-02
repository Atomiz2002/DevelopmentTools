using System;
using DevelopmentTools.Settings;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.DevelopmentTools.Editor {

    public class AllCursorsPreview : EditorWindow {

        [MenuItem(EngineSettings.MenuGroupPath + "Unity Cursors Preview")]
        public static void ShowWindow() => GetWindow<AllCursorsPreview>("Unity Cursors Preview");

        private void OnGUI() {
            EditorGUILayout.HelpBox("Hover over the boxes to see the cursor.", MessageType.Info);

            // Iterate through all built-in MouseCursor enum values
            foreach (MouseCursor cursorType in Enum.GetValues(typeof(MouseCursor))) {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                // Draw a label for the cursor name
                GUILayout.Label(cursorType.ToString(), GUILayout.Width(150));

                // Create a preview box area
                Rect previewRect = GUILayoutUtility.GetRect(100, 25);
                EditorGUI.DrawRect(previewRect, new(0.3f, 0.3f, 0.3f));

                // THIS is what changes the cursor when hovering the Rect
                EditorGUIUtility.AddCursorRect(previewRect, cursorType);

                EditorGUILayout.EndHorizontal();
            }
        }

    }

}