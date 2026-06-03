#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Settings;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
#if DEVELOPMENT_TOOLS_EDITOR_UNI_TASK
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
#else
using System.Threading.Tasks;
#endif

namespace DevelopmentTools.Editor.Debugging.DebugFields {

    public class DebugFields : OdinEditorWindow {

#if UNITY_EDITOR && !SIMULATE_BUILD

        private static bool autoOpenedWindowOnce;

        [ListDrawerSettings(IsReadOnly = true, DefaultExpandedState = true, ShowFoldout = false, ShowItemCount = false, DraggableItems = false)]
        public List<DebugFieldsValues> debugFields = new();

        private IDisposable eventDebugField;

        private static DebugFields instance;

        private static DebugFields Instance {
            get {
                if (instance)
                    return instance;

                if (autoOpenedWindowOnce)
                    return new();

                autoOpenedWindowOnce = true;
                return instance = GetWindow(true);
            }
        }

        private static DebugFields GetWindow(bool focus) => GetWindow<DebugFields>(nameof(DebugFields).ToTitleCase(), focus);

        protected override void OnEnable() {
            base.OnEnable();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected override void OnDisable() {
            base.OnDisable();

            eventDebugField?.Dispose();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        protected override void OnImGUI() {
            base.OnImGUI();
            Repaint();
        }

        [MenuItem(EngineSettings.MenuGroupPath + "Debug Fields #&d")]
        public static void TryShowWindow() => GetWindow(true);

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

        [PropertyOrder(float.MinValue)]
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
            Instance.debugFields.Clear();
        }

        public static void AddDebugField([CanBeNull] string name, string value, Texture2D icon = null, StackTrace stackTrace = null) {
            name ??= stackTrace.SafeString(new StackTrace(2, true).ToString());

            DebugFieldsValues debugField = Instance.debugFields.Find(fieldValue => fieldValue.FieldName == name);

            if (debugField == null)
                Instance.debugFields.Add(debugField = new(name));

            debugField.AddValue(value, icon, stackTrace);
        }

#endif

    }

}
#endif