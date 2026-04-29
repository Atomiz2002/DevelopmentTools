using System;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.ContextMenus {

    public abstract class IntEditorWindow : EditorWindow {

        private int @int;

        protected abstract int         DefaultInt { get; }
        protected abstract string      Prompt     { get; }
        protected abstract string      FieldName  { get; }
        protected abstract string      Button     { get; }
        protected abstract Action<int> Action     { get; }

        private void OnEnable() {
            @int = DefaultInt;
        }

        private void OnGUI() {
            EditorGUILayout.LabelField(Prompt, EditorStyles.boldLabel);
            @int = EditorGUILayout.IntField(FieldName, @int);

            if (!GUILayout.Button(Button)) return;

            Action(@int);
            Close();
        }

    }

}