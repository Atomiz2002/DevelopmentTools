using UnityEngine;
#if UNITY_EDITOR
using DevelopmentEssentials.Editor.Extensions.Unity;
using UnityEditor;
#endif
#if DEVELOPMENT_TOOLS_ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace DevelopmentTools.Editor.Debugging.Settings {

    [CreateAssetMenu(fileName = "TSH Editor Settings Preset", menuName = "The Sixth Hammer/Editor Settings Preset")]
    public class EditorSettingsPreset : ScriptableObject, ICopyable {

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
            if (EditorSettings.PresetGUID == AssetDatabase.GetAssetPath(this).PathToGUID())
                Apply();
        }

        [Button]
        public void UpdateValues() {
            DebugLoggerShowOnPlay      = EditorSettings.DebugLogger.ShowOnPlay;
            DebugLoggerMergeDuplicates = EditorSettings.DebugLogger.MergeDuplicates;

            OnCompileFocus       = EditorSettings.OnCompile.FocusOnCompile;
            OnCompilePlay        = EditorSettings.OnCompile.PlayOnCompile;
            OnCompileFocusOnPlay = EditorSettings.OnCompile.FocusOnPlay;
        }

        [Button]
        public void Apply() {
            EditorSettings.DebugLogger.ShowOnPlay      = DebugLoggerShowOnPlay;
            EditorSettings.DebugLogger.MergeDuplicates = DebugLoggerMergeDuplicates;

            EditorSettings.OnCompile.FocusOnCompile = OnCompileFocus;
            EditorSettings.OnCompile.PlayOnCompile  = OnCompilePlay;
            EditorSettings.OnCompile.FocusOnPlay    = OnCompileFocusOnPlay;
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
            if (EditorSettings.PresetGUID == AssetDatabase.GetAssetPath(this).PathToGUID())
                Apply();
        }

        // TODO where does this even show?
        // public void UpdateValues() {
        //     DebugLoggerShowOnPlay      = EditorSettings.DebugLogger.ShowOnPlay;
        //     DebugLoggerMergeDuplicates = EditorSettings.DebugLogger.MergeDuplicates;
        //
        //     OnCompileFocus       = EditorSettings.OnCompile.FocusOnCompile;
        //     OnCompilePlay        = EditorSettings.OnCompile.PlayOnCompile;
        //     OnCompileFocusOnPlay = EditorSettings.OnCompile.FocusOnPlay;
        // }

        public void Apply() {
            EditorSettings.DebugLogger.ShowOnPlay      = DebugLoggerShowOnPlay;
            EditorSettings.DebugLogger.MergeDuplicates = DebugLoggerMergeDuplicates;

            EditorSettings.OnCompile.FocusOnCompile = OnCompileFocus;
            EditorSettings.OnCompile.PlayOnCompile  = OnCompilePlay;
            EditorSettings.OnCompile.FocusOnPlay    = OnCompileFocusOnPlay;
        }

#endif

        public void OnPaste() => Apply();

#else
        public void OnPaste() {}
#endif

    }

}