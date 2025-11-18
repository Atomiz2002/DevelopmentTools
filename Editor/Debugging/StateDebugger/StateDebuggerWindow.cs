#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentTools.Editor.Debugging.StateDebugger.IndexedList;
using DevelopmentTools.Editor.Debugging.StateDebugger.WrapperHandling;
using DevelopmentTools.Editor.Extensions;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using EditorSettings = DevelopmentTools.Editor.Debugging.Settings.EditorSettings;

namespace DevelopmentTools.Editor.Debugging.StateDebugger {

    public class StateDebuggerWindow : EditorWindow {

        [OdinSerialize]
        private Dictionary<string, IndexedList<(PropertyTree tree, DateTime time, StackFrame frame)>> fieldsStates = new();

        // [OdinSerialize]
        // public PropertyTree tree;

        private static readonly Dictionary<PropertyTree, (float min, float max)> visibleStatesRange = new();

        private static bool RealtimeUpdate = false; // TODO + button "snapshot"

        private readonly Dictionary<string, bool> fieldsStatesFoldouts = new();

        private static Vector2 scrollPosition;

        [MenuItem(EditorSettings.MenuGroupPath + "State Debugger")]
        public static void TryShowWindow() => EditorSettings.TryShowWindow(GetWindow<StateDebuggerWindow>(), "State Debugger");

        public static void DebugState<T>(T t, string name) {
            StateDebuggerWindow debugger = GetWindow<StateDebuggerWindow>();
            name ??= (t?.ToString() ?? typeof(T).ToString()).Replace("\n", string.Empty);

            debugger.fieldsStates.TryAdd(name, new(IndexedListBehaviour.Default));

            PropertyTree prevTree = debugger.fieldsStates[name][0].tree;
            PropertyTree currTree = PropertyTree.Create(new PropertyTreeWrapper<T>(t));
            currTree.AttributeProcessorLocator = new PropertyTreeWrapperAttributeProcessorLocator();

            MarkDiffs(currTree, prevTree);

            (PropertyTree tree, DateTime Now, StackFrame frame) state = (currTree, DateTime.Now, new StackTrace(2, true).GetFrame(0));

            debugger.fieldsStates[name].Insert(0, state);
        }

        private static void MarkDiffs(PropertyTree curr, PropertyTree prev) {
            if (prev == null)
                return;

            List<InspectorProperty> currProps = curr?.EnumerateTree().ToList();
            List<InspectorProperty> prevProps = prev?.EnumerateTree().ToList();

            for (int i = 0; i < currProps?.Count; i++) {
                InspectorProperty currProp = currProps[i];
                InspectorProperty prevProp = prevProps?.ElementAtOrDefault(i);

                if (currProp == null) {
                    if (prevProp != null)
                        prevProp.GetAttribute<PropertyTreeWrapperAttribute>().DiffNext = true;

                    continue;
                }

                if (prevProp == null) {
                    currProp.GetAttribute<PropertyTreeWrapperAttribute>().DiffPrev = true;
                    continue;
                }

                if (currProp.ValueEntry?.WeakSmartValue?.ToJSON() == prevProp.ValueEntry?.WeakSmartValue?.ToJSON()) {
                    currProp.GetAttribute<PropertyTreeWrapperAttribute>().DiffPrev = false;
                    prevProp.GetAttribute<PropertyTreeWrapperAttribute>().DiffNext = false;
                    continue;
                }

                currProp.GetAttribute<PropertyTreeWrapperAttribute>().DiffPrev = true;
                prevProp.GetAttribute<PropertyTreeWrapperAttribute>().DiffNext = true;
            }
        }

        protected void OnInspectorUpdate() => Repaint();

        protected void OnGUI() => DrawCachedProperties();

        private void DrawCachedProperties() {
            if (fieldsStates == null)
                return;

            if (GUILayout.Button("Diffs"))
                foreach (IndexedList<(PropertyTree tree, DateTime time, StackFrame frame)> states in fieldsStates.Values) {
                    for (int i = 0; i < states.Count; i++)
                        MarkDiffs(states[i].tree, states[i - 1].tree);
                }

            GUIStyle warningLabelStyle = new(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = Color.yellow }
            };

            GUIStyle foldoutLabelStyle = new(EditorStyles.foldout) {
                fontSize  = 12,
                fontStyle = FontStyle.Bold
            };

            GUIStyle timeLabelStyle = new(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter,
                hover = {
                    textColor = Color.cyan
                }
            };

            GUIHelper.PushColor(Color.yellow);
            SirenixEditorGUI.BeginBox();
            GUIHelper.PopColor();
            EditorGUILayout.LabelField("Modifications are not persisted!", warningLabelStyle);
            SirenixEditorGUI.EndBox();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach ((string name, IndexedList<(PropertyTree tree, DateTime time, StackFrame frame)> fieldStates) in fieldsStates) {
                (PropertyTree tree, DateTime time, StackFrame frame) = fieldStates.Curr;

                EditorGUILayout.BeginHorizontal();
                visibleStatesRange.TryAdd(tree, (0, 1));
                (float min, float max) = visibleStatesRange[tree];
                EditorGUILayout.MinMaxSlider(ref min, ref max, 0, fieldStates.Count);
                visibleStatesRange[tree] = (min, max);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical();

                fieldsStatesFoldouts.TryAdd(name, true);

                if (fieldsStatesFoldouts[name] = EditorGUI.Foldout(EditorGUILayout.GetControlRect(true), fieldsStatesFoldouts[name], name, true, foldoutLabelStyle)) {
                    if (tree.EnumerateTree().ElementAtOrDefault(0) == null) {
                        GUIHelper.PushColor(Color.yellow);
                        SirenixEditorGUI.BeginBox();
                        GUIHelper.PopColor();
                        EditorGUILayout.LabelField("null", warningLabelStyle);
                        SirenixEditorGUI.EndBox();

                        EditorGUILayout.EndVertical();
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal(new GUIStyle { fixedWidth = position.width - 20 });

                    foreach ((PropertyTree t, _, _) in fieldStates.GetRange(Mathf.RoundToInt(min), Math.Max(1, Mathf.RoundToInt(max) - Mathf.RoundToInt(min)))) {
                        EditorGUILayout.BeginVertical();
                        SirenixEditorGUI.BeginBox();

                        Rect buttonRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                        EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Orbit);

                        if (fieldStates.IsNotLast && buttonRect.Contains(Event.current.mousePosition)) {
                            if (GUI.Button(buttonRect, $@"{time - fieldStates.Next.time:ss\.fff}", timeLabelStyle))
                                frame.OpenAsset();
                        }
                        else {
                            if (GUI.Button(buttonRect, $@"{time:mm\:ss\.fff}", timeLabelStyle))
                                frame.OpenAsset();
                        }

                        t.Draw(false);

                        SirenixEditorGUI.EndBox();
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

    }

}
#endif