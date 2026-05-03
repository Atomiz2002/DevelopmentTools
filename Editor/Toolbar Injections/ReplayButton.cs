#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR && !SIMULATE_BUILD
using System;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using DevelopmentTools.DevelopmentTools.Editor.Settings;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
#if DEVELOPMENT_TOOLS_EDITOR_UNI_TASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace DevelopmentTools.DevelopmentTools.Editor.Toolbar_Injections {

    public static class ReplayButton {

        [InitializeOnLoadMethod]
        public static void Initialize() => ToolbarGUIInjector.leftOfPlayGUI.Insert(0, (ReplayButtonGUI, 30, 100000000));

        private static void ReplayButtonGUI() {
            Rect rect = EditorGUILayout.GetControlRect(false, 20, GUILayout.Width(32)).SubX(3);

            SirenixEditorGUI.DrawRoundRect(rect, (EngineSettings.OnCompile.PlayOnCompile ? Color.cyan : Color.white).A(EditorApplication.isPlaying ? .25f : .1f), 4);
            EditorGUIUtility.AddCursorRect(rect, Event.current.control || Event.current.shift || Event.current.alt ? MouseCursor.RotateArrow : MouseCursor.Arrow);

            if (GUI.Button(rect, new GUIContent { tooltip = $"{(EditorApplication.isPlaying ? "Replay" : "AutoPlay")} (F4, Ctrl/Shift to toggle)" }, new(EditorStyles.label) { normal = { textColor = Color.white } })
                || SirenixEditorGUI.IconButton(rect.SetHeight(16).AddY(2), EditorIcons.Rotate, EditorStyles.whiteBoldLabel, string.Empty)) {
                if (Event.current.control || Event.current.shift || Event.current.alt)
                    EngineSettings.OnCompile.PlayOnCompile ^= true;
                else if (EditorApplication.isPlaying)
                    Replay();
            }
        }

        [Shortcut("Replay", KeyCode.F4)]
        private static async void Replay() {
            try {
                const int delayMs = 100;
                EditorApplication.ExitPlaymode();
#if DEVELOPMENT_TOOLS_EDITOR_UNI_TASK
                await UniTask.Delay(delayMs);
#else
                await Task.Delay(delayMs);
#endif
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