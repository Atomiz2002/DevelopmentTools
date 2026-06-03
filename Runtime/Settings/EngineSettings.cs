#if UNITY_EDITOR
using System;
using DevelopmentEssentials.Extensions.Unity;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Settings {

    public static class EngineSettings {

        /// <remarks>shortcuts: Ctrl <c>%</c>/<c>^</c>, Shift <c>#</c>, Alt <c>&amp;</c>, Key <c>_x</c> (_ if no modifiers)</remarks>
        public const string MenuGroupPath = "Window/Development Tools/";
        public const string ENABLE_LOGS     = "ENABLE_LOGS";
        public const string ONLY_EXCEPTIONS = "ONLY_EXCEPTIONS";

#if UNITY_EDITOR
        public static void TryShowWindow(Object instance, string name = null) {
            if (!instance)
                return;

            name ??= instance.name;
            if (!TryFocusWindow(name))
                EditorUtility.OpenPropertyEditor(instance);
        }

        [CanBeNull]
        public static EditorWindow TryFocusWindow(string name) {
            if (Selection.activeObject.n()?.name == name)
                return EditorWindow.GetWindow(Type.GetType("UnityEditor.InspectorWindow,UnityEditor"));

            foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
                if (window.titleContent.text == name) {
                    window.Focus();
                    return window;
                }

            return null;
        }
#endif

    }

}
#endif