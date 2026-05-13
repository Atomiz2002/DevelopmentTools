#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using System.Linq;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.DevelopmentTools.Editor {

    public class ProjectIcons {

        static ProjectIcons() => EditorApplication.projectWindowItemOnGUI += OnProjectGUI;

        private static void OnProjectGUI(string guid, Rect selectionRect) {
            try {
                bool selected = Selection.assetGUIDs.Contains(guid);
                bool active   = EditorWindow.focusedWindow.n()?.GetType().Name == "ProjectBrowser";

                Object  asset = guid.LoadAssetByGUID();
                Texture icon  = guid.GetIcon();

                if (asset is IHaveProjectViewIcon iHaveIcon)
                    icon = iHaveIcon.Icon;
                // else if (guid.LoadAssetByGUID<Texture2D>()) // better sliced previews but HELLA slow. draws the first sprite on all sub-assets. potential solution if possible is highly inefficient
                //     icon = guid.LoadAssetByGUID<Sprite>().ToTexture();

                if (!icon)
                    return;

                if (selected && active) {
                    icon.Trim(true);
                    icon.filterMode = FilterMode.Point;

                    guid.SetIcon(icon);
                }

                Rect iconRect = new(selectionRect.x, selectionRect.y, selectionRect.height, selectionRect.height);

                if (iconRect.height > 16)
                    iconRect.width = iconRect.height *= 0.8f;

                DrawProjectIcon(iconRect, icon, selected, active);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

        private static void DrawProjectIcon(Rect rect, Texture icon, bool selected, bool active) {
            EditorHelper.DrawColoredTexture(rect, EditorHelper.BackgroundColor(selected, active));
            GUI.DrawTexture(rect, icon, ScaleMode.StretchToFill);
        }

    }

}
#endif