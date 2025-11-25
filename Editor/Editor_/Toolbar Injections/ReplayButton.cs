#if UNITY_EDITOR && !SIMULATE_BUILD && DEVELOPMENT_TOOLS_ODIN_INSPECTOR
using System;
using System.Threading.Tasks;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using EditorSettings = DevelopmentTools.Editor.Debugging.Settings.EditorSettings;

namespace DevelopmentTools.Editor.Editor_.Toolbar_Injections {

    public static class ReplayButton {

        [InitializeOnLoadMethod]
        public static void Initialize() => ToolbarGUIInjector.leftOfPlayGUI.Insert(0, (ReplayButtonGUI, 30, 100000000));

        private static void ReplayButtonGUI() {
            Rect rect = EditorGUILayout.GetControlRect(false, 20, GUILayout.Width(32)).SubX(3);

            SirenixEditorGUI.DrawRoundRect(rect, (EditorSettings.OnCompile.PlayOnCompile ? Color.cyan : Color.white).A(EditorApplication.isPlaying ? .25f : .1f), 4);

            if (GUI.Button(rect, new GUIContent { tooltip = "Replay (Ctrl/Shift to toggle play on recompile)" }, EditorStyles.label)
                || SirenixEditorGUI.IconButton(rect.SetHeight(16).AddY(2), EditorIcons.Rotate, EditorStyles.whiteBoldLabel, "Replay (Ctrl/Shift to toggle play on recompile)")) {
                if (Event.current.control || Event.current.shift)
                    EditorSettings.OnCompile.PlayOnCompile ^= true;
                else if (EditorApplication.isPlaying)
                    Replay();
            }
        }

        private static async void Replay() {
            try {
                EditorApplication.ExitPlaymode();
                await Task.Delay(20);
                AssetDatabase.Refresh();
                EditorApplication.EnterPlaymode();
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

    }

}
#endif