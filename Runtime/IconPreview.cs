using UnityEngine;

namespace DevelopmentTools {

    public struct IconPreview : IHaveIconPreview {

        public Texture Icon  { get; set; }
        public Color   Color { get; set; }

        public static readonly IconPreview White = new(null);
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