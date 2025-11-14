using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Editor_.CopyPaste;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Debugging.Settings {

    [CreateAssetMenu(fileName = "TSH Editor Settings Preset", menuName = "The Sixth Hammer/Editor Settings Preset")]
    public class EditorSettingsPreset : ScriptableObject, ICopyable {

#if UNITY_EDITOR

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

        public void OnPaste() => Apply();

#else
        public void OnPaste() {}
#endif

    }

}