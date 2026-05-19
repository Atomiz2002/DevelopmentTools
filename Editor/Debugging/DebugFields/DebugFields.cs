#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DevelopmentTools.Settings;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
#if DEVELOPMENT_TOOLS_EDITOR_UNI_TASK
using Cysharp.Threading.Tasks;

#else
using System.Threading.Tasks;
#endif

namespace DevelopmentTools.DevelopmentTools.Editor.Debugging.DebugFields {

    [CreateAssetMenu(fileName = "Debug Fields", menuName = "Development Tools/Debug Fields")]
    public class DebugFields : SerializedScriptableObject {

#if UNITY_EDITOR && !SIMULATE_BUILD

        private static bool autoOpenedWindowOnce;

        [ListDrawerSettings(IsReadOnly = true, DefaultExpandedState = true, ShowFoldout = false, ShowItemCount = false)]
        public List<DebugFieldsValues> debugFields = new();

        private IDisposable eventDebugField;

        private static DebugFields instance;

        private void OnEnable() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            instance                               =  this;
        }

        private void OnDisable() {
            eventDebugField?.Dispose();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        [MenuItem(EngineSettings.MenuGroupPath + "Debug Fields")]
        public static void TryShowWindow() => EngineSettings.TryShowWindow(instance);

        private void OnPlayModeStateChanged(PlayModeStateChange state) {
            switch (state) {
                case PlayModeStateChange.EnteredPlayMode:
                    instance.debugFields.Clear();
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

#if DEVELOPMENT_TOOLS_EDITOR_UNI_TASK
            UniTask.Void(async () => {
                try {
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: clearConfirmCancellationTokenSource.Token);
                    clearConfirm = 0;
                }
                catch {}
            });
#else
            Task.Run(async () => {
                try {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken: clearConfirmCancellationTokenSource.Token);
                    clearConfirm = 0;
                }
                catch {}
            });
#endif

            if (clearConfirm < 2)
                return;

            clearConfirmCancellationTokenSource.Cancel();
            clearConfirm = 0;
            instance.debugFields.Clear();
        }

        public static void AddDebugField(string name, string value, Texture2D icon = null, StackTrace stackTrace = null) {
            if (!autoOpenedWindowOnce) {
                TryShowWindow();
                autoOpenedWindowOnce = true;
            }

            DebugFieldsValues debugField = instance.debugFields.Find(fieldValue => fieldValue.FieldName == name);

            if (debugField == null)
                instance.debugFields.Add(debugField = new(name));

            debugField.AddValue(value, icon, stackTrace);
        }

#endif

    }

}
#endif