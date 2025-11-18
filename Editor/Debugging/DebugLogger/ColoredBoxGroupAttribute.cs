using System;
using DevelopmentEssentials.Extensions.CS;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

namespace DevelopmentTools.Editor.Debugging.DebugLogger {

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class ColoredBoxGroupAttribute : BoxGroupAttribute {

#if UNITY_EDITOR

        public           Color                Color;
        private readonly string               colorExpression;
        public           ValueResolver<Color> valueResolver;

        public readonly bool BoldLabel;
        public readonly int  MarginTop;
        public readonly int  MarginBottom;

        public ColoredBoxGroupAttribute(
            string group,
            string colorGetter,
            string label = "",
            int marginTop = 5,
            int marginBottom = 5,
            bool showLabel = true,
            bool centerLabel = false,
            bool boldLabel = false,
            float order = 0
        ) : base(group, showLabel, centerLabel, order) {
            colorExpression = colorGetter;
            BoldLabel       = boldLabel;
            LabelText       = label;
            MarginTop       = marginTop;
            MarginBottom    = marginBottom;
        }

        public ColoredBoxGroupAttribute(
            string group,
            float r, float g, float b, float a,
            string label = "",
            int marginTop = 5,
            int marginBottom = 5,
            bool showLabel = true,
            bool centerLabel = false,
            bool boldLabel = false,
            float order = 0
        ) : base(group, showLabel, centerLabel, order) {
            Color        = new(r, g, b, a);
            BoldLabel    = boldLabel;
            LabelText    = label;
            MarginTop    = marginTop;
            MarginBottom = marginBottom;
        }

        public ColoredBoxGroupAttribute(
            string group,
            float a,
            string label = "",
            int marginTop = 5,
            int marginBottom = 5,
            bool showLabel = true,
            bool centerLabel = false,
            bool boldLabel = false,
            float order = 0
        ) : base(group, showLabel, centerLabel, order) {
            Color        = new(0, 0, 0, a);
            BoldLabel    = boldLabel;
            LabelText    = label;
            MarginTop    = marginTop;
            MarginBottom = marginBottom;
        }

        public void Resolve(InspectorProperty property) {
            if (colorExpression.IsNullOrEmpty())
                return;

            valueResolver = ValueResolver.Get<Color>(property, colorExpression.StartAt("@", false));
            Color         = valueResolver.GetValue();
        }

#endif

    }

}