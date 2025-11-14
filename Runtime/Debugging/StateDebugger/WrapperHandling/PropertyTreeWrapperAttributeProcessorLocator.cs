#if UNITY_EDITOR && !SIMULATE_BUILD
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;

namespace DevelopmentTools.Debugging.StateDebugger.WrapperHandling {

    public class PropertyTreeWrapperAttributeProcessorLocator : OdinAttributeProcessorLocator {

        private static readonly PropertyTreeWrapperAttributeProcessor Processor = new();

        public override List<OdinAttributeProcessor> GetSelfProcessors(InspectorProperty property) =>
            new(DefaultOdinAttributeProcessorLocator.Instance.GetSelfProcessors(property)) { Processor };

        public override List<OdinAttributeProcessor> GetChildProcessors(InspectorProperty parentProperty, MemberInfo member) =>
            new(DefaultOdinAttributeProcessorLocator.Instance.GetChildProcessors(parentProperty, member)) { Processor };

    }

}
#endif