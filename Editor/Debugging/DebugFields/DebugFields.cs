#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentTools.Settings;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.DevelopmentTools.Editor.Debugging.DebugFields {

    [CreateAssetMenu(fileName = "Debug Fields", menuName = "Development Tools/Debug Fields")]
    public class DebugFields : SerializedScriptableObject {

#if UNITY_EDITOR && !SIMULATE_BUILD

        private static bool autoOpenedWindowOnce;

        [ListDrawerSettings(IsReadOnly = true, DefaultExpandedState = true, ShowFoldout = false, ShowItemCount = false)]
        public List<DebugFieldsValues> debugFields = new();

        private IDisposable eventDebugField;

        private static DebugFields instance;

        private static DebugFields Instance {
            get {
                if (instance)
                    return instance;

                instance = typeof(DebugFields).FindAssets<DebugFields>().FirstOrDefault();

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

        [MenuItem(EngineSettings.MenuGroupPath + "Debug Fields")]
        public static void TryShowWindow() => EngineSettings.TryShowWindow(Instance);

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

        [Button("@clearConfirm == 0 ? \"Clear\" : \"<color=#ff0000>Are you sure?</color>\"")]
        [GUIColor("@clearConfirm == 0 ? GUI.color : Color.red")]
        private void Clear() {
            clearConfirm++;

            clearConfirmCancellationTokenSource?.Cancel();
            clearConfirmCancellationTokenSource = new();

            UniTask.Void(async () => {
                try {
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: clearConfirmCancellationTokenSource.Token);
                    clearConfirm = 0;
                }
                catch {}
            });

            if (clearConfirm < 2)
                return;

            clearConfirmCancellationTokenSource.Cancel();
            clearConfirm = 0;
            Instance.debugFields.Clear();
        }

        public static void AddDebugField(string name, string value, Texture2D icon = null, StackTrace stackTrace = null) {
            if (!autoOpenedWindowOnce) {
                TryShowWindow();
                autoOpenedWindowOnce = true;
            }

            DebugFieldsValues debugField = Instance.debugFields.Find(fieldValue => fieldValue.FieldName == name);

            if (debugField == null)
                Instance.debugFields.Add(debugField = new(name));

            debugField.AddValue(value, icon, stackTrace);
        }

#endif

    }

}
#endif