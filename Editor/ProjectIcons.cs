#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using System.Linq;
using DevelopmentEssentials.DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Editor {

    [InitializeOnLoad]
    public class ProjectIcons {

        static ProjectIcons() => EditorApplication.projectWindowItemOnGUI += OnProjectGUI;

        private static void OnProjectGUI(string guid, Rect selectionRect) {
            try {
                Object           asset = guid.LoadAssetByGUID();
                IHaveIconPreview icon  = asset as IHaveIconPreview ?? asset.GetIcon(true);

                if (!icon.Icon)
                    return;

                bool selected = Selection.assetGUIDs.Contains(guid);
                bool active   = EditorWindow.focusedWindow.n()?.GetType().Name == "ProjectBrowser";

                if (selected && active) {
                    icon.Icon.SetFilter(FilterMode.Point).Trim(true);

                    asset.SetIcon(icon);
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