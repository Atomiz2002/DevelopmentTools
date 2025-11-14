#if UNITY_EDITOR
using System;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Attributes.PreviewTexture2D {

    public class PreviewTextureAttributeDrawer<T> : OdinAttributeDrawer<PreviewTextureAttribute, T> where T : Object {

        protected override void DrawPropertyLayout(GUIContent label) {
            if (!ValueEntry.SmartValue && !Attribute.DrawIfNull) return;

            switch (ValueEntry.SmartValue) {
                case Texture texture: DrawTexturePreview(texture); break;
                case Sprite sprite:   DrawTexturePreview(sprite.ToTexture2D()); break;
                default:              CallNextDrawer(label); break;
            }
        }

        private void DrawTexturePreview(Texture texture) {
            try {
                Attribute.Resolve(Property);

                if (!texture) {
                    SirenixEditorGUI.ErrorMessageBox("Texture is null");
                    return;
                }

                float height      = Attribute.Height == 0 ? texture.height : Attribute.Height;
                Rect  textureRect = GUILayoutUtility.GetAspectRect(texture.width / height);

                Rect outlineRect = new() {
                    x      = textureRect.x - Attribute.OutlineThickness,
                    y      = textureRect.y - Attribute.OutlineThickness,
                    width  = textureRect.width + Attribute.OutlineThickness * 2,
                    height = textureRect.height + Attribute.OutlineThickness * 2
                };

                SirenixEditorGUI.DrawRoundRect(outlineRect, Attribute.BackgroundColor, 2, Attribute.OutlineColor, Attribute.OutlineThickness);
                texture.filterMode = FilterMode.Point;
                GUI.DrawTexture(outlineRect, texture, ScaleMode.ScaleToFit);

                PreviewTextureWindow.DrawZoomableGUI(outlineRect, texture);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

    }

}
#endif