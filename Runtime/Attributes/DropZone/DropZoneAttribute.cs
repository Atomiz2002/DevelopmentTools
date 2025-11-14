using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DevelopmentTools.Attributes.DropZone {

    [AttributeUsage(AttributeTargets.Field)]
    [DontApplyToListElements]
    public class DropZoneAttribute : PropertyAttribute {

        public string AddMethodName { get; }

        public DropZoneAttribute(string addMethodName) {
            AddMethodName = addMethodName;
        }

    }

}