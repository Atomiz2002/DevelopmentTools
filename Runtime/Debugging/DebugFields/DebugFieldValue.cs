#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Attributes.PreviewTexture2D;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Color = System.Drawing.Color;

namespace DevelopmentTools.Debugging.DebugFields {

    [Serializable]
    [InlineProperty]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public class DebugFieldValue {

        [HideInInspector] public string RawName;
        [HideInInspector] public string RawValue;

        [ShowIf("@Repetitions > 1")]
        [VerticalGroup("_/_")]
        [HideLabel]
        [DisplayAsString(EnableRichText = true)]
        [SerializeField]
        private string TimeRepeat;

        [HorizontalGroup("_", Width = 60)]
        [VerticalGroup("_/_")]
        [HideLabel]
        [DisplayAsString(EnableRichText = true)]
        [SerializeField]
        private string Time;

        [HorizontalGroup("_", Width = 32)]
        [HideLabel]
        [PreviewTexture]
        [SerializeField]
        public Texture2D Icon;

        [HorizontalGroup("_")]
        [HideLabel]
        [DisplayAsString(EnableRichText = true, FontSize = 14, Overflow = false)]
        [SerializeField]
        private string DisplayValue;

        [HorizontalGroup("_", Width = 15)]
        [HideLabel]
        [DisplayAsString(EnableRichText = true, FontSize = 14)]
        [SerializeField]
        private int Repetitions = 1;

        private string filePath;
        private int    lineNumber;
        private int    columnNumber;

        public DebugFieldValue(string name, string value, Texture2D icon = null, StackTrace stackTrace = null) {
            RawName  = name;
            RawValue = value;
            Icon     = icon;

            DisplayValue = value.Bold().Colored(Color.White);
            Time         = $"{DateTime.Now:mm:ss.fff}";

            StackFrame stackFrame = (stackTrace ?? new StackTrace(4, true)).GetFrame(0);
            filePath     = stackFrame?.GetFileName();
            lineNumber   = stackFrame?.GetFileLineNumber() ?? 0;
            columnNumber = stackFrame?.GetFileColumnNumber() ?? 0;
        }

        public void Repeat() {
            Repetitions++;
            TimeRepeat = $"{DateTime.Now:mm:ss.fff}";
        }

        [HorizontalGroup("_", Width = 22)]
        [Button(SdfIconType.EyeFill, ButtonHeight = 22)]
        private void OpenCaller() {
            if (filePath.Contains("Assets"))
                AssetDatabase.OpenAsset(filePath.StartAt("Assets").LoadAsset(), lineNumber, columnNumber);
            else {
                string quantumProjectPath = Path.Join(new DirectoryInfo(Application.dataPath).Parent!.Parent!.SafeString(), filePath.StartAt("quantum_code"));
                Process.Start(EditorPrefs.GetString("kScriptsDefaultApp"), $"--line {lineNumber} --column {columnNumber - 1} \"{quantumProjectPath}\"");
            }
        }

    }

}
#endif