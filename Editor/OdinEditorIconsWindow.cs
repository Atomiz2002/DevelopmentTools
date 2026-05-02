#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR && !SIMULATE_BUILD
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace DevelopmentTools.DevelopmentTools.Editor {

    public class OdinIconsViewer : EditorWindow {

        [MenuItem("Tools/Odin/Inspector/Editor Icons")] private static void ShowWindow() => EditorIconsOverview.OpenEditorIconsOverview();

    }

}
#endif