using System;
using UnityEngine;
#if DEVELOPMENT_TOOLS_RUNTIME_ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#endif

namespace DevelopmentTools {

    [Serializable]
#if DEVELOPMENT_TOOLS_RUNTIME_ODIN_INSPECTOR
    [InlineProperty]
    [HideReferenceObjectPicker]
#endif
    public struct IconPreview : IHaveIconPreview {

#if DEVELOPMENT_TOOLS_RUNTIME_ODIN_INSPECTOR
        [field: OdinSerialize, HideLabel, PreviewField]
#else
        [field: SerializeField]
#endif
        public Texture Icon { get; set; }
#if DEVELOPMENT_TOOLS_RUNTIME_ODIN_INSPECTOR
        [field: OdinSerialize, HideLabel]
#else
        [field: SerializeField]
#endif
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