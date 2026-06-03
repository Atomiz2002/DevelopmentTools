#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using DevelopmentEssentials.Extensions.Unity;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopmentTools.Editor.AttributeDrawers {

    public class TexturePreviewDrawer : OdinValueDrawer<Texture2D> {

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            if (ValueEntry.SmartValue)
                PreviewTexture2DWindow.DrawZoomableGUI(GUILayoutUtility.GetLastRect(), ValueEntry.SmartValue);
        }

    }

    public class SpritePreviewDrawer : OdinValueDrawer<Sprite> {

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            if (ValueEntry.SmartValue)
                PreviewTexture2DWindow.DrawZoomableGUI(GUILayoutUtility.GetLastRect(), ValueEntry.SmartValue.texture);
        }

    }

    public class ImageDrawer : OdinValueDrawer<Texture> {

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            if (ValueEntry.SmartValue)
                PreviewTexture2DWindow.DrawZoomableGUI(GUILayoutUtility.GetLastRect(), ValueEntry.SmartValue);
        }

    }

    public static class PreviewGraphicContextMenu {

        [MenuItem("CONTEXT/Image/Preview Image")]
        public static void PreviewImage(MenuCommand menuCommand) => PreviewTexture2DWindow.Create(((Image) menuCommand.context).sprite.ToTexture2D());

        [MenuItem("CONTEXT/RawImage/Preview RawImage")]
        public static void PreviewRawImage(MenuCommand menuCommand) => PreviewTexture2DWindow.Create(((RawImage) menuCommand.context).texture);

        [MenuItem("CONTEXT/SpriteRenderer/Preview Sprite")]
        public static void PreviewSpriteRenderer(MenuCommand menuCommand) => PreviewTexture2DWindow.Create(((SpriteRenderer) menuCommand.context).sprite.ToTexture2D());

        [MenuItem("CONTEXT/Image/Preview Image", true)]
        public static bool PreviewImageValidate(MenuCommand menuCommand) => menuCommand.context.Is(out Image i) && i;

        [MenuItem("CONTEXT/RawImage/Preview RawImage", true)]
        public static bool PreviewRawImageValidate(MenuCommand menuCommand) => menuCommand.context.Is(out RawImage ri) && ri;

        [MenuItem("CONTEXT/SpriteRenderer/Preview Sprite", true)]
        public static bool PreviewSpriteRendererValidate(MenuCommand menuCommand) => menuCommand.context.Is(out SpriteRenderer sr) && sr.sprite;

    }

}
#endif