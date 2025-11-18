#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using Sirenix.OdinInspector;

namespace DevelopmentTools.Editor.Debugging.StateDebugger.WrapperHandling {

    [DontApplyToListElements]
// [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class PropertyTreeWrapperAttribute : Attribute {

        public bool DiffPrev;
        public bool DiffNext;
        public bool Debug;

    }

}
#endif