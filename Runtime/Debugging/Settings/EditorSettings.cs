using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Editor_.Toolbar_Injections;
using JetBrains.Annotations;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if NEWTONSOFT_JSON
using System.Collections.Generic;
using DevelopmentTools.Editor_.CopyPaste;
using Newtonsoft.Json;
#endif

#if ENABLE_LOGS
#endif

namespace DevelopmentTools.Debugging.Settings {

    public static class EditorSettings {

        public const string MenuGroupPath = "Window/Atomiz/";

#if !SIMULATE_BUILD

        [InitializeOnLoadMethod]
        public static void Initialize() =>
            ToolbarGUIInjector.AddToolbarPopupButton(ToolbarGUIInjector.ToolbarSide.LeftOfPlay, "Editor Settings", 115, DrawEditorSettingsGUI, 500, 0, 101);

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() =>
            new("Atomiz/Editor Settings", SettingsScope.Project) {
                guiHandler = _ => DrawEditorSettingsGUI()
            };

        private static void DrawEditorSettingsGUI() {
            EditorSettingsPreset preset = PresetGUID.GUIDToPath().LoadAsset<EditorSettingsPreset>();
            PresetGUID = EditorGUILayout.ObjectField("Preset", preset, typeof(EditorSettingsPreset), false).GetAssetPath().PathToGUID();
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(preset);
            preset?.Apply();

            SirenixEditorGUI.Title("Debug Logger", null, TextAlignment.Left, true);
            DebugLogger.ShowOnPlay      = EditorGUILayout.Toggle(nameof(DebugLogger.ShowOnPlay).SplitPascalCase(), DebugLogger.ShowOnPlay);
            DebugLogger.MergeDuplicates = EditorGUILayout.Toggle(nameof(DebugLogger.MergeDuplicates).SplitPascalCase(), DebugLogger.MergeDuplicates);
            EditorGUILayout.Space();

            SirenixEditorGUI.Title("On Compile", null, TextAlignment.Left, true);
            OnCompile.FocusOnCompile = EditorGUILayout.Toggle(nameof(OnCompile.FocusOnCompile).SplitPascalCase(), OnCompile.FocusOnCompile);
            OnCompile.PlayOnCompile  = EditorGUILayout.Toggle(nameof(OnCompile.PlayOnCompile).SplitPascalCase(), OnCompile.PlayOnCompile);
            EditorGUI.BeginDisabledGroup(!OnCompile.PlayOnCompile);
            OnCompile.FocusOnPlay = EditorGUILayout.Toggle(nameof(OnCompile.FocusOnPlay).SplitPascalCase(), OnCompile.FocusOnPlay);
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginHorizontal();

#if NEWTONSOFT_JSON
            if (GUILayout.Button("Copy")) {
                new Dictionary<string, object> {
                    { nameof(DebugLogger.ShowOnPlay), DebugLogger.ShowOnPlay },
                    { nameof(DebugLogger.MergeDuplicates), DebugLogger.MergeDuplicates }
                }.CopyToClipboard();
            }

            if (GUILayout.Button("Paste")) {
                try {
                    Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(EditorGUIUtility.systemCopyBuffer);

                    if (data.TryGetValue(nameof(DebugLogger.ShowOnPlay), out object debugLoggerShowOnPlay))
                        DebugLogger.ShowOnPlay = Convert.ToBoolean(debugLoggerShowOnPlay);

                    if (data.TryGetValue(nameof(DebugLogger.MergeDuplicates), out object debugLoggerMergeDuplicates))
                        DebugLogger.MergeDuplicates = Convert.ToBoolean(debugLoggerMergeDuplicates);

                    if (data.TryGetValue(nameof(OnCompile.FocusOnCompile), out object onCompileFocus))
                        OnCompile.FocusOnCompile = Convert.ToBoolean(onCompileFocus);

                    if (data.TryGetValue(nameof(OnCompile.PlayOnCompile), out object onCompilePlay))
                        OnCompile.PlayOnCompile = Convert.ToBoolean(onCompilePlay);

                    if (data.TryGetValue(nameof(OnCompile.FocusOnPlay), out object onCompileFocusOnPlay))
                        OnCompile.FocusOnPlay = Convert.ToBoolean(onCompileFocusOnPlay);
                }
                catch (Exception) {
#if ENABLE_LOGS
                    $"Invalid format:\n{EditorGUIUtility.systemCopyBuffer}".LogEx();
#endif
                }
            }
#endif

            EditorGUILayout.EndHorizontal();
        }

