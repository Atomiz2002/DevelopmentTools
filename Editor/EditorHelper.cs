#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Editor.Editor.Extensions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using Object = UnityEngine.Object;
#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using DevelopmentTools.Editor.Editor.Debugging;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
#endif

namespace DevelopmentTools.Editor.Editor {

    public static class EditorHelper {

        public static void StartRecordingChange() {
            EditorGUI.BeginChangeCheck();
        }

        public static void EndRecordingComponentChange(MonoBehaviour component) {
            EditorGUI.EndChangeCheck();
            Undo.RecordObject(component, "Changed " + component.name);

            EditorUtility.SetDirty(component);
        }

        public static void EndRecordingGameObjectChange(GameObject GO) {
            EditorGUI.EndChangeCheck();
            Undo.RecordObject(GO, "Changed " + GO.name);

            EditorUtility.SetDirty(GO);
        }

        public static List<UnityEvent> ExtractUnityEvents(MonoBehaviour component, string propertyName) {
            List<UnityEvent> output = new List<UnityEvent>();

            UnityEvent newEvent = new UnityEvent();

            SerializedObject   so             = new SerializedObject(component);
            SerializedProperty propertyEvents = so.FindProperty(propertyName);

            if (propertyEvents.isArray) {
                for (int i = 0; i < propertyEvents.arraySize; i++) {
                    List<UnityEvent> results = ExtractEvents(propertyEvents.GetArrayElementAtIndex(i));
                    output = output.Concat(results).ToList();
                }
            }
            else {
                List<UnityEvent> results = ExtractEvents(propertyEvents);
                output = output.Concat(results).ToList();
            }

            return output;
        }

        private static List<UnityEvent> ExtractEvents(SerializedProperty eventsProperty) {
            List<UnityEvent> list = new List<UnityEvent>();

            SerializedProperty persistentCalls = eventsProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            for (int i = 0; i < persistentCalls.arraySize; ++i) {
                UnityEvent newEvent = new UnityEvent();

                Object     target     = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Target").objectReferenceValue;
                string     methodName = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_MethodName").stringValue;
                MethodInfo method     = null;

                try {
                    method = target.GetType().GetMethod(methodName, flags); // TODO flags?
                }
                catch {
                    if (target == null)
                        continue;

                    foreach (MethodInfo info in target.GetType().GetMethods(flags).Where(x => x.Name == methodName)) {
                        ParameterInfo[] _params = info.GetParameters();

                        if (_params.Length < 2) {
                            method = info;
                        }
                    }
                }

                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length > 0) {
                    switch (parameters[0].ParameterType.Name) {
                        case nameof(Boolean):
                            bool              bool_value   = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue;
                            UnityAction<bool> bool_execute = Delegate.CreateDelegate(typeof(UnityAction<bool>), target, methodName) as UnityAction<bool>;
                            UnityEventTools.AddBoolPersistentListener(
                                newEvent,
                                bool_execute,
                                bool_value
                            );

                            break;
                        case nameof(Int32):
                            int              int_value   = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_IntArgument").intValue;
                            UnityAction<int> int_execute = Delegate.CreateDelegate(typeof(UnityAction<int>), target, methodName) as UnityAction<int>;
                            UnityEventTools.AddIntPersistentListener(
                                newEvent,
                                int_execute,
                                int_value
                            );

                            break;
                        case nameof(Single):
                            float              float_value   = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_FloatArgument").floatValue;
                            UnityAction<float> float_execute = Delegate.CreateDelegate(typeof(UnityAction<float>), target, methodName) as UnityAction<float>;
                            UnityEventTools.AddFloatPersistentListener(
                                newEvent,
                                float_execute,
                                float_value
                            );

                            break;
                        case nameof(String):
                            string              str_value   = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_StringArgument").stringValue;
                            UnityAction<string> str_execute = Delegate.CreateDelegate(typeof(UnityAction<string>), target, methodName) as UnityAction<string>;
                            UnityEventTools.AddStringPersistentListener(
                                newEvent,
                                str_execute,
                                str_value
                            );

                            break;
                        case nameof(System.Object):
                            Object              obj_value   = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_ObjectArgument").objectReferenceValue;
                            UnityAction<Object> obj_execute = Delegate.CreateDelegate(typeof(UnityAction<Object>), target, methodName) as UnityAction<Object>;
                            UnityEventTools.AddObjectPersistentListener(
                                newEvent,
                                obj_execute,
                                obj_value
                            );

                            break;
                        default:
                            UnityAction void_execute = Delegate.CreateDelegate(typeof(UnityAction), target, methodName) as UnityAction;
                            UnityEventTools.AddPersistentListener(
                                newEvent,
                                void_execute
                            );

                            break;
                    }
                }
                // no params
                else {
                    bool        bool_value   = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue;
                    UnityAction bool_execute = Delegate.CreateDelegate(typeof(UnityAction), target, methodName) as UnityAction;
                    UnityEventTools.AddVoidPersistentListener(
                        newEvent,
                        bool_execute
                    );
                }

                list.Add(newEvent);
            }

            return list;
        }

        public static bool UnityEventsProblemsCheck(UnityEvent[] events) {
            if (events != null && events.Length > 0) {
                foreach (UnityEvent unityEvent in events) {
                    if (UnityEventsProblemsCheck(unityEvent))
                        return true;
                }
            }

            return false;
        }

        public static bool UnityEventsProblemsCheck(UnityEvent unityEvent) {
            if (unityEvent != null && unityEvent.GetPersistentEventCount() > 1)
                return true;

            return false;
        }

