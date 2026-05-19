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

                Object           asset = guid.LoadAssetByGUID();
                IHaveIconPreview icon  = guid.GetIcon();

                if (asset is IHaveIconPreview iHaveIcon)
                    icon = iHaveIcon;
                // else if (guid.LoadAssetByGUID<Texture2D>()) // better sliced previews but HELLA slow. draws the first sprite on all sub-assets. potential solution if possible is highly inefficient
                //     icon = guid.LoadAssetByGUID<Sprite>().ToTexture();

                if (!icon.Icon)
                    return;

                if (selected && active) {
                    icon.Icon.SetFilter(FilterMode.Point).Trim(true);

                    guid.SetIcon(icon);
                }

                Rect iconRect = new(selectionRect.x, selectionRect.y, selectionRect.height, selectionRect.height);

                if (iconRect.height > 16)
                    iconRect.width = iconRect.height *= 0.8f;

                icon.Draw(iconRect, ScaleMode.StretchToFill, selected, active);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

    }

}
#endif