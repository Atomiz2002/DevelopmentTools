#if DEVELOPMENT_TOOLS_ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevelopmentEssentials.Editor.Extensions.Unity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using EditorSettings = DevelopmentTools.Editor.Debugging.Settings.EditorSettings;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DevelopmentTools.Editor.Debugging.DebugFields {

    [CreateAssetMenu(fileName = "Debug Fields", menuName = "Development Tools/Debug Fields")]
    public class DebugFields : SerializedScriptableObject {

#if UNITY_EDITOR

        [ListDrawerSettings(IsReadOnly = true, DefaultExpandedState = true, ShowFoldout = false, ShowItemCount = false)]
        public List<DebugFieldsValues> debugFields = new();

        private IDisposable eventDebugField;

        private static DebugFields instance;

        private static DebugFields Instance {
            get {
                if (instance)
                    return instance;

                instance = AssetDatabase.FindAssets($"t:{nameof(DebugFields)}")
                    .FirstOrDefault()?
                    .GUIDToPath()
                    .LoadAsset<DebugFields>();

                if (instance)
                    return instance;

                instance      = CreateInstance<DebugFields>();
                instance.name = $"{nameof(DebugFields).ToTitleCase()} (Unsaved)";
                return instance;
            }
        }

        private void OnEnable() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable() {
            eventDebugField?.Dispose();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        [MenuItem(EditorSettings.MenuGroupPath + "Debug Fields")]
        public static void Show() {
            if (!EditorSettings.TryFocusWindow(Instance.name))
                EditorUtility.OpenPropertyEditor(instance);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state) {
            switch (state) {
                case PlayModeStateChange.EnteredPlayMode:
                    Instance.debugFields.Clear();
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    clearConfirm = 0;
                    break;
            }
        }

        private byte                    clearConfirm;
        private CancellationTokenSource clearConfirmCancellationTokenSource;

        [DisableInEditorMode]
        [Button("@clearConfirm == 0 ? \"Clear\" : \"<color=#ff0000>Are you sure?</color>\"")]
        [GUIColor("@clearConfirm == 0 ? GUI.color : Color.red")]
        private void Clear() {
            clearConfirm++;

            clearConfirmCancellationTokenSource?.Cancel();
            clearConfirmCancellationTokenSource = new();

            Task.Run(async () => {
                try {
                    await Task.Delay(TimeSpan.FromSeconds(1), clearConfirmCancellationTokenSource.Token);
                    clearConfirm = 0;
                }
                catch (TaskCanceledException) {}
            });

            if (clearConfirm < 2)
                return;

            clearConfirmCancellationTokenSource.Cancel();
            clearConfirm = 0;
            Instance.debugFields.Clear();
        }

        public static void AddDebugField(string name, string value, Texture2D icon = null, StackTrace stackTrace = null) {
            DebugFieldsValues debugField = Instance.debugFields.Find(fieldValue => fieldValue.FieldName == name);

            if (debugField == null)
                Instance.debugFields.Add(debugField = new(name));

            debugField.AddValue(value, icon, stackTrace);
        }

#endif

    }

}
#endif