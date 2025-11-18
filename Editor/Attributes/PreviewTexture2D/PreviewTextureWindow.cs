using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.Attributes.PreviewTexture2D {

    public class PreviewTextureWindow : OdinEditorWindow {

        [HideLabel]
        [PreviewTexture]
        public Texture texture;

        public void Show(Texture texture) {
            Show();
            this.texture = texture;
        }

        public static void DrawZoomableGUI(Rect rect, Texture texture, bool requireCtrl = false) {
            if (!mouseOverWindow)
                return;

            if (mouseOverWindow is PreviewTextureWindow)
                return;

            if (!rect.Contains(Event.current.mousePosition))
                return;

            bool zoomOnClick = !requireCtrl || Event.current.control;

            EditorGUIUtility.AddCursorRect(rect, zoomOnClick ? MouseCursor.Zoom : MouseCursor.Arrow);

            if (Event.current.type == EventType.MouseDown && zoomOnClick) {
                GetWindow<PreviewTextureWindow>().Show(texture);
                Event.current.Use();
            }

            mouseOverWindow.Repaint(); // for realtime cursor update
        }

    }

}