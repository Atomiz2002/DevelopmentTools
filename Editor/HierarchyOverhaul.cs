#if !SIMULATE_BUILD
using System;
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Editor.Debugging.DebugFields;
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
                if (EditorUtility.InstanceIDToObject(instanceID).IsNot(out GameObject go))
                    return;

                bool selected = Selection.instanceIDs.Contains(instanceID);
                bool active   = EditorWindow.focusedWindow && EditorWindow.focusedWindow.GetType().Name == "SceneHierarchyWindow";

                Texture icon  = go.GetIcon();
                Color   color = Color.clear;

                if (Selection.instanceIDs.Length <= 3) {
                    if (selected && active || !icon) {
                        if (go.TryGetComponent(out SpriteRenderer renderer)) {
                            icon  = renderer.sprite.n()?.ToTexture2D();
                            color = renderer.color;
                        }
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_UI
                        else if (go.TryGetComponent(out Image image)) {
                            icon  = image.sprite.n()?.ToTexture2D();
                            color = image.color;
                        }
                        else if (go.TryGetComponent(out RawImage rawImage)) {
                            icon  = rawImage.texture.Read();
                            color = rawImage.color;
                        }
#endif
                        else
                            icon = null;

                        if (icon != null) {
                            go.SetIcon(icon.SetFilter(FilterMode.Point).Trimmed(true));
                        }
                    }
                }

                if (!icon)
                    return;

                Rect iconRect = new(selectionRect.x, selectionRect.y, 16, 16);

                // colored indicator to the left of the icon
                // Rect colorCodeRect = new(iconRect.x - 20, iconRect.y, 4, 16);
                // Color color = Color.clear;
                // if (!colorCodes.TryGetValue(color, out Texture colorCode))
                //     colorCode = color.ToTexture();
                // GUI.DrawTexture(colorCodeRect, colorCode, ScaleMode.StretchToFill);

                // TODO gray (tmpro content) next to GO name

                icon.DrawIcon(iconRect, ScaleMode.ScaleToFit, selected, active);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

    }

}
#endif