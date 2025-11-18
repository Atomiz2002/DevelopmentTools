#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using DevelopmentTools.Editor.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DevelopmentTools.Editor.Debugging.StateDebugger.WrapperHandling {

    [Serializable]
    public class PropertyTreeWrapper<T> {

        [PropertyTreeWrapper]
        [InlineProperty]
        [HideLabel]
        [HideReferenceObjectPicker]
        [SerializeReference]
        public object obj;

        public PropertyTreeWrapper(T t) => obj = t.CloneJSON();

    }

}
#endif