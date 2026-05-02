#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR && !SIMULATE_BUILD
using System;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using DevelopmentTools.ODIN_INSPECTOR;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.DevelopmentTools.Editor.AttributeDrawers {

    public class PreviewTexture2DAttributeDrawer<T> : OdinAttributeDrawer<PreviewTexture2DAttribute, T> where T : Object {

        protected override void DrawPropertyLayout(GUIContent label) {
            if (!ValueEntry.SmartValue && !Attribute.DrawIfNull) return;

            switch (ValueEntry.SmartValue) {
                case Texture texture: DrawTexturePreview(texture); break;
                case Sprite sprite:   DrawTexturePreview(sprite ? sprite.texture : null); break;
                default:              CallNextDrawer(label); break;
            }
        }

        private void DrawTexturePreview(Texture texture) {
            try {
                Attribute.Resolve(Property);
                Draw(texture, Attribute.Height, Attribute.ExpandHeight, Attribute.BackgroundColor, Attribute.OutlineColor, Attribute.OutlineThickness);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

        public static void Draw(Texture texture, float height = 0, bool expandHeight = false, Color background = default, Color outline = default, int thickness = 0) {
            if (!texture) {
                SirenixEditorGUI.ErrorMessageBox("Texture is null");
                return;
            }

            float aspectRatio = (float) texture.width / texture.height;
            float rectHeight  = height > 0 ? height : texture.height;
            Rect  textureRect = GUILayoutUtility.GetAspectRect(aspectRatio, GUILayout.Height(rectHeight), GUILayout.ExpandHeight(expandHeight));

            Rect outlineRect = new() {
                x      = textureRect.x - thickness,
                y      = textureRect.y - thickness,
                width  = textureRect.width + thickness * 2,
                height = textureRect.height + thickness * 2
            };

            SirenixEditorGUI.DrawRoundRect(outlineRect, background, 2, outline, thickness);

            GUI.DrawTexture(outlineRect, texture, ScaleMode.ScaleToFit);

            PreviewTexture2DWindow.DrawZoomableGUI(outlineRect, texture);
        }

    }

}
#endif