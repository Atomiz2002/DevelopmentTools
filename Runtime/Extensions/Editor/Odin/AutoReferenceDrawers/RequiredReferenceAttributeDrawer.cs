// #define ruin_my_life

#if ruin_my_life
using _PristineMeadow.Extensions.Attributes.References;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

// ReSharper disable once UnusedType.Global
namespace _PristineMeadow.Extensions.Editor.Odin.AutoReferenceDrawers {

    public class RequiredReferenceAttributesDrawer<TAttribute, TReference> : OdinAttributeDrawer<TAttribute, TReference>
        where TAttribute : RequiredReferenceAttribute
        where TReference : Object {

        // protected override void DrawPropertyLayout(GUIContent label) {
        //     if (ValueEntry == null) return;
        //
        //     if (ValueEntry.ParentType.IsArray) {
        //         CallNextDrawer(label);
        //     }
        //
        //     if (ValueEntry.SmartValue) return;
        //
        //     GUIHelper.PushColor(Color.red);
        //     CallNextDrawer(label);
        //     GUIHelper.PopColor();
        // }

    }

    public class RequiredReferenceAttributeDrawer<TComponent> : RequiredReferenceAttributesDrawer<RequiredReferenceAttribute, TComponent> where TComponent : Object {}

}
#endif