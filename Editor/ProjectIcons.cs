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

                if (asset.GetIcon(true) == null || !icon?.Icon)
                    return;

                bool selected = Selection.assetGUIDs.Contains(guid);
                bool active   = EditorWindow.focusedWindow.n()?.GetType().Name == "ProjectBrowser";

#if UNITY_6000_0 || UNITY_6000_3
                EntityId[] selection = Selection.entityIds;
#else
                int[] selection = Selection.instanceIDs;
#endif

                if (selection.Length <= 3) {
                    if (selected && active && icon.Icon) {
                        asset.SetIcon(icon);
                    }
                }

                icon.DrawIcon(selectionRect, ScaleMode.StretchToFill, selected, active);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

    }

}
#endif