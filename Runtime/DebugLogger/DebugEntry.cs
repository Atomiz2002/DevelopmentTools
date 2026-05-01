#if DEVELOPMENT_TOOLS_RUNTIME_ODIN_INSPECTOR
using System;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using UnityEditor;
#if UNITY_EDITOR && !SIMULATE_BUILD && ENABLE_LOGS
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;
using Color = System.Drawing.Color;
#endif

namespace DevelopmentTools {

    [Serializable]
    public sealed class DebugEntry {

#if UNITY_EDITOR && !SIMULATE_BUILD && ENABLE_LOGS
        [SerializeField] [HideInInspector] public bool IsEvent;
        [SerializeField] [HideInInspector] public bool Received;
        [SerializeField] [HideInInspector] public bool IsError;

        public bool IsSeparator;

        [HideInInspector]
        public UnityEngine.Color color;

        public string   DisplayedCallerSignature;
        public string   DisplayedTime;
        public TimeSpan timestamp;

        public string DisplayedDetails;

        public List<Texture2D>                          Icons   = new();
        public List<(string details, string timestamp)> Details = new();

        #region More Details

        // public string Details2,  Details2Timestamp;
        // public string Details3,  Details3Timestamp;
        // public string Details4,  Details4Timestamp;
        // public string Details5,  Details5Timestamp;
        // public string Details6,  Details6Timestamp;
        // public string Details7,  Details7Timestamp;
        // public string Details8,  Details8Timestamp;
        // public string Details9,  Details9Timestamp;
        // public string Details10, Details10Timestamp;
        // public string Details11, Details11Timestamp;
        // public string Details12, Details12Timestamp;
        // public string Details13, Details13Timestamp;
        // public string Details14, Details14Timestamp;
        // public string Details15, Details15Timestamp;
        // public string Details16, Details16Timestamp;
        // public string Details17, Details17Timestamp;
        // public string Details18, Details18Timestamp;
        // public string Details19, Details19Timestamp;
        // public string Details20, Details20Timestamp;
        // public string Details21, Details21Timestamp;
        // public string Details22, Details22Timestamp;
        // public string Details23, Details23Timestamp;
        // public string Details24, Details24Timestamp;
        // public string Details25, Details25Timestamp;
        // public string Details26, Details26Timestamp;
        // public string Details27, Details27Timestamp;
        // public string Details28, Details28Timestamp;
        // public string Details29, Details29Timestamp;
        // public string Details30, Details30Timestamp;

        #endregion

        public string DisplayedStackTrace;

        public StackFrame[] stackFrames;

        [SerializeField] [HideInInspector] public bool   showStackTrace;
        [SerializeField] [HideInInspector] public string filePath;
        [SerializeField] [HideInInspector] public int    lineNumber;
        [SerializeField] [HideInInspector] public int    columnNumber;
        [SerializeField] [HideInInspector] public string returnValue;

        private Func<string, string> returnValueFormatting;

        public Guid guid;

        public static DebugEntry Separator() => new() { IsSeparator = true };

        /// <param name="_">prevents unity from calling this during serialization (it calls parameterless ctor)</param>
        private DebugEntry(byte _ = 0) => timestamp = TimeSpan.FromSeconds(Time.realtimeSinceStartup);

