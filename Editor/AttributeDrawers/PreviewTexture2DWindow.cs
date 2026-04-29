#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.Editor.AttributeDrawers {

    public class PreviewTexture2DWindow : OdinEditorWindow {

        [HideLabel]
        [PreviewTexture2D]
        public Texture texture;

        public void Show(Texture texture) {
            Show();
            this.texture = texture;
        }

        public static void DrawZoomableGUI(Rect rect, Texture texture, bool requireCtrl = false) {
            if (!mouseOverWindow)
                return;

            if (mouseOverWindow is PreviewTexture2DWindow)
                return;

            if (!rect.Contains(Event.current.mousePosition))
                return;

            bool zoomOnClick = !requireCtrl || Event.current.control;

            EditorGUIUtility.AddCursorRect(rect, zoomOnClick ? MouseCursor.Zoom : MouseCursor.Arrow);

            if (Event.current.type == EventType.MouseDown && zoomOnClick) {
                GetWindow<PreviewTexture2DWindow>().Show(texture);
                Event.current.Use();
            }

            mouseOverWindow.Repaint(); // for realtime cursor update
        }

    }

}
#endif