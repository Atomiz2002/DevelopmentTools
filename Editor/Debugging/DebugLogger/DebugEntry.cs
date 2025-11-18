using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using DevelopmentTools.Editor.Attributes.PreviewTexture2D;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Color = System.Drawing.Color;

namespace DevelopmentTools.Editor.Debugging.DebugLogger {

    [Serializable]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public sealed class DebugEntry {

#if UNITY_EDITOR

        private const string QuantumPrefix = "<color=cyan><b>[Q]</b></color>";

        [field: SerializeField, HideInInspector] public bool IsEvent     { get; private set; }
        [field: SerializeField, HideInInspector] public bool Received    { get; private set; }
        [field: SerializeField, HideInInspector] public bool IsError     { get; private set; }
        [field: SerializeField, HideInInspector] public int  Repetitions { get; private set; }

        [ReadOnly]
        [ShowIf(nameof(IsSeparator))]
        [GUIColor("black")]
        public bool IsSeparator;

        #region Info

        [HideLabel]
        [ColoredBoxGroup("Box", nameof(EntryColor), ShowLabel = false)]
        [HorizontalGroup("Box/h_Info,Details,Buttons&Icons")]
        [VerticalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons")]
        [HorizontalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons")]
        [DisplayAsString(EnableRichText = true, Overflow = false)]
        public string DisplayedCallerSignature;

        [HideLabel]
        [ShowIf("@Repetitions > 1")]
        [HorizontalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons", Width = 25, VisibleIf = "@!" + nameof(IsSeparator))]
        [DisplayAsString(EnableRichText = true, Overflow = false, Alignment = TextAlignment.Right)]
        public string DisplayedRepetitions;

        [HideLabel]
        [HorizontalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons", Width = 20, VisibleIf = "@!" + nameof(IsSeparator))]
        [DisplayAsString(EnableRichText = true, Overflow = false, Alignment = TextAlignment.Right)]
        public string DisplayedTime;

        private UnityEngine.Color EntryColor() =>
            IsSeparator
                ? UnityEngine.Color.black
                : IsError
                    ? UnityEngine.Color.red
                    : GUI.color;

        #endregion

        #region Buttons

        [ShowIf("@!" + nameof(IsSeparator))]
        [HorizontalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons", Width = 22)]
        [VerticalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons/v_FirstColumn")]
        [Button(SdfIconType.EyeFill, ButtonHeight = 22)]
        private void OpenCaller() {
            if (filePath.Contains("Assets")) {
                AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(filePath.StartAt("Assets")), lineNumber, columnNumber);
            }
            else {
                string quantumProjectPath = Path.Join(new DirectoryInfo(Application.dataPath).Parent!.Parent!.SafeString(), filePath.StartAt("quantum_code"));
                Process.Start(EditorPrefs.GetString("kScriptsDefaultApp"), $"--line {lineNumber} --column {columnNumber - 1} \"{quantumProjectPath}\"");
            }
        }

        [GUIColor("purple")]
        [ShowIf("@!" + nameof(IsSeparator))]
        [VerticalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons/v_FirstColumn")]
        [Button(SdfIconType.ChevronBarExpand, ButtonHeight = 22)]
        private void ToggleDisplayStackTrace() => showStackTrace = !showStackTrace;

        [ShowIf("@!" + nameof(IsSeparator) + " && !" + nameof(IsEvent))]
        [HorizontalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons", Width = 22)]
        [VerticalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons/v_SecondColumn")]
        [Button(SdfIconType.CaretRightFill, ButtonHeight = 22)]
        private void ButtonNotEvent() => LogMessageTypeAndAmount();

        [GUIColor("blue")]
        [ShowIf("@!" + nameof(IsSeparator) + " && " + nameof(IsEvent) + " && !" + nameof(Received))]
        [HorizontalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons", Width = 22)]
        [VerticalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons/v_SecondColumn")]
        [Button(SdfIconType.CaretUpFill, ButtonHeight = 22)]
        private void ButtonSentEvent() => LogMessageTypeAndAmount();

        [GUIColor("green")]
        [ShowIf("@!" + nameof(IsSeparator) + " && " + nameof(IsEvent) + " && " + nameof(Received))]
        [HorizontalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons", Width = 22)]
        [VerticalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons/v_SecondColumn")]
        [Button(SdfIconType.CaretDownFill, ButtonHeight = 22)]
        private void ButtonReceivedEvent() => LogMessageTypeAndAmount();

        private void LogMessageTypeAndAmount() {
            if (DisplayedCallerSignature.IsNullOrWhiteSpace()) return;

            string type = !IsEvent
                ? "▶ Logged"
                : Received
                    ? "▼ Received"
                    : "▲ Sent";

            type = DisplayedCallerSignature.StartsWith(QuantumPrefix) ? $"{QuantumPrefix.Colored(Color.Cyan)} {type}" : type;

            Debug.Log($"{type} {Repetitions} times in a row".Colored(!IsEvent
                ? Color.White
                : Received
                    ? Color.CornflowerBlue
                    : Color.OrangeRed));
        }

        [GUIColor("purple")]
        [ShowIf("@!" + nameof(IsSeparator))]
        [VerticalGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/h_Info&Buttons/v_SecondColumn")]
        [Button(SdfIconType.ChatSquareDotsFill, ButtonHeight = 22)]
        private void LogStackTrace() =>
            Debug.Log(stackTrace?.GetFrames()?
                          .Select(f => {
                              string filePath   = f.GetFileName() ?? "Unknown path";
                              int    lineNumber = f.GetFileLineNumber();
                              return $"{filePath.StartAt("Assets", true, "...")}:{lineNumber}".Link(filePath.StartAt("Assets"), lineNumber);
                              // why is the last line always ...Assets and the rest aren't
                              // make links only the shown ones in the dropdown
                              // wtf are these comments ^
                          })
                          .Join("\n")
                      ?? DisplayedStackTrace
                      + "\n\n\n");

        #endregion

        #region Icons

        [ShowIf("@!" + nameof(IsSeparator))]
        [HorizontalGroup("Box/h_Info,Details,Buttons&Icons", Width = 64, Order = 1)]
        [VerticalGroup("Box/h_Info,Details,Buttons&Icons/v_Icons",
            PaddingTop = 0,
            VisibleIf = "@" + nameof(Icon1) + " || " + nameof(Icon2) + " || " + nameof(Icon3),
            Order = 1)]
        [PreviewTexture("icon1Color", 2, "black")]
        public Texture2D Icon1;

        [ShowIf("@!" + nameof(IsSeparator))]
        [VerticalGroup("Box/h_Info,Details,Buttons&Icons/v_Icons", PaddingTop = 2)]
        [PreviewTexture("icon2Color", 2, "black")]
        public Texture2D Icon2;

        [ShowIf("@!" + nameof(IsSeparator))]
        [VerticalGroup("Box/h_Info,Details,Buttons&Icons/v_Icons")]
        [PreviewTexture("icon3Color", 2, "black")]
        public Texture2D Icon3;

        #endregion

        #region Details

        [HideLabel]
        [DisplayAsString(EnableRichText = true, Overflow = false)]
        [BoxGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/Details1", VisibleIf = "@" + nameof(DisplayedDetails) + "?.Length > 0", ShowLabel = false)]
        public string DisplayedDetails;

        #region More Details

        [HideLabel, DisplayAsString(EnableRichText = true, Overflow = false)]
        [BoxGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/Details2", VisibleIf = "@Details2?.Length > 0", ShowLabel = false)]
        public string Details2;
        private int details2Repetitions = 1;

        [HideLabel, DisplayAsString(EnableRichText = true, Overflow = false)]
        [BoxGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/Details3", VisibleIf = "@Details3?.Length > 0", ShowLabel = false)]
        public string Details3;
        private int details3Repetitions = 1;

        [HideLabel, DisplayAsString(EnableRichText = true, Overflow = false)]
        [BoxGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/Details4", VisibleIf = "@Details4?.Length > 0", ShowLabel = false)]
        public string Details4;
        private int details4Repetitions = 1;

        [HideLabel, DisplayAsString(EnableRichText = true, Overflow = false)]
        [BoxGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/Details5", VisibleIf = "@Details5?.Length > 0", ShowLabel = false)]
        public string Details5;
        private int details5Repetitions = 1;

        [HideLabel, DisplayAsString(EnableRichText = true, Overflow = false)]
        [BoxGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/Details6", VisibleIf = "@Details6?.Length > 0", ShowLabel = false)]
        public string Details6;
        private int details6Repetitions = 1;

        [HideLabel, DisplayAsString(EnableRichText = true, Overflow = false)]
        [BoxGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/Details7", VisibleIf = "@Details7?.Length > 0", ShowLabel = false)]
        public string Details7;
        private int details7Repetitions = 1;

        [HideLabel, DisplayAsString(EnableRichText = true, Overflow = false)]
        [BoxGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/Details8", VisibleIf = "@Details8?.Length > 0", ShowLabel = false)]
        public string Details8;
        private int details8Repetitions = 1;

        [HideLabel, DisplayAsString(EnableRichText = true, Overflow = false)]
        [BoxGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/Details9", VisibleIf = "@Details9?.Length > 0", ShowLabel = false)]
        public string Details9;
        private int details9Repetitions = 1;

        #endregion

        [HideLabel]
        [DisplayAsString(EnableRichText = true, Overflow = false)]
        [ColoredBoxGroup("Box/h_Info,Details,Buttons&Icons/v_Info&Details&Buttons/StackTrace", .8f, VisibleIf = nameof(showStackTrace), ShowLabel = false)]
        public string DisplayedStackTrace;

        #endregion

        public StackTrace stackTrace;

        [SerializeField] [HideInInspector] private bool   showStackTrace;
        [SerializeField] [HideInInspector] private string filePath;
        [SerializeField] [HideInInspector] private int    lineNumber;
        [SerializeField] [HideInInspector] private int    columnNumber;
        [SerializeField] [HideInInspector] public  string returnValue;

        private Func<string, string> returnValueFormatting;

        public DebugEntry() => IsSeparator = true;

        public DebugEntry(bool isQuantum, [CanBeNull] StackTrace stackTrace, [CanBeNull] object[] parametersValues, bool isEvent, bool received, bool isError,
            [CanBeNull] object[] details) {
#if UNITY_EDITOR
            try {
                this.stackTrace = stackTrace ??= new(3, true);

                StackFrame frame = stackTrace.GetFrame(0);

                filePath     = frame.GetFileName();
                lineNumber   = frame.GetFileLineNumber();
                columnNumber = frame.GetFileColumnNumber();

                DisplayedCallerSignature = GetSignature(frame,
                    parametersValues,
                    className => className,
                    methodName => methodName.Colored(Color.White),
                    paramName => paramName.Colored(Color.DimGray),
                    paramValue => paramValue.Colored(Color.DarkGray),
                    paramNullValue => paramNullValue.Colored(Color.Orange),
                    line => line.Colored(Color.DimGray),
                    returnValue => returnValue.Colored(Color.White));

                if (isQuantum)
                    DisplayedCallerSignature = $"{QuantumPrefix} {DisplayedCallerSignature}";

                DisplayedStackTrace = stackTrace.GetFrames()!
                    .Select(f => f.GetFileName()?
                                     .StartAt("Assets", true, "...")
                                     .StartAt("quantum.code", true, "...")
                                 + (":" + f.GetFileLineNumber()).Colored(Color.Gray))
                    .Replace(path => !path.StartsWith("...Assets")
                                     && !path.StartsWith("...quantum.code")
                                     || path.Contains("Core\\Core.cs"),
                        "...")
                    .MergeDuplicates()
                    .Join("\n")
                    .Colored(Color.Violet);

                IsEvent     = isEvent;
                Received    = received;
                IsError     = isError;
                Repetitions = 1;

                DateTime time = DateTime.Now;
                DisplayedTime = $"{time:mm}m\n{time:ss}s\n{time:fff}".Size(10);

                DisplayedDetails = details?.Join("\n").Colored(Color.White);

                // UpdateIcons();
            }
            catch (Exception e) {
                $"Failed to create new debug entry:\n{e}".LogEx();
                throw;
            }
#endif
        }

        public override string ToString() =>
            new List<string> {
                    $"{DisplayedCallerSignature} {DisplayedDetails}",
                    Details2,
                    Details3,
                    Details4,
                    Details5,
                    Details6,
                    Details7,
                    Details8,
                    Details9,
                    $"IsEvent: {IsEvent}",
                    $"Received: {Received}",
                    $"IsError: {IsError}",
                    $"Repetitions: {Repetitions}"
                }
                .Where(s => !s.IsNullOrEmpty())
                .Select(s => s.Unformatted())
                .Join("\n");

        public void IncreaseRepetitions() {
            if (Repetitions > 100)
                return;

            Repetitions++;
            DisplayedRepetitions = $"|{FormatRepetitions(Repetitions)}|".Colored(Color.Red).Bold();
        }

        // private void UpdateIcons() {
        //     EntityRef[] entityRefs = QuantumEditorDebugging.ExtractEntityRefs(ToString()).ToArray();
        //
        //     for (int i = 0, j = 0; i < entityRefs.Length && j < 3; i++) {
        //         switch (j) {
        //             case 0:
        //                 Icon1 = QuantumEditorDebugging.GetOrAddColoredIcon(entityRefs[i]).icon;
        //                 j++;
        //                 break;
        //             case 1:
        //                 Icon2 = QuantumEditorDebugging.GetOrAddColoredIcon(entityRefs[i]).icon;
        //                 j++;
        //                 break;
        //             case 2:
        //                 Icon3 = QuantumEditorDebugging.GetOrAddColoredIcon(entityRefs[i]).icon;
        //                 j++;
        //                 break;
        //         }
        //     }
        // }

        private string FormatRepetitions(int repetitions) => repetitions < 100 ? $"{repetitions}" : "##";

        public void AddDetails([JetBrains.Annotations.NotNull] string details) {
            if (DisplayedDetails.IsNullOrEmpty() || DisplayedDetails.StartsWith(details))
                DisplayedDetails = DisplayedDetails.IsNullOrEmpty() ? details : $"{details} ({FormatRepetitions(Repetitions++)})";
            else if (Details2.IsNullOrEmpty() || Details2.StartsWith(details))
                Details2 = Details2.IsNullOrEmpty() ? details : $"{details} ({FormatRepetitions(details2Repetitions++)})";
            else if (Details3.IsNullOrEmpty() || Details3.StartsWith(details))
                Details3 = Details3.IsNullOrEmpty() ? details : $"{details} ({FormatRepetitions(details3Repetitions++)})";
            else if (Details4.IsNullOrEmpty() || Details4.StartsWith(details))
                Details4 = Details4.IsNullOrEmpty() ? details : $"{details} ({FormatRepetitions(details4Repetitions++)})";
            else if (Details5.IsNullOrEmpty() || Details5.StartsWith(details))
                Details5 = Details5.IsNullOrEmpty() ? details : $"{details} ({FormatRepetitions(details5Repetitions++)})";
            else if (Details6.IsNullOrEmpty() || Details6.StartsWith(details))
                Details6 = Details6.IsNullOrEmpty() ? details : $"{details} ({FormatRepetitions(details6Repetitions++)})";
            else if (Details7.IsNullOrEmpty() || Details7.StartsWith(details))
                Details7 = Details7.IsNullOrEmpty() ? details : $"{details} ({FormatRepetitions(details7Repetitions++)})";
            else if (Details8.IsNullOrEmpty() || Details8.StartsWith(details))
                Details8 = Details8.IsNullOrEmpty() ? details : $"{details} ({FormatRepetitions(details8Repetitions++)})";
            else if (Details9.IsNullOrEmpty() || Details9.StartsWith(details))
                Details9 = Details9.IsNullOrEmpty() ? details : $"{details} ({FormatRepetitions(details9Repetitions++)})";
            else
                Details9 += $"\n----------------------\n{details}";

            // UpdateIcons();
        }

        /// Will apply colors only if <paramref name="parametersValues"/> is not null
        private string GetSignature(StackFrame frame, object[] parametersValues,
            Func<string, string> classNameFormatting,
            Func<string, string> methodNameFormatting,
            Func<string, string> paramLabelFormatting,
            Func<string, string> paramValueFormatting,
            Func<string, string> paramNullValueFormatting,
            Func<string, string> lineNumberFormatting,
            Func<string, string> returnValueFormatting) {
            classNameFormatting        ??= name => name;
            methodNameFormatting       ??= name => name;
            paramLabelFormatting       ??= name => name;
            paramValueFormatting       ??= value => value;
            paramNullValueFormatting   ??= nullValue => nullValue;
            lineNumberFormatting       ??= line => line;
            this.returnValueFormatting =   returnValueFormatting;

            MethodBase method     = frame.GetMethod();
            MethodInfo methodInfo = method as MethodInfo;
            Type       classType  = method.DeclaringType!;

            if (method.TryGet(out ColoredLogsAttribute methodAttribute))
                methodNameFormatting += name => name.Colored(methodAttribute.Color);

            if (classType.TryGet(out ColoredLogsAttribute classAttribute))
                classNameFormatting += name => name.Colored(classAttribute.Color);

            string callerClassName  = classNameFormatting($"{method.DeclaringType!.Name}.");
            string callerMethodName = methodNameFormatting($"{method.Name.StartAtLast(".", false)}()");
            string callerLineNumber = lineNumberFormatting($":{frame.GetFileLineNumber()}");
            string callerReturnType = paramValueFormatting(methodInfo?.ReturnType.Name ?? "unknown");
            string returnTypeValue  = $"{paramLabelFormatting("returned:")} ({paramValueFormatting(callerReturnType)})";

            if (parametersValues == null)
                return $"{callerClassName}{callerMethodName}{callerLineNumber}\n{returnTypeValue}";

            ParameterInfo[] parametersInfo     = method.GetParameters();
            string[]        paramNameWithValue = new string[parametersInfo.Length];

            for (int i = 0; i < parametersInfo.Length; i++) {
                string paramName  = parametersInfo[i].Name;
                string paramValue = i < parametersValues.Length ? parametersValues[i].SafeString(paramNullValueFormatting("null")) : paramNullValueFormatting("missing");
                paramNameWithValue[i] = paramLabelFormatting($"{paramLabelFormatting(paramName)}: {paramValueFormatting(paramValue)}");
            }

            string parametersWithValues = paramNameWithValue.Join("\n");

            return $"{callerClassName}{callerMethodName}{callerLineNumber}\n{parametersWithValues}\n{returnTypeValue}";
        }

        internal string SetReturn(object returnValue) {
            this.returnValue = returnValue.SafeString();

            DisplayedCallerSignature += $" {returnValueFormatting(this.returnValue)}";
            // UpdateIcons();

            return DisplayedCallerSignature;
        }

#endif

    }

}