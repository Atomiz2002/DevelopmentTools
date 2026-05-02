#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using DevelopmentTools.DevelopmentTools.Editor.AttributeDrawers;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace DevelopmentTools.DevelopmentTools.Editor.Extensions.Odin {

    public class TextureDrawer : OdinValueDrawer<Texture2D> {

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            if (ValueEntry.SmartValue)
                PreviewTexture2DWindow.DrawZoomableGUI(GUILayoutUtility.GetLastRect(), ValueEntry.SmartValue);
        }

    }

    public class SpriteDrawer : OdinValueDrawer<Sprite> {

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

}
#endif