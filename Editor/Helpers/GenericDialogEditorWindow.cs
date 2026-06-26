using System;
using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Editor.Helpers {

    public sealed class GenericDialogEditorWindow : EditorWindow {

        private List<FieldEntry>                   fields = new();
        private Action<Dictionary<string, object>> onConfirm;
        private string                             okText, cancelText;
        private bool                               focusedFirstControl;

        public static void Show(string title, FieldEntry field, Action<object> onConfirm, string ok = "Ok", string cancel = "Cancel")
            => Show(title, new() { field }, dict => onConfirm.InvokeSafe(dict[field.label]), ok, cancel);

        public static void Show(string title, List<FieldEntry> fields, Action<Dictionary<string, object>> onConfirm, string ok = "Ok", string cancel = "Cancel") {
            GenericDialogEditorWindow window = CreateInstance<GenericDialogEditorWindow>();
            window.titleContent = new(title);
            window.fields       = fields;
            window.onConfirm    = onConfirm;
            window.okText       = ok;
            window.cancelText   = cancel;
            window.ShowUtility();
        }

        private void OnGUI() {
            foreach (FieldEntry f in fields) {
                f.value = DrawDynamicField(f.label, f.value);
                if (f.errored)
                    EditorGUILayout.HelpBox(f.error.InvokeSafe(f.value) ?? "Invalid", MessageType.Error);
            }

            if (GUILayout.Button(okText)) {
                bool allValid = true;

                foreach (FieldEntry f in fields) {
                    if (f.validation != null && !f.validation.InvokeSafe(f.value)) {
                        f.errored = true;
                        allValid  = false;
                    }
                    else {
                        f.errored = false;
                    }
                }

                if (allValid) {
                    onConfirm.InvokeSafe(fields.ToDictionary(f => f.label, f => f.value));
                    Close();
                }
            }

            if (!cancelText.IsNullOrEmpty() && GUILayout.Button(cancelText))
                Close();
        }

        private object DrawDynamicField(string label, object obj) {
            if (!focusedFirstControl)
                GUI.SetNextControlName(nameof(GenericDialogEditorWindow) + label);

            GUIContent content = new(label);

            object field = obj switch {
                int i             => EditorGUILayout.IntField(content, i),
                string s          => EditorGUILayout.TextField(content, s),
                float f           => EditorGUILayout.FloatField(content, f),
                double d          => EditorGUILayout.DoubleField(content, d),
                bool b            => EditorGUILayout.Toggle(content, b),
                Color c           => EditorGUILayout.ColorField(content, c),
                Vector2 v2        => EditorGUILayout.Vector2Field(content, v2),
                Vector3 v3        => EditorGUILayout.Vector3Field(content, v3),
                Vector4 v4        => EditorGUILayout.Vector4Field(content, v4),
                Rect r            => EditorGUILayout.RectField(content, r),
                Bounds b          => EditorGUILayout.BoundsField(content, b),
                AnimationCurve ac => EditorGUILayout.CurveField(content, ac),
                Enum e            => EditorGUILayout.EnumPopup(content, e),
                LayerMask lm      => EditorGUILayout.LayerField(content, lm),
                Object o          => EditorGUILayout.ObjectField(content, o, obj.GetType(), true),
                _                 => throw new NotSupportedException($"Type {obj.GetType()} not supported")
            };

            if (!focusedFirstControl) {
                GUI.FocusControl(nameof(GenericDialogEditorWindow) + label);
                focusedFirstControl = true;
            }

            return field;
        }

    }

}