        public DebugEntry(Guid guid, bool isQuantum, UnityEngine.Color color, [CanBeNull] StackTrace stackTrace, [CanBeNull] object[] parametersValues, bool isEvent, bool received, bool isError, [CanBeNull] object[] details) : this() {
#if UNITY_EDITOR
            try { // TODO parametersValues as details if method takes no parameters
                this.guid   = guid;
                this.color  = color;
                stackFrames = (stackTrace ??= new(3, true)).GetFrames()!;

                StackFrame frame = stackFrames[0];

                filePath     = frame.GetFileName();
                lineNumber   = frame.GetFileLineNumber();
                columnNumber = frame.GetFileColumnNumber();

                DisplayedCallerSignature = GetSignature(frame, parametersValues,
                    className => className.Colored(Color.Gray),
                    methodName => methodName.Colored(Color.White).Size(13),
                    suffix => suffix.Colored(Color.DimGray).Size(10),
                    paramName => paramName.Colored(Color.DimGray),
                    paramValue => paramValue.Colored(Color.DarkGray),
                    paramMissingValue => paramMissingValue.Colored(Color.Orange),
                    line => line.Colored(Color.DimGray),
                    returnValue => returnValue.Colored(Color.White));

                if (isQuantum)
                    DisplayedCallerSignature = "[Q] ".Colored(Color.Cyan) + DisplayedCallerSignature;

                DisplayedStackTrace = stackFrames
                    .Select(f => f.GetFileName()?
                                     .StartAt("Assets", false, "...")
                                     .StartAt("quantum.code", true, "...")
                                     .Replace('\\', '/')
                                     .Regex(@"([^/]*)(\.cs)$", "$1".Bold().Size(13).Colored(color.AlterEditorOnlyForNow()))
                                 + (":" + f.GetFileLineNumber()).Colored(Color.Gray))
                    .Where(path => path.StartsWith("...")
                                   && !path.Contains("Core\\Core.cs"))
                    .JoinSmart("\n")
                    .Colored(color.AlterEditorOnlyForNow(.2f));

                IsEvent  = isEvent;
                Received = received;
                IsError  = isError;

                DisplayedTime = $"{(int) timestamp.TotalMinutes:D2}m {timestamp.Seconds:D2}s {timestamp.Milliseconds:D3}".Size(10).Colored(Color.White);

                DisplayedDetails = details?.JoinSmart("\n", string.Empty).Colored(Color.White);

                InfoContainer.ExtractAndModifyInfo(typeof(DebugEntry), ref DisplayedCallerSignature, ref Icons);
                InfoContainer.ExtractAndModifyInfo(typeof(DebugEntry), ref DisplayedDetails, ref Icons);

#if UNITY_EDITOR // ENABLE_LOGS is handled in DebugLogger.LogEntry
                if (isError)
                    if (DisplayedDetails.IsNullOrWhiteSpace())
                        $"{DisplayedCallerSignature}\n{stackTrace}".LogErr();
                    else
                        $"{DisplayedDetails}\n{DisplayedCallerSignature}\n{stackTrace}".LogErr();
#endif
            }
            catch (Exception e) {
#if ENABLE_LOGS
                $"Failed to create new debug entry:\n{e}".LogException();
#endif
                throw;
            }
#endif
        }

        public void AddIcons(params Texture2D[] textures) => Icons.AddRange(textures);

        public void AddDetails([NotNull] string details) {
            TimeSpan delay          = TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble) - timestamp;
            string   displayedDelay = $"+{delay:ss}s {delay.Milliseconds:000}";

            InfoContainer.ExtractAndModifyInfo(typeof(DebugEntry), ref details, ref Icons);

            for (int i = 0; i < Details.Count; i++)
                if (Details[i].details.IsNullOrEmpty()) {
                    Details[i] = (details, displayedDelay);
                    break;
                }
        }

