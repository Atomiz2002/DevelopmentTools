using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Editor.Extensions.Editor.DropZone {

    public class DropZoneDrawer<T> : OdinAttributeDrawer<DropZoneAttribute, T> {

        private const float DropZoneHeight = 50f;
        private const float Spacing        = 5f;

        private Texture2D inspectorBg;
        private GUIStyle  dropAreaStyle;

        protected override void Initialize() {
            inspectorBg = new(1, 1);
            inspectorBg.SetPixel(0, 0, new(0.2f, 0.2f, 0.2f));
            inspectorBg.Apply();

            dropAreaStyle = new(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 14,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = Color.white, background = inspectorBg },
                border    = new()
            };
        }

        protected override void DrawPropertyLayout(GUIContent label) {
            CallNextDrawer(label);

            Rect dropZoneRect = EditorGUILayout.GetControlRect(GUILayout.Height(DropZoneHeight + Spacing));

            dropZoneRect.y      += Spacing;
            dropZoneRect.height =  DropZoneHeight;

            GUILayout.Space(Spacing);

            EditorHelper.DrawDropZone<T>(dropZoneRect, draggedObjects => {
                Object targetObject = (Object) Property.Tree.WeakTargets[0];
                Undo.RecordObject(targetObject, "DropZone: Add Item");

                DropZoneAttribute dropZoneAttr = Attribute;

                MethodInfo addMethod = Property.ParentType.GetMethod(
                    dropZoneAttr.AddMethodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (addMethod == null) {
                    SirenixEditorGUI.ErrorMessageBox($"Invalid add method name: {dropZoneAttr.AddMethodName}");
                    return;
                }

                foreach (T draggedObject in draggedObjects)
                    addMethod.Invoke(targetObject, new object[] { draggedObject });

                EditorUtility.SetDirty(targetObject);
            });

            GUI.Box(dropZoneRect, "Drop items here", dropAreaStyle);
            EditorHelper.DrawDashedBorder(dropZoneRect, 10f, 5f, 5f, Color.grey);
        }

    }

}