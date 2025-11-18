#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DevelopmentTools.Editor.Debugging.DebugFields {

    [Serializable]
    public class DebugFieldsValues {

        [HideInInspector] public string FieldName;

        [SerializeField]
        [LabelText("@FieldName")]
        [ListDrawerSettings(
            IsReadOnly = true,
            NumberOfItemsPerPage = 5,
            DefaultExpandedState = true)]
        private List<DebugFieldValue> DebugFields = new();

        public DebugFieldsValues(string fieldName) => FieldName = fieldName;

        public void AddValue(string value, Texture2D icon, StackTrace stackTrace) {
            if (DebugFields.Count > 0 && DebugFields[0].RawValue == value && DebugFields[0].Icon == icon)
                DebugFields[0].Repeat();
            else
                DebugFields.Insert(0, new(FieldName, value, icon, stackTrace));
        }

    }

}
#endif