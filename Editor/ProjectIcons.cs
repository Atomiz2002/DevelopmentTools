#if !SIMULATE_BUILD
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Editor {

    [InitializeOnLoad]
    public class ProjectIcons {

        static ProjectIcons() => EditorApplication.projectWindowItemOnGUI += OnProjectGUI;

        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        private static void OnProjectGUI(string guid, Rect selectionRect) {
            try {
                if (selectionRect.height > 16)
                    return;

                Object           asset = guid.LoadAssetByGUID();
                IHaveIconPreview icon  = asset as IHaveIconPreview;

                if (!asset.GetIcon() || !icon?.Icon)
                    return;

                bool selected = Selection.assetGUIDs.Contains(guid);
                bool active   = EditorWindow.focusedWindow.n()?.GetType().Name == "ProjectBrowser";

                if (Selection.instanceIDs.Length <= 3) {
                    if (selected && active && icon.Icon) {
                        asset.SetIcon(icon.Icon.SetFilter(FilterMode.Point).Trimmed(true));
                    }
                }

                icon.Icon.DrawIcon(selectionRect, ScaleMode.StretchToFill, selected, active);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

    }

}
#endif