        /// Will apply colors only if <paramref name="parametersValues"/> is not null
        private string GetSignature(StackFrame frame, [CanBeNull] object[] parametersValues,
            Func<string, string> classNameFormatting,
            Func<string, string> methodNameFormatting,
            Func<string, string> suffixFormatting,
            Func<string, string> paramLabelFormatting,
            Func<string, string> paramValueFormatting,
            Func<string, string> paramMissingValueFormatting,
            Func<string, string> lineNumberFormatting,
            Func<string, string> returnValueFormatting) {
            classNameFormatting         ??= x => x;
            methodNameFormatting        ??= x => x;
            suffixFormatting            ??= x => x;
            paramLabelFormatting        ??= x => x;
            paramValueFormatting        ??= x => x;
            paramMissingValueFormatting ??= x => x;
            lineNumberFormatting        ??= x => x;
            this.returnValueFormatting  =   returnValueFormatting ?? (x => x);

            (string className, string methodName, string suffix) = DebugLogger.GetHumanReadableCaller(frame);
            MethodBase method     = frame.GetMethod();
            MethodInfo methodInfo = method as MethodInfo;
            Type       classType  = method.DeclaringType!;

            if (method.TryGet(out ColoredLogsAttribute methodAttribute))
                methodNameFormatting += name => name.Colored(methodAttribute.Color);

            if (classType.TryGet(out ColoredLogsAttribute classAttribute))
                classNameFormatting += name => name.Colored(classAttribute.Color);

            string callerClassName  = classNameFormatting($"{CleanUp(className)}.");
            string callerMethodName = methodNameFormatting($"{CleanUp(methodName)}()");
            string callerSuffix     = suffix.IsNullOrWhiteSpace() ? string.Empty : suffixFormatting($"[{suffix}]");
            string callerLineNumber = lineNumberFormatting($":{frame.GetFileLineNumber()}");

            string returnTypeName  = methodInfo?.ReturnType.Name;
            string returnTypeValue = returnTypeName == "Void" ? string.Empty : $"\n{paramLabelFormatting("returned:")} {paramValueFormatting(CleanUp(returnTypeName))}";

            if (parametersValues == null)
                return $"{callerClassName}{callerMethodName}{callerSuffix}{callerLineNumber}{returnTypeValue}";

            ParameterInfo[] parametersInfo       = method.GetParameters();
            string          parametersWithValues = string.Empty;

            if (parametersInfo.Length > 0)
                for (int i = 0; i < parametersInfo.Length; i++) {
                    string paramName  = parametersInfo[i].Name;
                    string paramValue = i < parametersValues.Length ? parametersValues[i].SafeString("null") : paramMissingValueFormatting("???");
                    parametersWithValues += $"\n{paramLabelFormatting($"{paramLabelFormatting(paramName)}: {paramValueFormatting(CleanUp(paramValue))}")}";
                }
            else if (parametersValues.Length > 0 && parametersValues[0] != null)
                foreach (object passedParams in parametersValues)
                    parametersWithValues += $"\n{paramLabelFormatting($"{paramLabelFormatting("???")}: {paramValueFormatting(CleanUp(passedParams.SafeString("null")))}")}";

            return $"{callerClassName}{callerMethodName}{callerSuffix}{callerLineNumber}{parametersWithValues}{returnTypeValue}";
        }

        internal void SetReturn(object returnValue) {
            this.returnValue = CleanUp(returnValue);
            InfoContainer.ExtractAndModifyInfo(typeof(DebugEntry), ref this.returnValue, ref Icons);

            DisplayedCallerSignature += $" {returnValueFormatting(this.returnValue)}";
        }

        private string CleanUp(object type) {
            return Regex.Replace(type.SafeString("unknown"), @"\w+\.|`\d", string.Empty)
                .Replace("Void", "void")
                .Replace("Boolean", "bool");
        }

        public void OpenCaller() {
            StackFrame frame  = stackFrames[0];
            string     path   = filePath;
            int        line   = lineNumber;
            int        column = columnNumber;

            if (frame != null) {
                path   = frame.GetFileName();
                line   = frame.GetFileLineNumber();
                column = frame.GetFileColumnNumber();
            }

            AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(path!.StartAt("Assets")), line, column);
        }

        public override string ToString() =>
            Details
                .Select(d => d.details)
                .Prepend($"{(IsError ? "[ERROR] " : string.Empty)}{(IsEvent ? Received ? "[RECEIVED] " : "[SENT] " : string.Empty)}DebugEntry: {DisplayedCallerSignature} {DisplayedDetails}")
                .Where(s => !s.IsNullOrEmpty())
                .Select(s => s.Unformatted())
                .JoinSmart("\n");

#endif

    }

}
#endif