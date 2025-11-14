#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopmentTools.Editor {

    [InitializeOnLoad]
    public static class HierarchyIcons {

        // todo editor setting to enable/disable for play mode

        internal static Texture bgTex;
        // private static readonly Dictionary<Color, Texture> colorCodes = new();

        static HierarchyIcons() => EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect) {
            try {
                // if (Application.isPlaying)
                //     return;

                if (EditorUtility.InstanceIDToObject(instanceID) is not GameObject go)
                    return;

                Rect iconRect = new(selectionRect.x, selectionRect.y, 16, 16);
                // Rect colorCodeRect = new(iconRect.x - 20, iconRect.y, 4, 16);

                // Color color = Color.clear; // colored indicator to the left of the icon
                // if (!colorCodes.TryGetValue(color, out Texture colorCode))
                //     colorCode = color.ToTexture();

                // GUI.DrawTexture(colorCodeRect, colorCode, ScaleMode.StretchToFill);

                Texture icon = null;

                if (go.TryGetComponent(out SpriteRenderer renderer)) {
                    icon = renderer.sprite.ToTexture2D();
                }
                else if (go.TryGetComponent(out Image image)) {
                    icon = image.sprite.ToTexture2D();
                }
                else if (go.TryGetComponent(out RawImage rawImage)) {
                    icon = rawImage.texture;
                }

                if (!icon)
                    return;

                icon.Trim(true);
                icon.filterMode = FilterMode.Point;

                if (!bgTex)
                    bgTex = new Color(.22f, .22f, .22f, 1f).ToTexture();

                GUI.DrawTexture(iconRect, bgTex, ScaleMode.StretchToFill);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

    }

}
#endif