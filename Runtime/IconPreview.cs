#if DEVELOPMENT_TOOLS_RUNTIME_ODIN_INSPECTOR
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace DevelopmentTools {

    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    public struct IconPreview : IHaveIconPreview {

        [field: OdinSerialize, HideLabel, PreviewField]
        public Texture Icon { get; set; }
        [field: OdinSerialize, HideLabel]
        public Color Color { get; set; }

        public static readonly IconPreview White = new(icon: null);
        public static readonly IconPreview Empty = new(null, Color.clear);

        public IconPreview(Texture icon) {
            Icon  = icon;
            Color = Color.white;
        }

        public IconPreview(Texture icon, Color color) {
            Icon  = icon;
            Color = color;
        }

        public IconPreview(IHaveIconPreview iconPreview) {
            Icon  = iconPreview?.Icon;
            Color = iconPreview?.Color ?? Color.clear;
        }

    }

}
#endif