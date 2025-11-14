#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using DevelopmentTools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DevelopmentTools.Debugging.StateDebugger.WrapperHandling {

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