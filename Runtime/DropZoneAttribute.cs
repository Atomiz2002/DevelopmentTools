using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DevelopmentTools {

    [AttributeUsage(AttributeTargets.Field)]
    [DontApplyToListElements]
    public class DropZoneAttribute : PropertyAttribute {

        public string AddMethodName { get; }

        public DropZoneAttribute(string addMethodName) {
            AddMethodName = addMethodName;
        }

    }

}