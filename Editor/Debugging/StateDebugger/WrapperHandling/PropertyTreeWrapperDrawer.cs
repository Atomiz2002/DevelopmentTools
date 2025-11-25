#if UNITY_EDITOR && !SIMULATE_BUILD && DEVELOPMENT_TOOLS_ODIN_INSPECTOR
using DevelopmentEssentials.Extensions.Unity;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace DevelopmentTools.Editor.Debugging.StateDebugger.WrapperHandling {

    public class PropertyTreeWrapperDrawer : OdinAttributeDrawer<PropertyTreeWrapperAttribute> {

        protected override void DrawPropertyLayout(GUIContent label) {
            if (Attribute.Debug) {
                GUIHelper.PushContentColor(Color.red);
                GUIHelper.PushLabelColor(Color.red);
                CallNextDrawer(label);
                GUIHelper.PopLabelColor();
                GUIHelper.PopContentColor();
                return;
            }

            if (!Attribute.DiffNext && !Attribute.DiffPrev) {
                GUIHelper.PushContentColor(Color.white.A(.6f));
                GUIHelper.PushLabelColor(Color.white.A(.6f));
                CallNextDrawer(label);
                GUIHelper.PopLabelColor();
                GUIHelper.PopContentColor();
                return;
            }

            GUIHelper.PushContentColor(Color.white);
            GUIHelper.PushLabelColor(Color.white);
            CallNextDrawer(label);
            GUIHelper.PopLabelColor();
            GUIHelper.PopContentColor();

            Rect      rect = GUILayoutUtility.GetLastRect();
            const int R    = 3;

            Rect left = rect;
            left.width =  5;
            left.x     -= left.width - 3;

            Rect right = rect;
            right.width =  5;
            right.x     += rect.width - 3;

            if (Attribute.DiffNext) SirenixEditorGUI.DrawRoundRect(left, Color.red, R, Color.black, 1);
            if (Attribute.DiffPrev) SirenixEditorGUI.DrawRoundRect(right, Color.red, R, Color.black, 1);
        }

    }

}
#endif