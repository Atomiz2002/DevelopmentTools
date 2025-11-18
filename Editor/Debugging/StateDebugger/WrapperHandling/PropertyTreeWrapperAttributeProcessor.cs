#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;

namespace DevelopmentTools.Editor.Debugging.StateDebugger.WrapperHandling {

    public class PropertyTreeWrapperAttributeProcessor : OdinAttributeProcessor {

        public override bool CanProcessSelfAttributes(InspectorProperty property) =>
            property.Attributes /*.Log(property)*/.HasAttribute<PropertyTreeWrapperAttribute>();

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes) {
            attributes /*.Log(property)*/.RemoveAttributeOfType<PropertyTreeWrapperAttribute>();
            attributes.Add<PropertyTreeWrapperAttribute>();
        }

        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member) =>
            parentProperty /*.Log(parentProperty)*/.Attributes.HasAttribute<PropertyTreeWrapperAttribute>();

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes) {
            attributes /*.Log()*/.RemoveAttributeOfType<PropertyTreeWrapperAttribute>();
            attributes.Add<PropertyTreeWrapperAttribute>();
        }

    }

}
#endif