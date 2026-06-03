#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentTools.Settings;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Editor.Debugging.RealtimeDebugger {

    public class RealtimeDebuggerWindow : OdinEditorWindow {

        private const  BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static bool         autoOpenedWindowOnce;

        private static readonly Dictionary<object, Func<object, bool>> _cachedProperties = new();

        [OdinSerialize]
        [ListDrawerSettings(
            OnBeginListElementGUI = nameof(DrawCachedProperties1),
            OnEndListElementGUI = nameof(DrawCachedProperties2))]
        private List<object> cachedProperties;

        private static RealtimeDebuggerWindow Instance => GetWindow<RealtimeDebuggerWindow>();

        [MenuItem(EngineSettings.MenuGroupPath + "Realtime Debugger")]
        public static void TryShowWindow() => Instance.name = "Realtime Debugger";

        public static void CacheProperty<T>(T obj, Func<T, bool> condition = null, bool openWindow = false) {
            if (!_cachedProperties.ContainsKey(obj))
                Instance.cachedProperties.Add(obj);

            _cachedProperties[obj] = o => condition.InvokeSafe((T) o);

            if (openWindow)
                TryShowWindow();
        }

        protected override void OnEnable() {
            base.OnEnable();

            SceneManager.sceneLoaded -= CachePropertiesOnSceneLoad;
            SceneManager.sceneLoaded += CachePropertiesOnSceneLoad;
        }

        protected override void OnDisable() {
            base.OnDisable();

            SceneManager.sceneLoaded -= CachePropertiesOnSceneLoad;
        }

        private void CachePropertiesOnSceneLoad(Scene scene, LoadSceneMode loadSceneMode) {
            CacheProperties();

            if (autoOpenedWindowOnce || !cachedProperties.Any())
                return;

            TryShowWindow();
            autoOpenedWindowOnce = true;
        }

        protected void OnInspectorUpdate() {
            List<object> list = new();

            foreach ((object key, Func<object, bool> condition) in _cachedProperties)
                if (condition.InvokeSafe(key))
                    list.Add(key);

            cachedProperties = list;
            Repaint();
        }

        private void DrawCachedProperties1(int i) {
            object obj = cachedProperties[i];
            Object o   = obj as Object;

            if (!o && obj == null)
                return;

            SirenixEditorGUI.BeginBox();

            EditorGUILayout.BeginHorizontal();
            o.DrawUnityObjectHeader();
        }

        private void DrawCachedProperties2(int i) {
            object obj = cachedProperties[i];
            Object o   = obj as Object;

            if (!o && obj == null)
                return;

            EditorGUILayout.EndHorizontal();

            SirenixEditorGUI.EndBox();
        }

        [Button]
        private void CacheProperties() {
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                bool         debugBehaviour = behaviour.GetType().IsDefined<RealtimeDebugAttribute>(true);
                MemberInfo[] members        = behaviour.GetType().GetMembers(Flags);

                if (!debugBehaviour && !members.Any(m => m.TryGet(out RealtimeDebugAttribute _)))
                    continue;

                cachedProperties.Add(behaviour);
            }

            cachedProperties.Reverse();
        }

    }

}
#endif