using System;
using DevelopmentEssentials.Extensions.CS;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Color = UnityEngine.Color;

namespace DevelopmentTools.Attributes.PreviewTexture2D {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PreviewTextureAttribute : PreviewFieldAttribute {

#if UNITY_EDITOR && !SIMULATE_BUILD

        public Color  OutlineColor = Color.black;
        public string OutlineColorGetter;

        public int    OutlineThickness = 1;
        public string OutlineThicknessGetter;

        public Color  BackgroundColor;
        public string BackgroundColorGetter;

        public bool DrawIfNull   = true;
        public bool ExpandHeight = true;

        public PreviewTextureAttribute() {}

        public PreviewTextureAttribute(Color outlineColor, int outlineThickness, Color backgroundColor, bool drawIfNull = false) {
            OutlineColor     = outlineColor;
            OutlineThickness = outlineThickness;
            BackgroundColor  = backgroundColor;
            DrawIfNull       = drawIfNull;
        }

        public PreviewTextureAttribute(string outlineColorGetter, string outlineThicknessGetter, string backgroundColorGetter = null, bool drawIfNull = false) {
            OutlineColorGetter     = outlineColorGetter;
            OutlineThicknessGetter = outlineThicknessGetter;
            BackgroundColorGetter  = backgroundColorGetter;
            DrawIfNull             = drawIfNull;
        }

        public PreviewTextureAttribute(string outlineColorGetter, int outlineThickness, string backgroundColorGetter = null, bool drawIfNull = false) {
            OutlineColorGetter    = outlineColorGetter;
            OutlineThickness      = outlineThickness;
            BackgroundColorGetter = backgroundColorGetter;
            DrawIfNull            = drawIfNull;
        }

        public void Resolve(InspectorProperty property) {
            if (OutlineColorGetter != null)
                OutlineColor = ValueResolver.Get<Color>(property, OutlineColorGetter.StartAt("@", false)).GetValue();

            if (OutlineThicknessGetter != null)
                OutlineThickness = ValueResolver.Get<int>(property, OutlineThicknessGetter.StartAt("@", false)).GetValue();

            if (BackgroundColorGetter != null)
                BackgroundColor = ValueResolver.Get<Color>(property, BackgroundColorGetter.StartAt("@", false)).GetValue();
        }

#endif

    }

}