#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.ODIN_INSPECTOR;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.AttributeDrawers {

    public class PreviewTexture2DWindow : OdinEditorWindow, IHasCustomMenu {

        private const int OutlineThickness = 8;

        private bool ExpandHeight = true;

        [HideLabel]
        [PreviewTexture2D(Height = 0, ExpandHeightGetter = nameof(ExpandHeight))]
        public Texture texture;

        private bool movedToMouse;

        public void Show(Texture texture) {
            Show();
            titleContent = new("Preview");
            this.texture = texture;
            minSize      = Vector2.one;
            OriginalSize();
        }

        protected override void OnImGUI() {
            base.OnImGUI();

            if (movedToMouse)
                return;

            if (Event.current == null)
                return;

            position     = new(Event.current.mousePosition.x, Event.current.mousePosition.y, position.width, position.height);
            movedToMouse = true;
        }

        public void OriginalSize() {
            position = new(position.x, position.y, texture.width + OutlineThickness, texture.height + OutlineThickness);
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

        public void AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new("Original size"), false, OriginalSize);
            menu.AddItem(new("Expand"), ExpandHeight, () => ExpandHeight ^= true);
        }

    }

}
#endif