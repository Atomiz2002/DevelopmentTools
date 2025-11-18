using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DevelopmentTools.Editor.Extensions.Editor.DropZone {

    [AttributeUsage(AttributeTargets.Field)]
    [DontApplyToListElements]
    public abstract class DropZoneAttribute : PropertyAttribute {

        public string AddMethodName { get; }

        public DropZoneAttribute(string addMethodName) {
            AddMethodName = addMethodName;
        }

    }

}