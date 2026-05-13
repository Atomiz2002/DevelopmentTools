#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using UnityEditor;
using UnityEngine;
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_UI
using UnityEngine.UI;
#endif

namespace DevelopmentTools.DevelopmentTools.Editor {

    [InitializeOnLoad]
    public static class HierarchyOverhaul {

        // todo editor setting to enable/disable for play mode

        private static readonly Dictionary<int, Color> iconsColors = new();

        static HierarchyOverhaul() => EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect) {
            try {
                // if (Application.isPlaying)
                //     return;

                if (!EditorUtility.InstanceIDToObject(instanceID).Is(out GameObject go))
                    return;

                bool selected = Selection.instanceIDs.Contains(instanceID);
                bool active   = EditorWindow.focusedWindow && EditorWindow.focusedWindow.GetType().Name == "SceneHierarchyWindow";

                (Texture texture, Color color) icon = (instanceID.GetIcon(), Color.white);
                iconsColors.TryGetValue(instanceID, out icon.color);

                if (selected && active) {
                    if (go.TryGetComponent(out SpriteRenderer renderer)) {
                        if (renderer.sprite && renderer.sprite.texture)
                            icon = (renderer.sprite.n()?.texture.n() ?? Texture2D.whiteTexture, renderer.color);
                    }
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_UI
                    else if (go.TryGetComponent(out Image image)) {
                        icon = (image.sprite.n()?.texture.n() ?? Texture2D.whiteTexture, image.color);
                    }
                    else if (go.TryGetComponent(out RawImage rawImage)) {
                        icon = (rawImage.texture.n() ?? Texture2D.whiteTexture, rawImage.color);
                    }
#endif

                    icon.texture.Trim(true);
                    icon.texture.filterMode = FilterMode.Point;

                    instanceID.SetIcon(icon.texture);
                    iconsColors[instanceID] = icon.color;
                }

                if (!icon.texture)
                    return;

                Rect iconRect = new(selectionRect.x, selectionRect.y, 16, 16);

                // colored indicator to the left of the icon
                // Rect colorCodeRect = new(iconRect.x - 20, iconRect.y, 4, 16);
                // Color color = Color.clear;
                // if (!colorCodes.TryGetValue(color, out Texture colorCode))
                //     colorCode = color.ToTexture();
                // GUI.DrawTexture(colorCodeRect, colorCode, ScaleMode.StretchToFill);

                // TODO gray (tmpro content) next to GO name

                EditorHelper.DrawColoredTexture(iconRect, EditorHelper.BackgroundColor(selected, active));
                Color guiColor = GUI.color;
                GUI.color = icon.color;
                GUI.DrawTexture(iconRect, icon.texture, ScaleMode.ScaleToFit);
                GUI.color = guiColor;
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

    }

}
#endif