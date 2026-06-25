#if !SIMULATE_BUILD
using System;
using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
#if DEVELOPMENT_TOOLS_EDITOR_TMP
using TMPro;
#endif
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_UI
using UnityEngine.UI;
#endif

namespace DevelopmentTools.Editor {

    [InitializeOnLoad]
    public static class HierarchyOverhaul {

        private const string IconPrefKey = nameof(HierarchyOverhaul) + ".Icon";
        private const string PlayPrefKey = nameof(HierarchyOverhaul) + ".Play";
        private const string TextPrefKey = nameof(HierarchyOverhaul) + ".Text";

        // todo editor setting to enable/disable for play mode

        static HierarchyOverhaul() {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

            SceneHierarchyHooks.addItemsToSceneHeaderContextMenu += (menu, _) => {
                menu.AddItem(new("Preview Icons"), EditorPrefs.GetBool(IconPrefKey), () => {
                    EditorPrefs.SetBool(IconPrefKey, !EditorPrefs.GetBool(IconPrefKey));
                });

                menu.AddItem(new("Preview Texts"), EditorPrefs.GetBool(TextPrefKey), () => {
                    EditorPrefs.SetBool(TextPrefKey, !EditorPrefs.GetBool(TextPrefKey));
                });

                menu.AddItem(new("Disable in Play Mode"), !EditorPrefs.GetBool(PlayPrefKey), () => {
                    EditorPrefs.SetBool(PlayPrefKey, !EditorPrefs.GetBool(PlayPrefKey));
                });
            };
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect) {
            if (Application.isPlaying && !EditorPrefs.GetBool(PlayPrefKey))
                return;

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
            IHaveIconPreview icon    = go.GetIcon();
            IconPreview      preview = new(icon);

            if (selectionLength <= 3) {
                if (selected || icon == null) {
                    if (go.TryGetComponent(out SpriteRenderer renderer)) {
                        preview = new(renderer.sprite.n()?.ToTexture2D(), renderer.color);
                    }
#if DEVELOPMENT_TOOLS_EDITOR_UNITY_UI
                    else if (go.TryGetComponent(out Image image)) {
                        preview = new(image.sprite.n()?.ToTexture2D().n() ?? Texture2D.whiteTexture, image.color);
                        if (!preview.Icon)
                            preview.Icon = Texture2D.whiteTexture;
                    }
                    else if (go.TryGetComponent(out RawImage rawImage)) {
                        preview = new(rawImage.texture.n()?.Read().n() ?? Texture2D.whiteTexture, rawImage.color);
                    }
#endif
                    // else if (has ExtractableInfo)
                    else
                        preview.Icon = null;

                    // if (icon?.Icon)
                    //     icon.Icon.SetFilter(FilterMode.Point).Trim(true);

                    go.SetIcon(preview, true);
                }
            }

            if (!preview.Icon)
                return;

            Rect iconRect = new(selectionRect.x, selectionRect.y, 16, 16);

            // colored indicator to the left of the icon
            // Rect colorCodeRect = new(iconRect.x - 20, iconRect.y, 4, 16);
            // Color color = Color.clear;
            // if (!colorCodes.TryGetValue(color, out Texture colorCode))
            //     colorCode = color.ToTexture();
            // GUI.DrawTexture(colorCodeRect, colorCode, ScaleMode.StretchToFill);

            preview.DrawIcon(iconRect, ScaleMode.ScaleToFit, selected, active);
        }

        // TODO use InfoContainer and merge with GetIcon from EditorHelper
        private static readonly Dictionary<GlobalObjectId, ((string text, FontStyle style, Color color) content, long timestamp)> texts = new();

        private static void DrawText(GameObject go, int selectionLength, bool selected, bool active, Rect selectionRect) {
            GlobalObjectId id = go.GlobalId();
            texts.TryGetValue(id, out ((string text, FontStyle style, Color color) content, long timestamp) value);
            bool textDisabled = !go.activeInHierarchy;

            if (selectionLength <= 3) {
                if (selected || value.content.text == null && DateTime.Now.Ticks - value.timestamp > TimeSpan.FromSeconds(5).Ticks) {
                    if (go.TryGetComponent(out Text text)) {
                        value.content = (text.text, text.fontStyle, text.color);
                        textDisabled  = !text.isActiveAndEnabled;
                    }
#if DEVELOPMENT_TOOLS_EDITOR_TMP
                    else if (go.TryGetComponent(out TextMeshProUGUI tmpro)) {
                        value.content = Process(tmpro);
                        textDisabled  = !tmpro.isActiveAndEnabled;
                    }
#endif
                    else
                        value.content.text = null;

                    texts.TryAdd(id, (value.content, DateTime.Now.Ticks));
                }
            }

            if (value.content.text.IsNullOrWhiteSpace())
                return;

            selectionRect.xMin += 16 + GUI.skin.label.CalcSize(new(go.name)).x;
            EditorGUI.BeginDisabledGroup(textDisabled);
            GUI.Label(selectionRect, value.content.text, new(GUI.skin.label) { richText = true, fontStyle = value.content.style, normal = { textColor = value.content.color }, hover = { textColor = value.content.color }, alignment = TextAnchor.MiddleRight, fontSize = 12, clipping = TextClipping.Ellipsis });
            EditorGUI.EndDisabledGroup();
        }

#if DEVELOPMENT_TOOLS_EDITOR_TMP
        private static (string text, FontStyle style, Color color) Process(TextMeshProUGUI tmpro) {
            string     text   = tmpro.text;
            FontStyles styles = tmpro.fontStyle;

            if ((styles & FontStyles.UpperCase) != 0) text     = text.ToUpper();
            if ((styles & FontStyles.LowerCase) != 0) text     = text.ToLower();
            if ((styles & FontStyles.SmallCaps) != 0) text     = $"<smallcaps>{text}</smallcaps>";
            if ((styles & FontStyles.Bold) != 0) text          = $"<b>{text}</b>";
            if ((styles & FontStyles.Italic) != 0) text        = $"<i>{text}</i>";
            if ((styles & FontStyles.Underline) != 0) text     = $"<u>{text}</u>";
            if ((styles & FontStyles.Strikethrough) != 0) text = $"<s>{text}</s>";
            if ((styles & FontStyles.Superscript) != 0) text   = $"<sup>{text}</sup>";
            if ((styles & FontStyles.Subscript) != 0) text     = $"<sub>{text}</sub>";

            bool b = (styles & FontStyles.Bold) != 0;
            bool i = (styles & FontStyles.Italic) != 0;
            FontStyle style = b && i
                ? FontStyle.BoldAndItalic
                : b
                    ? FontStyle.Bold
                    : i
                        ? FontStyle.Italic
                        : FontStyle.Normal;

            return (text, style, tmpro.color);
        }
#endif

    }

}
#endif