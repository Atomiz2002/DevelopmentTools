using UnityEditor;
#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace DevelopmentTools.Editor.ContextMenus {

#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
    public class LinkedPropertiesEditor : OdinEditor {

#else
    [CustomEditor(typeof(LinkedPropertiesSO))]
    public class LinkedPropertiesEditor : UnityEditor.Editor {
#endif

        public override void OnInspectorGUI() {
            serializedObject.Update();
            SerializedProperty prop = serializedObject.FindProperty(nameof(LinkedPropertiesSO.LinkedGroups));
            prop.isExpanded = true;
            EditorGUILayout.PropertyField(prop, true);
            serializedObject.ApplyModifiedProperties();
        }

    }

}