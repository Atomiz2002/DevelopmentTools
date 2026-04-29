#if DEVELOPMENT_TOOLS_RUNTIME_ODIN_INSPECTOR
using System;
using System.Diagnostics;
using DevelopmentEssentials.Extensions.CS;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
#endif

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class ColoredBoxGroupAttribute : BoxGroupAttribute {

    public Color  Color;
    public string ColorGetter;
    public bool   Stripe;
    public bool   Background = true;

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
        ColorGetter  = colorGetter;
        BoldLabel    = boldLabel;
        LabelText    = label;
        MarginTop    = marginTop;
        MarginBottom = marginBottom;
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

#if UNITY_EDITOR
    public ValueResolver<Color> valueResolver;
    public void Resolve(InspectorProperty property) {
        if (ColorGetter.IsNullOrEmpty())
            return;

        valueResolver = ValueResolver.Get<Color>(property, ColorGetter.StartAt("@", false));
        Color         = valueResolver.GetValue();
    }
#endif

}
#endif