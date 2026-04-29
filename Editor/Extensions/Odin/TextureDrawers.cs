#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using DevelopmentTools.Editor.Editor.AttributeDrawers;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace DevelopmentTools.Editor.Editor.Extensions.Odin {

    public class TextureDrawer : OdinValueDrawer<Texture2D> {

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            if (ValueEntry.SmartValue)
                PreviewTexture2DWindow.DrawZoomableGUI(GUILayoutUtility.GetLastRect(), ValueEntry.SmartValue, true);
        }

    }

    public class SpriteDrawer : OdinValueDrawer<Sprite> {

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            if (ValueEntry.SmartValue)
                PreviewTexture2DWindow.DrawZoomableGUI(GUILayoutUtility.GetLastRect(), ValueEntry.SmartValue.texture, true);
        }

    }

    public class ImageDrawer : OdinValueDrawer<Texture> {

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            if (ValueEntry.SmartValue)
                PreviewTexture2DWindow.DrawZoomableGUI(GUILayoutUtility.GetLastRect(), ValueEntry.SmartValue, true);
        }

    }

}
#endif