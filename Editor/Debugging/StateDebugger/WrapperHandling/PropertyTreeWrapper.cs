#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR && !SIMULATE_BUILD
using System;
using DevelopmentTools.DevelopmentTools.Editor.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DevelopmentTools.DevelopmentTools.Editor.Debugging.StateDebugger.WrapperHandling {

    [Serializable]
    public class PropertyTreeWrapper<T> {

        [PropertyTreeWrapper]
        [InlineProperty]
        [HideLabel]
        [HideReferenceObjectPicker]
        [SerializeReference]
        public object obj;

        public PropertyTreeWrapper(T t) => obj = t.CloneJson();

    }

}
#endif