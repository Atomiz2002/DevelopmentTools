#if UNITY_EDITOR
using System;
using DevelopmentEssentials.Extensions.Unity;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Runtime.Settings {

    public static class EngineSettings {

        public const string MenuGroupPath = "Window/Atomiz/";
        public const string ENABLE_LOGS   = "ENABLE_LOGS";

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

        public static void TryShowWindow(Object instance, string name = null) {
            name ??= instance.name;
            if (!TryFocusWindow(name))
                EditorUtility.OpenPropertyEditor(instance);
        }

    }

}
#endif