#if UNITY_EDITOR && !SIMULATE_BUILD && DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using System;
using DevelopmentTools.Editor.Editor.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DevelopmentTools.Editor.Editor.Debugging.StateDebugger.WrapperHandling {

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