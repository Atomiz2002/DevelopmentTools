#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using DevelopmentTools.Editor.Editor_;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Editor {

    [InitializeOnLoad]
    public class ProjectIcons {

        static ProjectIcons() => EditorApplication.projectWindowItemOnGUI += OnProjectGUI;

        private static void OnProjectGUI(string guid, Rect selectionRect) {
            try {
                if (Application.isPlaying)
                    return;

                Object  asset = guid.LoadAssetByGUID();
                Texture icon;

                if (asset is IHaveProjectViewIcon iHaveIcon)
                    icon = iHaveIcon.Icon;
                // else if (guid.LoadAssetByGUID<Texture2D>()) // better sliced previews but HELLA slow. draws the first sprite on all sub-assets. potential solution if possible is highly inefficient
                //     icon = guid.LoadAssetByGUID<Sprite>().ToTexture();
                else
                    return;

                if (!icon)
                    return;

                icon.filterMode = FilterMode.Point;

                Rect iconRect = new(selectionRect.x, selectionRect.y, selectionRect.height, selectionRect.height);

                if (iconRect.height > 16)
                    iconRect.width = iconRect.height *= 0.8f;

                GUI.DrawTexture(iconRect, HierarchyIcons.bgTex, ScaleMode.StretchToFill);
                GUI.DrawTexture(iconRect, icon.Trimmed(true) /*.Underlay(new(.22f, .22f, .22f, 1f))*/, ScaleMode.ScaleToFit);
            }
            catch (Exception e) {
                e.LogEx();
            }
        }

    }

}
#endif