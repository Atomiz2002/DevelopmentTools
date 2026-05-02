#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.ODIN_INSPECTOR;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.DevelopmentTools.Editor.AttributeDrawers {

    public class PreviewTexture2DWindow : OdinEditorWindow {

        [HideLabel]
        [PreviewTexture2D]
        public Texture texture;

        public void Show(Texture texture) {
            Show();
            titleContent = new("Preview");
            this.texture = texture;
        }

        public static void Create(Texture texture) => GetWindow<PreviewTexture2DWindow>().Show(texture);

        public static void DrawZoomableGUI(Rect rect, Texture texture) {
            if (!mouseOverWindow)
                return;

            if (mouseOverWindow.Is<PreviewTexture2DWindow>())
                return;

            if (!rect.Contains(Event.current.mousePosition))
                return;

            EditorGUIUtility.AddCursorRect(rect, Event.current.control ? MouseCursor.Zoom : MouseCursor.Arrow);

            if (Event.current.type == EventType.MouseDown && Event.current.control) {
                Create(texture);
                Event.current.Use();
            }

            mouseOverWindow.Repaint(); // for realtime cursor update
        }

    }

}
#endif