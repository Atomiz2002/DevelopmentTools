#if UNITY_EDITOR
using System.Collections.Generic;
using DevelopmentTools.Editor.Toolbar_Injections;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static DevelopmentTools.Settings.EngineSettings;
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_ADDRESSABLES
using UnityEditor.AddressableAssets.Settings;
#endif
#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using Sirenix.Utilities.Editor;
#endif

namespace DevelopmentTools.Editor.Settings {

    public class BuildSettings : EditorWindow {

        public static           string          Versions => $"Current Versions - Bundle: v{PlayerSettings.bundleVersion}  |  Android: v{PlayerSettings.Android.bundleVersionCode}  |  IOS: v{PlayerSettings.iOS.buildNumber}";
        private static readonly HashSet<string> symbols        = new() { EngineSettings.SIMULATE_BUILD, ENABLE_LOGS, ONLY_EXCEPTIONS };
        private static readonly HashSet<string> currentSymbols = new();

        [InitializeOnLoadMethod]
        public static void Initialize() =>
            ToolbarGUIInjector.AddToolbarPopupButton(ToolbarGUIInjector.ToolbarSide.LeftOfPlay, "Build Settings", 100, DrawGUI, 500, 0, 100, 5);

        [MenuItem(MenuGroupPath + "Build Settings", false, -10000)]
        public static void ShowWindow() {
            SettingsService.OpenProjectSettings("Atomiz/Build Settings");
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() =>
            new("Atomiz/Build Settings", SettingsScope.Project) { guiHandler = _ => DrawGUI() };

        private void OnGUI() {
            DrawGUI();
        }

        private static void DrawGUI() {
            GUI_Build();

            EditorHelper.GUILayoutLine();

            GUI_Version();


#if UNITY_WEBGL
        EditorHelper.GUILayoutLine();

        GUI_WebGL();
#endif

            EditorHelper.GUILayoutLine();

            GUI_Symbols();
        }

        private static void GUI_Build() {
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_ADDRESSABLES
            EditorGUI.BeginDisabledGroup(true);
            if (GUILayout.Button("Build Check (Alt + B)"))
                RunBuildCheck();

            EditorGUI.EndDisabledGroup();
#endif

            GUILayout.Label("Configure Build Target", EditorStyles.boldLabel);

            if (GUILayout.Button("Prepare for RELEASE build")) {
                PrepareForReleaseBuild();
            }

            if (GUILayout.Button("Prepare for TEST build")) {
                PrepareForDevelopmentBuild();
            }
        }

        private static void GUI_Version() {
            GUILayout.Label("Build Version", EditorStyles.boldLabel);

            GUILayout.Box(Versions, new GUIStyle(GUI.skin.box) {
                normal = {
                    textColor = Color.white
                },
                alignment    = TextAnchor.MiddleLeft,
                stretchWidth = true
            });

            if (GUILayout.Button("Increment Version: Patch")) {
                IncrementVersionPatch();
            }

            if (GUILayout.Button("Increment Version: Minor")) {
                IncrementVersionMinor();
            }
        }

        private static void GUI_WebGL() {
            GUILayout.Label("Configure WebGL Build", EditorStyles.boldLabel);

            if (GUILayout.Button("Crazy Games")) {
                WebGLSetupCrazyGames();
            }

            if (GUILayout.Button("Self Hosted")) {
                WebGLSetupSelfHosted();
            }
        }

        private static void GUI_Symbols() {
            bool     updatable    = false;
            GUIStyle greenButton  = new(GUI.skin.button) { normal = { textColor = Color.green }, hover  = { textColor = Color.green } };
            GUIStyle yellowButton = new(GUI.skin.button) { normal = { textColor = Color.yellow }, hover = { textColor = Color.yellow } };

#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
            SirenixEditorGUI.BeginVerticalList(); // just draws the background
#endif
            EditorGUILayout.Space(2);
            GUILayout.Label("Configure Symbols", EditorStyles.boldLabel);

            foreach (string symbol in symbols) {
#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
                if (DebugLogger.GetSymbols().Contains(symbol))
                    continue;
#endif

                EditorGUI.BeginDisabledGroup(!currentSymbols.Contains(ENABLE_LOGS) && symbol == ONLY_EXCEPTIONS);
                drawSymbolButton(symbol);
                EditorGUI.EndDisabledGroup();
            }

#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
            if (EditorHelper.IsSymbolDefined(ENABLE_LOGS)) {
                EditorHelper.GUILayoutLine();
                GUILayout.Label("DebugLogger Symbols", EditorStyles.boldLabel);

                foreach (string symbol in DebugLogger.GetSymbols())
                    drawSymbolButton(symbol);

                EditorGUILayout.Space(1);

                if (GUILayout.Button("Add All"))
                    foreach (string s in DebugLogger.GetSymbols())
                        currentSymbols.Add(s);

                if (GUILayout.Button("Remove All"))
                    foreach (string s in DebugLogger.GetSymbols())
                        currentSymbols.Remove(s);
            }
#endif

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!updatable);

            if (GUILayout.Button("Apply Updated Symbols", new GUIStyle(GUI.skin.button) { fontStyle = updatable ? FontStyle.Bold : FontStyle.Normal })) {
                foreach (string s in symbols)
                    EditorHelper.RemoveSymbol(s);

                foreach (string s in currentSymbols)
                    EditorHelper.AddSymbol(s);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(2);
#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
            SirenixEditorGUI.EndVerticalList();
#endif
            return;

            void drawSymbolButton(string symbol) {
                if (currentSymbols.Contains(symbol)) {
                    bool modified = !EditorHelper.IsSymbolDefined(symbol);
                    updatable |= modified;
                    if (GUILayout.Button($"Remove Symbol: {symbol}" + (modified ? "*" : ""), greenButton))
                        currentSymbols.Remove(symbol);
                }
                else {
                    bool modified = EditorHelper.IsSymbolDefined(symbol);
                    updatable |= modified;
                    if (GUILayout.Button($"Add Symbol: {symbol}" + (modified ? "*" : ""), yellowButton))
                        currentSymbols.Add(symbol);
                }
            }
        }

        public static void IncrementVersionPatch() {
            string newVersion = IncrementVersion(2);
            if (newVersion == null)
                return;

            SetNewVersion(newVersion);
        }

        private static void IncrementVersionMinor() {
            string newVersion = IncrementVersion(1);
            if (newVersion == null)
                return;

            SetNewVersion(newVersion);
        }

        private static void SetNewVersion(string newVersion) {
            if (newVersion == null)
                return;

            PlayerSettings.bundleVersion = newVersion;

            PlayerSettings.iOS.buildNumber = newVersion;
            PlayerSettings.Android.bundleVersionCode++;

            AssetDatabase.SaveAssets();
        }

        private static string IncrementVersion(int incrementPart) {
            string   oldVersion = PlayerSettings.bundleVersion;
            string[] parts      = oldVersion.Split('.');
            if (parts.Length != 3)
                return null;

            int major = int.Parse(parts[0]);
            int minor = int.Parse(parts[1]);
            int patch = int.Parse(parts[2]);

            if (incrementPart == 0)
                major++;
            else if (incrementPart == 1)
                minor++;
            else if (incrementPart == 2)
                patch++;

            string newVersion = $"{major}.{minor}.{patch}";

            if (EditorUtility.DisplayDialog(
                    "Confirm version change",
                    $"Are you sure you want to change the version from {oldVersion} to {newVersion}?",
                    "OK",
                    "Cancel")) {
                return newVersion;
            }

            return null;
        }

#if DEVELOPMENT_TOOLS_EDITOR_UNITY_ADDRESSABLES
        [Shortcut("Run Build check", KeyCode.B, ShortcutModifiers.Alt)]
        public static void RunBuildCheck() => AddressableAssetSettings.BuildPlayerContent();
#endif

        public static void PrepareForDevelopmentBuild() {
            EditorHelper.RemoveSymbol("SIMULATE_BUILD");
            EditorHelper.RemoveSymbol("ONLY_EXCEPTIONS");
            EditorHelper.AddSymbol("ENABLE_LOGS");

            EditorUserBuildSettings.development = true;

            const Il2CppCompilerConfiguration compilerConfiguration = Il2CppCompilerConfiguration.Debug;
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, compilerConfiguration);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android, compilerConfiguration);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.iOS, compilerConfiguration);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Standalone, compilerConfiguration);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void PrepareForReleaseBuild() {
            // symbols
            EditorHelper.RemoveSymbol("SIMULATE_BUILD");
            EditorHelper.RemoveSymbol("ONLY_EXCEPTIONS");
            EditorHelper.RemoveSymbol("ENABLE_LOGS");

            EditorUserBuildSettings.development = false;

            const Il2CppCompilerConfiguration compilerConfiguration = Il2CppCompilerConfiguration.Master;
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, compilerConfiguration);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android, compilerConfiguration);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.iOS, compilerConfiguration);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Standalone, compilerConfiguration);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void WebGLSetupCrazyGames() {
            // Add the CRAZY_GAMES define symbol.
            EditorHelper.AddSymbol("CRAZY_GAMES");

            // Set the WebGL template to Crazy_2020.
            PlayerSettings.WebGL.template = "PROJECT:Crazy_2020";

            // Enable the crazySDK.jslib for WebGL.
            string         pluginPath = "Assets/Plugins/crazySDK.jslib";
            PluginImporter importer   = AssetImporter.GetAtPath(pluginPath) as PluginImporter;
            importer.SetCompatibleWithPlatform(BuildTarget.WebGL, true);
            importer.SaveAndReimport();

            EditorUtility.DisplayDialog("Success", "WebGL settings applied for build type: CrazyGames", "Ok");
        }

        private static void WebGLSetupSelfHosted() {
            // Remove the CRAZY_GAMES define symbol.
            EditorHelper.RemoveSymbol("CRAZY_GAMES");

            // Set the WebGL template to TSH.
            PlayerSettings.WebGL.template = "PROJECT:TSH";

            // Exclude the crazySDK.jslib for WebGL.
            string         pluginPath = "Assets/Plugins/crazySDK.jslib";
            PluginImporter importer   = AssetImporter.GetAtPath(pluginPath) as PluginImporter;
            importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
            importer.SaveAndReimport();

            EditorUtility.DisplayDialog("Success", "WebGL settings applied for build type: Self Hosted", "Ok");
        }

    }

}
#endif