        public static GUID PresetGUID {
            get => new(EditorPrefs.GetString(nameof(PresetGUID)));
            set => EditorPrefs.SetString(nameof(PresetGUID), value.ToString());
        }

#endif

        public static class DebugLogger {

            public static bool ShowOnPlay {
                get => EditorPrefs.GetBool(nameof(ShowOnPlay));
                set => EditorPrefs.SetBool(nameof(ShowOnPlay), value);
            }

            public static bool MergeDuplicates {
                get => EditorPrefs.GetBool(nameof(MergeDuplicates));
                set => EditorPrefs.SetBool(nameof(MergeDuplicates), value);
            }

        }

        public static class OnCompile {

            public static bool FocusOnCompile {
                get => EditorPrefs.GetBool(nameof(FocusOnCompile));
                set {
                    EditorPrefs.SetBool(nameof(FocusOnCompile), value);

                    if (value)
                        AssemblyReloadEvents.afterAssemblyReload += DelayedFocusUnity;
                }
            }

            public static bool PlayOnCompile {
                get => EditorPrefs.GetBool(nameof(PlayOnCompile));
                set => EditorPrefs.SetBool(nameof(PlayOnCompile), value);
            }

            public static bool FocusOnPlay {
                get => EditorPrefs.GetBool(nameof(FocusOnPlay));
                set => EditorPrefs.SetBool(nameof(FocusOnPlay), value);
            }

            private static IDisposable eventGameplayStateChanged;

            [InitializeOnLoadMethod]
            public static void Initialize() {
                if (FocusOnCompile)
                    AssemblyReloadEvents.afterAssemblyReload += DelayedFocusUnity;

                if (PlayOnCompile) {
                    EditorApplication.isPlaying = true;

                    SceneManager.sceneLoaded += OnSceneLoaded;
                }
            }

            private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
                if (FocusOnPlay && scene.buildIndex == 2) {
                    FocusUnity();
                    SceneManager.sceneLoaded -= OnSceneLoaded;
                }
            }

            [DllImport("user32.dll")]   private static extern uint   GetWindowThreadProcessId(IntPtr hWnd);
            [DllImport("user32.dll")]   private static extern IntPtr GetForegroundWindow();
            [DllImport("kernel32.dll")] private static extern uint   GetCurrentThreadId();
            [DllImport("user32.dll")]   private static extern bool   AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
            [DllImport("user32.dll")]   private static extern bool   BringWindowToTop(IntPtr hWnd);
            [DllImport("user32.dll")]   private static extern bool   ShowWindow(IntPtr hWnd, IntPtr nCmdShow);

            private static void DelayedFocusUnity() => EditorApplication.delayCall += FocusUnity;

            public static void FocusUnity() {
                foreach (Process process in Process.GetProcesses()) {
                    if (process.HasExited || process.ProcessName != "Unity")
                        continue;

                    IntPtr hWnd       = process.MainWindowHandle;
                    uint   foreThread = GetWindowThreadProcessId(GetForegroundWindow());
                    uint   appThread  = GetCurrentThreadId();
                    IntPtr SW_SHOW    = new(5);

                    if (foreThread != appThread) {
                        AttachThreadInput(foreThread, appThread, true);
                        BringWindowToTop(hWnd);
                        ShowWindow(hWnd, SW_SHOW);
                        AttachThreadInput(foreThread, appThread, false);
                    }
                    else {
                        BringWindowToTop(hWnd);
                        ShowWindow(hWnd, SW_SHOW);
                    }
                }
            }

            // [MenuItem("Test/Test")]
            // public static async void Test() {
            //     await Task.Delay(TimeSpan.FromSeconds(2));
            //     await Task.Run(FocusProcess);
            // }

        }

        [CanBeNull]
        public static EditorWindow TryFocusWindow(string name) {
            if (Selection.activeObject?.name == name)
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