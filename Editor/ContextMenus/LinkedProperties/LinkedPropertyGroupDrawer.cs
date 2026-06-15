using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.ContextMenus {

#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
    public class LinkedPropertyGroupDrawer : OdinValueDrawer<LinkedPropertyGroup> {

        protected override void Initialize() {
            foreach (LinkedProperty property in ValueEntry.SmartValue.LinkedProperties)
                property.UpdateObjectName();
        }

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.y      += EditorGUIUtility.standardVerticalSpacing / 2;
            rect.height =  EditorGUIUtility.singleLineHeight;
            rect.width  /= 3;
            rect.x      += rect.width;

            if (GUI.Button(rect, "▼ Push 1st to all ▼"))
                foreach (LinkedProperty linkedProperty in ValueEntry.SmartValue.LinkedProperties)
                    linkedProperty.SetPropertyValue(ValueEntry.SmartValue.LinkedProperties[0].GetPropertyValue());
        }
#else
    [CustomPropertyDrawer(typeof(LinkedPropertyGroup))]
    public class LinkedPropertyGroupDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            float h = EditorGUIUtility.singleLineHeight;
            float y = position.y;

            EditorGUI.PropertyField(new(position.x, y, position.width, h), property.FindPropertyRelative("Name"));
            y += h + 2;
            EditorGUI.PropertyField(new(position.x, y, position.width, h), property.FindPropertyRelative("PredominantValue"));
            y += h + 2;
            EditorGUI.PropertyField(new(position.x, y, position.width, h), property.FindPropertyRelative("LinkedProperties"), true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property.FindPropertyRelative("LinkedProperties"), true) + 50;

#endif

    }

}