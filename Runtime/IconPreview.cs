using UnityEngine;

namespace DevelopmentTools.DevelopmentTools {

    public struct IconPreview : IHaveIconPreview {

        public Texture Icon  { get; }
        public Color   Color { get; }

        public static readonly IconPreview Empty = new(null, Color.clear);

        public IconPreview(Texture icon) {
            Icon  = icon;
            Color = Color.white;
        }

        public IconPreview(Texture icon, Color color) {
            Icon  = icon;
            Color = color;
        }

    }

}