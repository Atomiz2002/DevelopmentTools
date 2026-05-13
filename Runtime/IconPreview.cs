using UnityEngine;

namespace DevelopmentTools.DevelopmentTools {

    public struct IconPreview : IHaveIconPreview {

        public Texture Icon  { get; }
        public Color   Color { get; }

        public IconPreview(Texture icon, Color color) {
            Icon  = icon;
            Color = color;
        }

    }

}