using System;
using System.Collections;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Editor.ContextMenus {

    [Serializable]
    public class LinkedProperty {

        private const string delimiter = "~~~";

        public GlobalObjectId GlobalId;
        public string         GlobalIdString;
        public string         PropertyPath;
        public string         Type;
        public string         DisplayType;
        public string         LinkedId;
        public string         PropertyName;
        public string         DisplayName;

        public LinkedProperty(SerializedProperty property) {
            GlobalId       = property.serializedObject.targetObject.GlobalId();
            GlobalIdString = GlobalId.ToString();
            PropertyPath   = property.propertyPath;
            Type           = property.type;
            DisplayType    = Type.Replace("<", string.Empty).Replace(">", string.Empty).Replace("PPtr", string.Empty);
            LinkedId       = string.Join(delimiter, GlobalId, PropertyPath, Type);
            PropertyName   = property.displayName;
            UpdateObjectName();
        }

        public void UpdateObjectName() => DisplayName = $"{GlobalId.ToObject().name}.{PropertyName}.{PropertyPath}";

        public void SetPropertyValue(object value, bool? isArray = null, Object obj = null) {
            if (!obj)
                obj = GlobalId.ToObject();

            SerializedObject   so = new(obj);
            SerializedProperty sp = so.FindProperty(PropertyPath);

            if (sp == null)
                return;

            if (isArray.GetValueOrDefault(sp.isArray && sp.propertyType != SerializedPropertyType.String)) {
                IList list = (IList) value;
                sp.arraySize = list.Count;

                for (int i = 0; i < list.Count; i++)
                    sp.GetArrayElementAtIndex(i).boxedValue = list[i];
            }
            else {
                sp.boxedValue = value;
            }

            so.ApplyModifiedProperties();
        }

        public object GetPropertyValue() {
            Object obj = GlobalId.ToObject();

            SerializedObject   so = new(obj);
            SerializedProperty sp = so.FindProperty(PropertyPath);

            bool isArray = sp.isArray && sp.propertyType != SerializedPropertyType.String;

            if (!isArray)
                return sp.boxedValue;

            object[] values = new object[sp.arraySize];

            for (int i = 0; i < sp.arraySize; i++)
                values[i] = sp.GetArrayElementAtIndex(i).boxedValue;

            return values;
        }

        public object GetPropertyValue(SerializedProperty prop, out bool isArray) {
            isArray = prop.isArray && prop.propertyType != SerializedPropertyType.String;

            if (!isArray)
                return prop.boxedValue;

            object[] values = new object[prop.arraySize];

            for (int i = 0; i < prop.arraySize; i++)
                values[i] = prop.GetArrayElementAtIndex(i).boxedValue;

            return values;
        }

    }

}