        public static AssetReferenceGameObject GameObjectRefToAddressableRef(GameObject prefab) {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string guid, out long localID);
            return new AssetReferenceGameObject(guid);
        }

        public static void AddSymbol(string symbol) {
            NamedBuildTarget buildTarget    = EditorUserBuildSettings.selectedBuildTargetGroup.ToNamed();
            string           currentDefines = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            List<string>     defines        = new(currentDefines.Split(';'));

            if (!defines.Contains(symbol))
                defines.Add(symbol);

            PlayerSettings.SetScriptingDefineSymbols(buildTarget, string.Join(";", defines.ToArray()));
        }

        public static void RemoveSymbol(string symbol) {
            NamedBuildTarget buildTarget    = EditorUserBuildSettings.selectedBuildTargetGroup.ToNamed();
            string           currentDefines = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            List<string>     defines        = new(currentDefines.Split(';'));

            if (defines.Contains(symbol))
                defines.Remove(symbol);

            PlayerSettings.SetScriptingDefineSymbols(buildTarget, string.Join(";", defines.ToArray()));
        }

        public static string[] GetDefineSymbols() {
            NamedBuildTarget targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup.ToNamed();
            PlayerSettings.GetScriptingDefineSymbols(targetGroup, out string[] symbols);
            return symbols;
        }

        public static bool IsSymbolDefined(string symbol) => GetDefineSymbols().Contains(symbol);

        public static void GUILayoutLine() {
            GUILayout.Space(10);
            Rect rect3 = EditorGUILayout.GetControlRect(false, 2);
            rect3.height = 2;
            EditorGUI.DrawRect(rect3, Color.gray);
            GUILayout.Space(10);
        }

        public static void DrawDropZone<T>(Rect rect, Action<T[]> onAcceptDrag) {
            Event evt = Event.current;

            if (!rect.Contains(evt.mousePosition))
                return;

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Link;

            if (evt.type == EventType.DragPerform && DragAndDrop.objectReferences.All(x => x is T)) {
                DragAndDrop.AcceptDrag();
                onAcceptDrag.SafeInvoke(DragAndDrop.objectReferences.Cast<T>().ToArray());
            }

            evt.Use();
        }

        public static void DrawDashedBorder(Rect rect, float dashLength, float gapLength, float thickness, Color color) {
            Handles.BeginGUI();
            Handles.color = color;
            DrawDashedLine(new(rect.x, rect.y), new(rect.x + rect.width, rect.y), dashLength, gapLength, thickness);
            DrawDashedLine(new(rect.x + rect.width, rect.y), new(rect.x + rect.width, rect.y + rect.height), dashLength, gapLength, thickness);
            DrawDashedLine(new(rect.x + rect.width, rect.y + rect.height), new(rect.x, rect.y + rect.height), dashLength, gapLength, thickness);
            DrawDashedLine(new(rect.x, rect.y + rect.height), new(rect.x, rect.y), dashLength, gapLength, thickness);
            Handles.EndGUI();
        }

        private static void DrawDashedLine(Vector3 start, Vector3 end, float dashLength, float gapLength, float thickness) {
            float   distance      = Vector3.Distance(start, end);
            Vector3 direction     = (end - start).normalized;
            float   currentLength = 0f;

            while (currentLength < distance) {
                Vector3 dashStart       = start + direction * currentLength;
                float   remainingLength = distance - currentLength;
                float   segmentLength   = Mathf.Min(dashLength, remainingLength);
                Vector3 dashEnd         = dashStart + direction * segmentLength;
                Handles.DrawAAPolyLine(thickness, dashStart, dashEnd);
                currentLength += dashLength + gapLength;
            }
        }

        public static Color BackgroundColor(bool selectedBackground = false, bool activeSelection = false) =>
            (selectedBackground
                ? activeSelection
                    ? EditorGUIUtility.isProSkin ? new(0.17f, 0.36f, 0.53f) : new Color(0.24f, 0.49f, 0.91f) // Active
                    : EditorGUIUtility.isProSkin
                        ? new(0.30f, 0.30f, 0.30f)
                        : new Color(0.68f, 0.68f, 0.68f) // Inactive
                : EditorGUIUtility.isProSkin
                    ? new(0.22f, 0.22f, 0.22f)
                    : new Color(0.76f, 0.76f, 0.76f)) // Default
            * GUI.color; // Handle PlayMode tint, GUI.enabled, etc.

        public static void DrawColoredTexture(Rect rect, Color color) {
            Color prevColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = prevColor;
        }

        public static readonly Dictionary<string, Texture> cachedIcons = new();

        public static void SetIcon(this Object asset, Texture icon) =>
            cachedIcons[AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(asset)).ToString()] = icon;

        public static Texture GetIcon(this Object asset) {
            if (cachedIcons.TryGetValue(AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(asset)).ToString(), out Texture icon))
                return icon;

            return EditorGUIUtility.ObjectContent(asset, asset.n()?.GetType())?.image;
        }

        public static bool IsConsoleFocused() => EditorWindow.focusedWindow.n()?.GetType().Name == "ConsoleWindow";

        public static void FocusConsole() => EditorApplication.ExecuteMenuItem("Window/General/Console");

        public static void NotifyGameView(string notification, float duration = 1) =>
            EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.PlayModeView")).ShowNotification(new(notification), duration);

#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR

        public static void DrawTabbedList(IEnumerable<string> tabs) {
            // TODO take from DebugLoggerEditor
        }

#endif

    }

}

#endif