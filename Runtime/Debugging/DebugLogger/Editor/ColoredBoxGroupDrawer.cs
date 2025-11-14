#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace DevelopmentTools.Debugging.DebugLogger.Editor {

    public class ColoredBoxGroupDrawer : OdinGroupDrawer<ColoredBoxGroupAttribute> {

        protected override void DrawPropertyLayout(GUIContent label) {
            Attribute.Resolve(Property);
            Attribute.valueResolver?.DrawError();

            GUILayout.Space(Attribute.MarginTop);

            string headerLabel = Attribute.LabelText;

            if (Attribute.ShowLabel)
                headerLabel ??= string.Empty;

            if (Attribute.Color.a > 0)
                GUIHelper.PushColor(Attribute.Color);

            SirenixEditorGUI.BeginBox();

            if (Attribute.ShowLabel)
                SirenixEditorGUI.BeginBoxHeader();

            if (Attribute.ShowLabel) {
                SirenixEditorGUI.Title(headerLabel,
                    null,
                    Attribute.CenterLabel ? TextAlignment.Center : TextAlignment.Left,
                    false,
                    Attribute.BoldLabel);

                SirenixEditorGUI.EndBoxHeader();
            }

            if (Attribute.Color.a > 0)
                GUIHelper.PopColor();

            foreach (InspectorProperty t in Property.Children)
                t.Draw();

            SirenixEditorGUI.EndBox();

            GUILayout.Space(Attribute.MarginBottom);
        }

    }

}
#endif