#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR && UNITY_EDITOR && !SIMULATE_BUILD
using DevelopmentEssentials.Extensions.CS;
using DevelopmentTools.ODIN_INSPECTOR;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace DevelopmentTools.Editor.AttributeDrawers {

    public class ColoredBoxGroupDrawer : OdinGroupDrawer<ColoredBoxGroupAttribute> {

        protected override void DrawPropertyLayout(GUIContent label) {
            Attribute.Resolve(Property);
            Attribute.valueResolver?.DrawError();

            if (Attribute.Background)
                BeginColoredBox(Attribute.Color, Attribute.ShowLabel ? Attribute.LabelText : null, Attribute.BoldLabel, Attribute.CenterLabel);
            else if (Attribute.ShowLabel)
                BeginTitledBox(Attribute.LabelText, Attribute.BoldLabel, Attribute.CenterLabel);
            else
                SirenixEditorGUI.BeginBox();

            foreach (InspectorProperty t in Property.Children)
                t.Draw();

            if (Attribute.Stripe)
                EndBox(Attribute.Color);
            else
                SirenixEditorGUI.EndBox();
        }

        public static void BeginStripedBox() => SirenixEditorGUI.BeginBox();

        /// <inheritdoc cref="BeginTitledBox"/>
        public static void BeginColoredBox(Color color, string labelText = null, bool bold = false, bool centerLabel = false) {
            GUIHelper.PushColor(color);

            BeginTitledBox(labelText, bold, centerLabel);

            GUIHelper.PopColor();
        }

        /// End with <see cref="Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox"/>
        public static void BeginTitledBox(string labelText, bool bold = false, bool centerLabel = false) {
            SirenixEditorGUI.BeginBox();

            if (!labelText.IsNullOrWhiteSpace()) {
                SirenixEditorGUI.BeginBoxHeader();

                SirenixEditorGUI.Title(labelText,
                    null,
                    centerLabel ? TextAlignment.Center : TextAlignment.Left,
                    false,
                    bold);

                SirenixEditorGUI.EndBoxHeader();
            }
        }

        // public static void EndTitledBox(Color stripeColor = default)  => EndBox(stripeColor);
        // public static void EndColoredBox(Color stripeColor = default) => EndBox(stripeColor);
        // public static void EndStripedBox(Color stripeColor = default) => EndBox(stripeColor);

        public static void EndBox(Color stripeColor = default) {
            SirenixEditorGUI.EndBox();

            if (stripeColor.a > 0) {
                Rect boxRect = GUILayoutUtility.GetLastRect().Expand(-1);
                boxRect.xMax = boxRect.x + 3;
                SirenixEditorGUI.DrawRoundRect(boxRect, stripeColor, 5, 0, 5, 0);
            }
        }

    }

}
#endif