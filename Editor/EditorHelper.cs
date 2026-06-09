using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Editor.Debugging.RealtimeDebugger;
using DevelopmentTools.Editor.Extensions;
using JetBrains.Annotations;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif

namespace DevelopmentTools.Editor {

    public static class EditorHelper {

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

        public static void DrawDropZone<T>(Rect rect, Action<T[]> onAcceptDrag) where T : Object {
            Event evt = Event.current;

            if (!rect.Contains(evt.mousePosition))
                return;

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Link;

            if (evt.type == EventType.DragPerform && DragAndDrop.objectReferences.All(x => x.Is<T>())) {
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

        public static void DrawUnityObjectHeader(this object obj, bool vertical = false) {
            Object o = obj as Object;

            if (!o)
                return;

            if (!Event.current.control)
                return;

            if (vertical)
                EditorGUILayout.BeginVertical();
            else
                EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

            if (GUILayout.Button("Select")) o.SelectAndPing();
            if (GUILayout.Button("Ping")) o.Ping();

            if (obj.OpenScript(out Action open) && GUILayout.Button("Open Script"))
                open.SafeInvoke();

            GUILayout.FlexibleSpace();

            if (vertical)
                EditorGUILayout.EndVertical();
            else
                EditorGUILayout.EndHorizontal();
        }

        // todo can return incorrect line
        private static bool OpenScript(this object obj, out Action open, string target = null) {
            open = () => {};
            MonoScript monoScript;

            switch (obj) {
                case MonoBehaviour mb:    monoScript = MonoScript.FromMonoBehaviour(mb); break;
                case ScriptableObject so: monoScript = MonoScript.FromScriptableObject(so); break;
                default:                  return false;
            }

            string targetMatchPattern = @$"{target ?? monoScript.name}(?!\w)";

            string[] lines      = monoScript.text.Split('\n');
            int      targetLine = lines.IndexOf(lines.First(line => Regex.IsMatch(line, targetMatchPattern))) + 1;

            open = () => AssetDatabase.OpenAsset(monoScript, targetLine);
            return true;
        }

        #region Get/Set Icon

        private static readonly Dictionary<GlobalObjectId, (IHaveIconPreview icon, long timestamp)> cachedIcons = new();

        public static GlobalObjectId GlobalId(this Object obj)                        => GlobalObjectId.GetGlobalObjectIdSlow(obj);
        public static GlobalObjectId GlobalId(this Object obj, out GlobalObjectId id) => id = GlobalObjectId.GetGlobalObjectIdSlow(obj);

        public static void SetIcon(this Object obj, [CanBeNull] IHaveIconPreview icon , bool acceptNull = false) {
            if (!obj)
                return;

            if (icon != null && icon.Icon)
                cachedIcons[obj.GlobalId()] = (icon, DateTime.Now.Ticks);
            else if (acceptNull)
                cachedIcons[obj.GlobalId()] = (null, DateTime.Now.Ticks);
        }

        [CanBeNull]
        public static IHaveIconPreview GetIcon(this Object obj, bool fallback = false) {
            if (!obj)
                return null;

            GlobalObjectId id = obj.GlobalId();

            if (cachedIcons.TryGetValue(id, out (IHaveIconPreview icon, long timestamp) c)) {
                if (c.icon != null)
                    return c.icon;

                if (DateTime.Now.Ticks - c.timestamp > TimeSpan.FromSeconds(5).Ticks)
                    cachedIcons.Remove(id);
            }
            else {
                cachedIcons[id] = (null, DateTime.Now.Ticks);
            }

            if (fallback) {
                Texture texture = EditorGUIUtility.ObjectContent(obj, obj.GetType()).n()?.image;
                if (texture)
                    return new IconPreview(texture, Color.white);

                texture = EditorGUIUtility.GetIconForObject(obj);
                if (texture)
                    return new IconPreview(texture, Color.white);

                texture = AssetPreview.GetAssetPreview(obj);
                if (texture)
                    return new IconPreview(texture, Color.white);
            }

            return null;
        }

        public static void DrawIcon(this IHaveIconPreview icon, Rect rect, ScaleMode scaleMode, bool selectedBackground = false, bool activeSelection = false) {
            if (icon == null)
                return;

            DrawColoredTexture(rect, BackgroundColor(selectedBackground, activeSelection));

            Color c = GUI.color;
            GUI.color = icon.Color;

            GUI.DrawTexture(rect, icon.Icon, scaleMode);

            GUI.color = c;
        }

        [InitializeOnLoadMethod]
        private static void ClearCachedIcons() {
            SceneHierarchyHooks.addItemsToSceneHeaderContextMenu += (menu, _) => {
                menu.AddItem(new("Clear Cached Icons"), false, () => {
                    cachedIcons.Clear();
                });
            };
        }

        #endregion

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