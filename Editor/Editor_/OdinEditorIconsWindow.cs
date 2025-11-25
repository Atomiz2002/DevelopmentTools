#if UNITY_EDITOR && !SIMULATE_BUILD && DEVELOPMENT_TOOLS_ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace DevelopmentTools.Editor.Editor_ {

    public class OdinIconsViewer : EditorWindow {

        [MenuItem("Tools/Odin/Inspector/Editor Icons")] private static void ShowWindow() => EditorIconsOverview.OpenEditorIconsOverview();

    }

}
#endif