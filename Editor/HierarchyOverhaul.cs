#if !SIMULATE_BUILD
using System;
using System.Linq;
using DevelopmentEssentials.Extensions.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_UI
using UnityEngine.UI;
#endif

namespace DevelopmentTools.Editor {

    [InitializeOnLoad]
    public static class HierarchyOverhaul {

        private const string IconPrefKey = nameof(HierarchyOverhaul) + ".Icon";
        private const string TextPrefKey = nameof(HierarchyOverhaul) + ".Text";

        // todo editor setting to enable/disable for play mode

        static HierarchyOverhaul() {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

            SceneHierarchyHooks.addItemsToSceneHeaderContextMenu += (menu, _) => {
                menu.AddItem(new("Preview Icons"), EditorPrefs.GetBool(IconPrefKey), () => {
                    EditorPrefs.SetBool(IconPrefKey, !EditorPrefs.GetBool(IconPrefKey));
                });

                menu.AddDisabledItem(new("Preview Texts") /*, EditorPrefs.GetBool(TextPrefKey), () => {
                    EditorPrefs.SetBool(TextPrefKey, !EditorPrefs.GetBool(TextPrefKey));
                }*/);
            };
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect) {
            try {
#if UNITY_6000_0 || UNITY_6000_3
                if (EditorUtility.EntityIdToObject(instanceID).IsNot(out GameObject go))
                    return;

                EntityId[] selection = Selection.entityIds;
#else
                if (EditorUtility.InstanceIDToObject(instanceID).IsNot(out GameObject go))
                    return;

                int[] selection = Selection.instanceIDs;
#endif
                bool selected = selection.Contains(instanceID);
                bool active   = EditorWindow.focusedWindow && EditorWindow.focusedWindow.GetType().Name == "SceneHierarchyWindow";

                if (EditorPrefs.GetBool(IconPrefKey))
                    DrawIcon(go, selection.Length, selected, active, selectionRect);

                if (EditorPrefs.GetBool(TextPrefKey))
                    DrawText(go, selection.Length, selected, active, selectionRect);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

        private static void DrawIcon(GameObject go, int selectionLength, bool selected, bool active, Rect selectionRect) {
            IHaveIconPreview icon = go.GetIcon();

            if (selectionLength <= 3) {
                if (selected && active || icon == null) {
                    if (go.TryGetComponent(out SpriteRenderer renderer)) {
                        icon = new IconPreview(renderer.sprite.n()?.ToTexture2D(), renderer.color);
                    }
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_UI
                    else if (go.TryGetComponent(out Image image)) {
                        icon = new IconPreview(image.sprite.n()?.ToTexture2D() ?? Texture2D.whiteTexture, image.color);
                        if (!icon.Icon)
                            icon.Icon = Texture2D.whiteTexture;
                    }
                    else if (go.TryGetComponent(out RawImage rawImage)) {
                        icon = new IconPreview(rawImage.texture.n()?.Read() ?? Texture2D.whiteTexture, rawImage.color);
                    }
#endif
                    // else if (has ExtractableInfo)
                    else
                        icon = null;

                    // if (icon?.Icon)
                    //     icon.Icon.SetFilter(FilterMode.Point).Trim(true);

                    go.SetIcon(icon, true);
                }
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

            icon.DrawIcon(iconRect, ScaleMode.ScaleToFit, selected, active);
        }

        private static void DrawText(GameObject go, int selectionLength, bool selected, bool active, Rect selectionRect) {
            // TODO
        }

    }

}
#endif