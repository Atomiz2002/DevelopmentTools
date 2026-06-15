using DevelopmentEssentials.Extensions.Unity;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.ContextMenus {

    [CustomPropertyDrawer(typeof(LinkedProperty))]
    public class LinkedPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (GUILayout.Button(property.FindPropertyRelative(nameof(LinkedProperty.DisplayName)).stringValue))
                property.FindPropertyRelative(nameof(LinkedProperty.GlobalIdString)).stringValue.ToGlobalObjectId().ToObject().Select();
        }

    }

}