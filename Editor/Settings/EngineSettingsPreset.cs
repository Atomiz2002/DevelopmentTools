using UnityEngine;
#if UNITY_EDITOR
using DevelopmentEssentials.Editor.Extensions.Unity;
using UnityEditor;
#endif

#if DEVELOPMENT_TOOLS_ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace DevelopmentTools.Editor.Settings {

    [CreateAssetMenu(fileName = "Engine Settings Preset", menuName = "Development Tools/Engine Settings Preset")]
    public class EngineSettingsPreset : ScriptableObject, ICopyable {

#if UNITY_EDITOR
#if DEVELOPMENT_TOOLS_ODIN_INSPECTOR
        [Title("Debug Logger")]
        [LabelText("ShowOnPlay")] public bool DebugLoggerShowOnPlay;
        [LabelText("MergeDuplicates")] public bool DebugLoggerMergeDuplicates;

        [Title("On Compile")]
        [LabelText("Focus")] public bool OnCompileFocus;
        [LabelText("Play")]          public bool OnCompilePlay;
        [LabelText("Focus On Play")] public bool OnCompileFocusOnPlay;

        private void OnValidate() {
            if (EngineSettings.PresetGUID == AssetDatabase.GetAssetPath(this).PathToGUID())
                Apply();
        }

        [Button]
        public void UpdateValues() {
            DebugLoggerShowOnPlay = EngineSettings.DebugLogger.ShowOnPlay;
            DebugLoggerMergeDuplicates = EngineSettings.DebugLogger.MergeDuplicates;

            OnCompileFocus = EngineSettings.OnCompile.FocusOnCompile;
            OnCompilePlay = EngineSettings.OnCompile.PlayOnCompile;
            OnCompileFocusOnPlay = EngineSettings.OnCompile.FocusOnPlay;
        }

        [Button]
        public void Apply() {
            EngineSettings.DebugLogger.ShowOnPlay = DebugLoggerShowOnPlay;
            EngineSettings.DebugLogger.MergeDuplicates = DebugLoggerMergeDuplicates;

            EngineSettings.OnCompile.FocusOnCompile = OnCompileFocus;
            EngineSettings.OnCompile.PlayOnCompile = OnCompilePlay;
            EngineSettings.OnCompile.FocusOnPlay = OnCompileFocusOnPlay;
        }
#else
        [Header("Debug Logger")]
        [InspectorName("ShowOnPlay")] public bool DebugLoggerShowOnPlay;
        [InspectorName("MergeDuplicates")] public bool DebugLoggerMergeDuplicates;

        [Header("On Compile")]
        [InspectorName("Focus")] public bool OnCompileFocus;
        [InspectorName("Play")]          public bool OnCompilePlay;
        [InspectorName("Focus On Play")] public bool OnCompileFocusOnPlay;

        private void OnValidate() {
            if (EngineSettings.PresetGUID == AssetDatabase.GetAssetPath(this).PathToGUID())
                Apply();
        }

        // TODO where does this even show?
        // public void UpdateValues() {
        //     DebugLoggerShowOnPlay      = EngineSettings.DebugLogger.ShowOnPlay;
        //     DebugLoggerMergeDuplicates = EngineSettings.DebugLogger.MergeDuplicates;
        //
        //     OnCompileFocus       = EngineSettings.OnCompile.FocusOnCompile;
        //     OnCompilePlay        = EngineSettings.OnCompile.PlayOnCompile;
        //     OnCompileFocusOnPlay = EngineSettings.OnCompile.FocusOnPlay;
        // }

        public void Apply() {
#if DEVELOPMENT_TOOLS_ODIN_INSPECTOR
            EngineSettings.DebugLogger.ShowOnPlay = DebugLoggerShowOnPlay;
            EngineSettings.DebugLogger.MergeDuplicates = DebugLoggerMergeDuplicates;
#endif
            EngineSettings.OnCompile.FocusOnCompile = OnCompileFocus;
            EngineSettings.OnCompile.PlayOnCompile  = OnCompilePlay;
            EngineSettings.OnCompile.FocusOnPlay    = OnCompileFocusOnPlay;
        }

#endif

        public void OnPaste() => Apply();

#else
        public void OnPaste() {}
#endif

    }

}