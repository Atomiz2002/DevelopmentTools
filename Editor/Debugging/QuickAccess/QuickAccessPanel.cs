#if UNITY_EDITOR && !SIMULATE_BUILD
using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Editor.Editor_.Toolbar_Injections;
using DevelopmentTools.Editor.Extensions.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Color = System.Drawing.Color;
using EditorSettings = DevelopmentTools.Editor.Debugging.Settings.EditorSettings;

namespace DevelopmentTools.Editor.Debugging.QuickAccess {

    public class QuickAccessPanel : EditorWindow {

        private static          List<Object> pinned        = new();
        private static          List<Object> history       = new();
        private static readonly List<Object> removeObjects = new();

        private static          Vector2 scroll;
        private static          bool?   openedFromShortcut;
        private static          int     selectedIndex;
        private static readonly float   elementHeight = EditorGUIUtility.singleLineHeight;

        // todo gray dir next to prefabs names

        [InitializeOnLoadMethod]
        private static void Initialize() {
            ToolbarGUIInjector.AddToolbarPopupButton(ToolbarGUIInjector.ToolbarSide.RightOfPlay, "Quick Access", 110, DrawGUI, 300, 700);

            Load();
            Selection.selectionChanged += () => {
                if (history.Contains(Selection.activeObject))
                    history.Remove(Selection.activeObject);

                history.Insert(0, Selection.activeObject);

                history = history.Distinct().ToList();
                Save();
            };
        }

        [MenuItem(EditorSettings.MenuGroupPath + "Quick Access Panel %q")]
        private static void OpenWindow(MenuCommand command) => GetWindow<QuickAccessPanel>("Quick Access").Show();

        private void OnEnable() {
            openedFromShortcut = null;
            Load();

            selectedIndex = Selection.activeObject == null ? 0 : 1;
        }

        private void OnGUI() {
            DrawGUI(this);
        }

        private static void DrawGUI() => DrawGUI(null);

