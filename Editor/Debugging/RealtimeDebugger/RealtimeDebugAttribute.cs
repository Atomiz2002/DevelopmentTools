using System;
using Sirenix.Serialization;

namespace DevelopmentTools.Editor.Debugging.RealtimeDebugger {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field /* | AttributeTargets.Property |*/ /*AttributeTargets.Method*/)]
    public class RealtimeDebugAttribute : OdinSerializeAttribute {

        public string Label;
        public string LabelGetter;
        public string ValueGetter;
        public string DebugCondition;
        public bool   AsString;

    }

}