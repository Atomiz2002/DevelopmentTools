using UnityEngine;

namespace DevelopmentTools {

    public interface IHaveIconPreview {

        public Texture Icon      { get; }
        public Color?  IconColor => Color.white;

    }

}