        private static void DrawGUI(EditorWindow window = null) {
            if (window) {
                openedFromShortcut ??= Event.current.control;

                if (openedFromShortcut == true && !Event.current.control) {
                    window.Close();
                    EditorUtility.FocusProjectWindow();
                    return;
                }

                if (Event.current.alt) {
                    Deselect();
                    window.Close();
                    EditorUtility.FocusProjectWindow();
                    return;
                }
            }

            // Q to iterate history elements
            if (history.Any() && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Q) {
                selectedIndex = (selectedIndex + (Event.current.shift ? -1 : 1) + history.Count) % history.Count; // + history.Count to wrap around negatives
                scroll.y      = elementHeight * selectedIndex;
                Event.current.Use();
            }

            // Rect windowRect = new(Vector2.zero, window.position.size);
            // EditorHelper.DrawDropZone<Object>(windowRect,
            //     dropped => {
            //         pinned.AddRange(dropped);
            //         pinned = pinned.Distinct().ToList();
            //     });

            // DrawCopyPaste(); // not fully working yet

            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawPinned();

            if (pinned.Any() && history.Any()) {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Label("History ", new GUIStyle(SirenixGUIStyles.LeftAlignedGreyMiniLabel) { stretchWidth = false, margin = new(6, 0, -6, 0) });
                SirenixEditorGUI.HorizontalLineSeparator(Color.DimGray.ToUnityColor());
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            DrawHistory();

            // if (!pinned.Any())
            //     DrawDropZoneUI(windowRect);

            window?.Repaint();

            EditorGUILayout.EndScrollView();
        }

        private static void DrawDropZoneUI(Rect windowRect) {
            EditorHelper.DrawDashedBorder(windowRect, 10f, 5f, 5f, Color.Gray.ToUnityColor());

            EditorGUI.LabelField(windowRect,
                "Drag here to pin",
                new GUIStyle(GUI.skin.label) {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize  = 22
                });
        }

        private static void DrawCopyPaste() {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy")) EditorPrefs.GetString(nameof(pinned)).CopyToClipboard();
            if (GUILayout.Button("Paste")) EditorPrefs.SetString(nameof(pinned), EditorGUIUtility.systemCopyBuffer);
            GUILayout.EndHorizontal();
        }

        private static void DrawPinned() {
            GUIStyle label = new(GUI.skin.label) {
                fontStyle = FontStyle.Bold,
                normal = {
                    textColor = Color.White.ToUnityColor()
                }
            };

            Rect rect = EditorGUILayout.GetControlRect(false, elementHeight * pinned.Count);
            rect.height = elementHeight;

            Rect modifyRect = rect;
            Rect iconRect   = rect;
            Rect labelRect  = rect;

            modifyRect.width =  elementHeight;
            iconRect.width   =  elementHeight;
            labelRect.width  -= (elementHeight + 2) * 2;

            iconRect.x  += iconRect.width + 2;
            labelRect.x += (elementHeight + 2) * 2;

            modifyRect.y -= elementHeight;
            iconRect.y   -= elementHeight;
            labelRect.y  -= elementHeight;

            foreach (Object o in pinned) {
                modifyRect.y += elementHeight;
                iconRect.y   += elementHeight;
                labelRect.y  += elementHeight;

                bool queuedForRemoval = removeObjects.Contains(o);
                label.normal.textColor = queuedForRemoval ? Color.Tomato.ToUnityColor() : label.normal.textColor;

                if (queuedForRemoval) {
                    if (SirenixEditorGUI.SDFIconButton(modifyRect, SdfIconType.ArrowCounterclockwise, GUI.skin.label))
                        removeObjects.Remove(o);
                }
                else {
                    if (SirenixEditorGUI.SDFIconButton(modifyRect, SdfIconType.X, GUI.skin.label))
                        removeObjects.Add(o);
                }

                Texture icon = EditorGUIUtility.ObjectContent(o, o.GetType()).image;
                GUI.DrawTexture(iconRect, icon);

                if (GUI.Button(labelRect, o.name, label)) {
                    Deselect();
                    o.SelectAndPing();
                }
            }
        }

        private static void DrawHistory() {
            GUIStyle          label          = new(GUI.skin.label);
            UnityEngine.Color labelTextColor = label.normal.textColor;

            Rect rect = EditorGUILayout.GetControlRect(false, elementHeight * history.Count);
            rect.height = elementHeight;

            Rect modifyRect = rect;
            Rect iconRect   = rect;
            Rect labelRect  = rect;

            modifyRect.width =  elementHeight;
            iconRect.width   =  elementHeight;
            labelRect.width  -= (elementHeight + 2) * 2;

            iconRect.x  += iconRect.width + 2;
            labelRect.x += (elementHeight + 2) * 2;

            modifyRect.y -= elementHeight;
            iconRect.y   -= elementHeight;
            labelRect.y  -= elementHeight;

            foreach (Object o in history) {
                modifyRect.y += elementHeight;
                iconRect.y   += elementHeight;
                labelRect.y  += elementHeight;

                bool isPinned = pinned.Contains(o);
                bool selected = history.IndexOf(o) == selectedIndex;
                label.fontStyle = selected
                    ? FontStyle.Bold
                    : FontStyle.Normal;

                label.normal.textColor = selected
                    ? Color.Cyan.ToUnityColor()
                    : isPinned
                        ? Color.White.ToUnityColor()
                        : labelTextColor;

                if (isPinned) {
                    if (SirenixEditorGUI.SDFIconButton(modifyRect, SdfIconType.PinFill, GUI.skin.label))
                        pinned.Remove(o);
                }
                else {
                    if (SirenixEditorGUI.SDFIconButton(modifyRect, SdfIconType.Pin, GUI.skin.label))
                        pinned.Add(o);
                }

                Texture icon = EditorGUIUtility.ObjectContent(o, o.GetType()).image;
                GUI.DrawTexture(iconRect, icon);

                if (GUI.Button(labelRect, o.name, label)) {
                    Deselect();
                    o.SelectAndPing();
                }
            }
        }

        private static void Deselect() => selectedIndex = -1;

        private void OnDestroy() {
            pinned.RemoveAll(removeObjects.Contains);
            removeObjects.Clear();

            Save();

            if (selectedIndex > -1 && selectedIndex < history.Count)
                history[selectedIndex].SelectAndPing();
        }

        private static void Load() {
            pinned  = EditorPrefs.GetString(nameof(pinned)).Split(";").Select(guid => guid.LoadAssetByGUID<Object>()).Existing().ToList();
            history = EditorPrefs.GetString(nameof(history)).Split(";").Select(guid => guid.LoadAssetByGUID<Object>()).Existing().ToList();
        }

        private static void Save() {
            EditorPrefs.SetString(nameof(pinned), pinned.Select(o => o.GetAssetGUID()).Join(";"));
            EditorPrefs.SetString(nameof(history), history.Select(o => o.GetAssetGUID()).Join(";"));
        }

        // [MenuItem("CONTEXT/QuickAccessPanel/Copy")]
        // private static void Copy() {}

    }

}
#endif