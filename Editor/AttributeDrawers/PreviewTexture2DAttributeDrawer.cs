#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR && !SIMULATE_BUILD
using System;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.ODIN_INSPECTOR;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Editor.AttributeDrawers {

    public class PreviewTexture2DAttributeDrawer<T> : OdinAttributeDrawer<PreviewTexture2DAttribute, T>, IDefinesGenericMenuItems where T : Object {

        protected override void DrawPropertyLayout(GUIContent label) {
            if (!ValueEntry.SmartValue && !Attribute.DrawIfNull) return;

            if (ValueEntry.SmartValue.Is(out Texture texture))
                DrawTexturePreview(texture);
            else if (ValueEntry.SmartValue.Is(out Sprite sprite))
                DrawTexturePreview(sprite ? sprite.texture : null);
            else
                CallNextDrawer(label);
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

            // stable across Layout and Repaint
            Rect totalRect = GUILayoutUtility.GetRect(0, rectHeight, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(expandHeight));

            Rect  textureRect = totalRect;
            float targetWidth = totalRect.height * aspectRatio;

            if (targetWidth > totalRect.width) {
                textureRect.height = totalRect.width / aspectRatio;
            }
            else {
                textureRect.width = targetWidth;
            }

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

        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu) {
            genericMenu.AddItem(new("Copy to clipboard"), false, () => ValueEntry.SmartValue.CopyObjToClipboard());
        }

    }

}
#endif