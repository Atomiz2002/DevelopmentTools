#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DevelopmentTools.DevelopmentTools.Editor.Extensions.DropZone {

    [AttributeUsage(AttributeTargets.Field)]
    [DontApplyToListElements]
    public abstract class DropZoneAttribute : PropertyAttribute {

        public string AddMethodName { get; }

        public DropZoneAttribute(string addMethodName) {
            AddMethodName = addMethodName;
        }

    }

}
#endif