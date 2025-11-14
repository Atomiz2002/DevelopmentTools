#if DEVELOPMENT_TOOLS
using _DevelopmentTools.Attributes.PreviewTexture2D;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace _DevelopmentTools.Extensions.Editor.Odin {

    public class TextureDrawer : OdinValueDrawer<Texture2D> {

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            if (ValueEntry.SmartValue)
                PreviewTextureWindow.DrawZoomableGUI(GUILayoutUtility.GetLastRect(), ValueEntry.SmartValue, true);
        }

    }

    public class SpriteDrawer : OdinValueDrawer<Sprite> {

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            if (ValueEntry.SmartValue)
                PreviewTextureWindow.DrawZoomableGUI(GUILayoutUtility.GetLastRect(), ValueEntry.SmartValue.texture, true);
        }

    }

    public class ImageDrawer : OdinValueDrawer<Texture> {

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            if (ValueEntry.SmartValue)
                PreviewTextureWindow.DrawZoomableGUI(GUILayoutUtility.GetLastRect(), ValueEntry.SmartValue, true);
        }

    }

}
#endif