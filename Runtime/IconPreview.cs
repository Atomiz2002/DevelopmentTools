using System;
using DevelopmentTools.ODIN_INSPECTOR;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace DevelopmentTools {

    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    public struct IconPreview : IHaveIconPreview {

        [field: OdinSerialize, HideLabel, PreviewField]
        public Texture2D Icon { get; set; }
        [field: OdinSerialize, HideLabel]
        public Color Color { get; set; }

        public static readonly IconPreview White = new(null);
        public static readonly IconPreview Empty = new(null, Color.clear);

        public IconPreview(Texture2D icon) {
            Icon  = icon;
            Color = Color.white;
        }

        public IconPreview(Texture2D icon, Color color) {
            Icon  = icon;
            Color = color;
        }

    }

}