#if UNITY_EDITOR
using DevelopmentTools.Editor.Editor_.Toolbar_Injections;
using DevelopmentTools.Editor.Extensions.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.ShortcutManagement;
using UnityEngine;
#if DEVELOPMENT_TOOLS_UNITY_ADDRESSABLES
using UnityEditor.AddressableAssets.Settings;
#endif

namespace DevelopmentTools.Editor.Debugging.Settings {

    public class BuildSettings : EditorWindow {

        public static           string   Versions => $"Current Versions - Bundle: v{PlayerSettings.bundleVersion}  |  Android: v{PlayerSettings.Android.bundleVersionCode}  |  IOS: v{PlayerSettings.iOS.buildNumber}";
        private static readonly string[] symbols = { "SIMULATE_BUILD", "DISABLESTEAMWORKS", "ONLY_EXCEPTIONS", "ENABLE_LOGS" };

        [InitializeOnLoadMethod]
        public static void Initialize() =>
            ToolbarGUIInjector.AddToolbarPopupButton(ToolbarGUIInjector.ToolbarSide.LeftOfPlay, "Build Settings", 100, DrawGUI, 500, 0, 100, 5);

        [MenuItem(EditorSettings.MenuGroupPath + "Build Settings", false, -10000)]
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
#if DEVELOPMENT_TOOLS_UNITY_ADDRESSABLES
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
            GUILayout.Label("Configure Symbols", EditorStyles.boldLabel);

            foreach (string symbol in symbols) {
                // display if the symbol is already defined in the current platform
                bool isDefined = EditorHelper.IsSymbolDefined(symbol);

                GUIStyle style = new(GUI.skin.button) {
                    normal = {
                        textColor = isDefined ? Color.green : Color.yellow
                    }
                };

                if (isDefined) {
                    if (GUILayout.Button($"Remove Symbol: {symbol}", style)) {
                        EditorHelper.RemoveSymbol(symbol);
                    }
                }
                else {
                    if (GUILayout.Button($"Add Symbol: {symbol}", style)) {
                        EditorHelper.AddSymbol(symbol);
                    }
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

#if DEVELOPMENT_TOOLS_UNITY_ADDRESSABLES
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