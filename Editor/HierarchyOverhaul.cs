#if !SIMULATE_BUILD
using System;
using System.Linq;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using UnityEditor;
using UnityEngine;
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_UI
using UnityEngine.UI;
#endif

namespace DevelopmentTools.Editor {

    [InitializeOnLoad]
    public static class HierarchyOverhaul {
        // todo editor setting to enable/disable for play mode

        static HierarchyOverhaul() => EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect) {
            try {
                // if (Application.isPlaying)
                //     return;

                if (EditorUtility.InstanceIDToObject(instanceID).IsNot(out GameObject go))
                    return;

                bool selected = Selection.instanceIDs.Contains(instanceID);
                bool active   = EditorWindow.focusedWindow && EditorWindow.focusedWindow.GetType().Name == "SceneHierarchyWindow";

                IHaveIconPreview icon = null;

                if (selected && active || !go.HasIcon()) {

                    if (go.TryGetComponent(out SpriteRenderer renderer)) {
                        icon = new IconPreview(renderer.sprite.n()?.ToTexture2D(), renderer.color);
                    }
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_UI
                    else if (go.TryGetComponent(out Image image)) {
                        icon = new IconPreview(image.sprite.n()?.ToTexture2D(), image.color);
                    }
                    else if (go.TryGetComponent(out RawImage rawImage)) {
                        icon = new IconPreview(rawImage.texture, rawImage.color);
                    }
#endif
                    else
                        icon = new IconPreview();

                    icon.Icon.n()?.SetFilter(FilterMode.Point).Trim(true, EditorHelper.BackgroundColor());

                    go.SetIcon(icon);
                }

                if (!icon?.Icon)
                    return;

                Rect iconRect = new(selectionRect.x, selectionRect.y, 16, 16);

                // colored indicator to the left of the icon
                // Rect colorCodeRect = new(iconRect.x - 20, iconRect.y, 4, 16);
                // Color color = Color.clear;
                // if (!colorCodes.TryGetValue(color, out Texture colorCode))
                //     colorCode = color.ToTexture();
                // GUI.DrawTexture(colorCodeRect, colorCode, ScaleMode.StretchToFill);

                // TODO gray (tmpro content) next to GO name

                icon.Draw(iconRect, ScaleMode.ScaleToFit, selected, active);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }
    }

}
#endif