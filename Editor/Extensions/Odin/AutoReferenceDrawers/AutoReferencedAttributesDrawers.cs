#if ruin_my_life
using System;
using System.Reflection;
using _PristineMeadow.Extensions.Attributes.References;
using _PristineMeadow.Extensions.Unity;
using Sirenix.Utilities.Editor;
using UnityEngine;
using Color = System.Drawing.Color;
using Object = UnityEngine.Object;

// ReSharper disable UnusedType.Global

namespace _PristineMeadow.Extensions.Editor.Odin.AutoReferenceDrawers {

    public abstract class AutoReferencedAttributesDrawer<TAttribute, TReference> : RequiredReferenceAttributesDrawer<TAttribute, TReference>
        where TAttribute : RequiredReferenceAttribute
        where TReference : Object {

        protected abstract Func<PristineBehaviour, TReference> AutoReferenceAction { get; }

        // protected override void DrawPropertyLayout(GUIContent label) {
        //     if (ValueEntry == null) return;
        //
        //     base.DrawPropertyLayout(label);
        //
        //     if (ValueEntry.SmartValue) return;
        //     if (AutoReferenceAction == null) return;
        //     if (!Property.Parent.ValueEntry.WeakSmartValue.Inherits(out PristineBehaviour behaviour)) return;
        //
        //     try {
        //         ValueEntry.SmartValue = AutoReferenceAction(behaviour);
        //
        //         if (ValueEntry.SmartValue)
        //             behaviour.OnAutoReferenced();
        //     }
        //     catch (Exception e) {
        //         SirenixEditorGUI.DetailedMessageBox(e.Message, e.StackTrace, MessageType.Error, true);
        //     }
        // }

    }

    public class TryAddComponentAttributeDrawer<TComponent> : AutoReferencedAttributesDrawer<TryAddComponentAttribute, TComponent>
        where TComponent : Component {

        protected override Func<PristineBehaviour, TComponent> AutoReferenceAction =>
            behaviour => behaviour.TryAddComponent(out TComponent component, Attribute.FieldName)
                ? component
                : null;

    }

    public class TryGetComponentAttributeDrawer<TComponent> : AutoReferencedAttributesDrawer<TryGetComponentAttribute, TComponent>
        where TComponent : Component {

        protected override Func<PristineBehaviour, TComponent> AutoReferenceAction =>
            behaviour => behaviour.TryGetComponent(out TComponent component)
                ? component
                : null;

    }

    public class GetComponentInParentAttributeDrawer<TComponent> : AutoReferencedAttributesDrawer<GetComponentInParentAttribute, TComponent>
        where TComponent : Component {

        protected override Func<PristineBehaviour, TComponent> AutoReferenceAction =>
            behaviour => behaviour.GetComponentInParent(out TComponent component)
                ? component
                : null;

    }

    public class GetOrInstantiateChildAttributeDrawer<TComponent> : AutoReferencedAttributesDrawer<GetOrInstantiateChildAttribute, TComponent>
        where TComponent : Component {

        protected override Func<PristineBehaviour, TComponent> AutoReferenceAction =>
            behaviour => {
                GameObject parent = Attribute.ParentFieldName != null
                    ? (Property.ParentType
                        .GetField(Attribute.ParentFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?.GetValue(Property?.Parent?.ValueEntry?.WeakSmartValue) as MonoBehaviour)?.gameObject
                    : behaviour.gameObject;

                if (parent)
                    return parent.GetOrInstantiateChild(Attribute.ChildName, out TComponent component)
                        ? component
                        : null;

                SirenixEditorGUI.ErrorMessageBox($"Parent {Attribute.ParentFieldName.Colored(Color.Red)} is null".Size(12));
                return null;
            };

    }

}
#endif