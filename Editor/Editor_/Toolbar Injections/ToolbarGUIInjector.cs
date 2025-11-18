#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;

namespace DevelopmentTools.Editor.Editor_.Toolbar_Injections {

    [InitializeOnLoad]
    public static class ToolbarGUIInjector {

        public static readonly List<(Action drawGUI, float width, float order)> leftGUI        = new();
        public static readonly List<(Action drawGUI, float width, float order)> rightGUI       = new();
        public static readonly List<(Action drawGUI, float width, float order)> leftOfPlayGUI  = new();
        public static readonly List<(Action drawGUI, float width, float order)> rightOfPlayGUI = new();

        static ToolbarGUIInjector() => EditorApplication.update += DrawInjectedGUIs;

        private static void DrawInjectedGUIs() {
            Object[] toolbars = Resources.FindObjectsOfTypeAll(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar"));
            if (toolbars.Length == 0)
                return;

            Object    toolbar   = toolbars[0];
            FieldInfo rootField = toolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rootField == null)
                return;

            if (rootField.GetValue(toolbar) is not VisualElement root || root.Q("CustomToolbarGUI") != null)
                return;

            foreach ((Action drawGUI, float width, _) in rightGUI.OrderByDescending(e => e.order))
                root.Q("ToolbarZoneRightAlign")?.Add(new IMGUIContainer(drawGUI) { style = { width = width } });

            foreach ((Action drawGUI, float width, _) in leftGUI.OrderByDescending(e => e.order))
                root.Q("ToolbarZoneLeftAlign")?.Add(new IMGUIContainer(drawGUI) { style = { width = width } });

            VisualElement playButtons = root.Q("ToolbarZonePlayMode");

            foreach ((Action drawGUI, float width, _) in leftOfPlayGUI.OrderByDescending(e => e.order)) {
                float totalWidth = leftOfPlayGUI.Sum(e => e.width);

                playButtons.Insert(0, new IMGUIContainer(drawGUI) { style = { width = width } });
                playButtons.style.marginLeft = -totalWidth;
            }

            foreach ((Action drawGUI, float width, _) in rightOfPlayGUI.OrderByDescending(e => e.order)) { // offsetting by margins (below) is untested standalone & untested with left elements (how would the margins behave)
                float totalWidth = rightOfPlayGUI.Sum(e => e.width);

                playButtons.Add(new IMGUIContainer(drawGUI) { style = { width = width } });
                playButtons.style.marginRight = totalWidth;
            }

            EditorApplication.update -= DrawInjectedGUIs;
        }

        public static void AddToolbarPopupButton(ToolbarSide side, string buttonName, float buttonWidth, Action drawGUI, float popupWidth, float popupHeight = 0, float order = 0, float space = 15) {
            switch (side) {
                case ToolbarSide.Left:        leftGUI.Add((drawToolbarPopupButton, buttonWidth, order)); break;
                case ToolbarSide.Right:       rightGUI.Add((drawToolbarPopupButton, buttonWidth, order)); break;
                case ToolbarSide.LeftOfPlay:  leftOfPlayGUI.Add((drawToolbarPopupButton, buttonWidth, order)); break;
                case ToolbarSide.RightOfPlay: rightOfPlayGUI.Add((drawToolbarPopupButton, buttonWidth, order)); break;
                default:                      throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            return;

            void drawToolbarPopupButton() {
                GUILayout.BeginHorizontal();

                if (side is ToolbarSide.Right or ToolbarSide.RightOfPlay)
                    GUILayout.Space(space);

                if (GUILayout.Button(buttonName, SirenixGUIStyles.DropDownMiniButton)) {
                    Vector2 position = GUILayoutUtility.GetLastRect().position;
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    CustomPopup popup     = new(drawGUI, popupWidth, popupHeight);
                    Rect        popupRect = new(position, popup.GetWindowSize());
                    PopupWindow.Show(popupRect, popup);
                }

                if (side is ToolbarSide.Left or ToolbarSide.LeftOfPlay)
                    GUILayout.Space(space);

                GUILayout.EndHorizontal();
            }
        }

        public enum ToolbarSide {

            Left,
            Right,
            LeftOfPlay,
            RightOfPlay

        }

    }

}
#endif