using System;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
#if UNITY_EDITOR && !SIMULATE_BUILD && ENABLE_LOGS
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Color = System.Drawing.Color;
#endif

[Serializable]
public sealed class DebugEntry {

#if UNITY_EDITOR && !SIMULATE_BUILD && ENABLE_LOGS

    [SerializeField] [HideInInspector] public bool IsEvent;
    [SerializeField] [HideInInspector] public bool Received;
    [SerializeField] [HideInInspector] public bool IsError;

    [ReadOnly] [DisplayAsString] [ShowIf(nameof(IsSeparator))] [GUIColor("black")] public bool IsSeparator;

    [HideInInspector]
    public UnityEngine.Color color;

    public string   DisplayedCallerSignature;
    public string   DisplayedTime;
    public TimeSpan timestamp;

    public Texture2D Icon1;
    public Texture2D Icon2;
    public Texture2D Icon3;
    public Texture2D Icon4;
    public Texture2D Icon5;

    public string DisplayedDetails;

    #region More Details

    public string Details2,  Details2Timestamp;
    public string Details3,  Details3Timestamp;
    public string Details4,  Details4Timestamp;
    public string Details5,  Details5Timestamp;
    public string Details6,  Details6Timestamp;
    public string Details7,  Details7Timestamp;
    public string Details8,  Details8Timestamp;
    public string Details9,  Details9Timestamp;
    public string Details10, Details10Timestamp;
    public string Details11, Details11Timestamp;
    public string Details12, Details12Timestamp;
    public string Details13, Details13Timestamp;
    public string Details14, Details14Timestamp;
    public string Details15, Details15Timestamp;
    public string Details16, Details16Timestamp;
    public string Details17, Details17Timestamp;
    public string Details18, Details18Timestamp;
    public string Details19, Details19Timestamp;
    public string Details20, Details20Timestamp;
    public string Details21, Details21Timestamp;
    public string Details22, Details22Timestamp;
    public string Details23, Details23Timestamp;
    public string Details24, Details24Timestamp;
    public string Details25, Details25Timestamp;
    public string Details26, Details26Timestamp;
    public string Details27, Details27Timestamp;
    public string Details28, Details28Timestamp;
    public string Details29, Details29Timestamp;
    public string Details30, Details30Timestamp;

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

    private DebugEntry() => timestamp = TimeSpan.FromSeconds(Time.realtimeSinceStartup);

    public DebugEntry(Guid guid, bool isQuantum, UnityEngine.Color color, [CanBeNull] StackTrace stackTrace, [CanBeNull] object[] parametersValues, bool isEvent, bool received, bool isError, [CanBeNull] object[] details) : this() {
#if UNITY_EDITOR
        try { // TODO parametersValues as details if method takes no parameters
            this.guid = guid;

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

            timestamp     = TimeSpan.FromSeconds(Time.realtimeSinceStartup);
            DisplayedTime = $"{(int) timestamp.TotalMinutes:D2}m {timestamp.Seconds:D2}s {timestamp.Milliseconds:D3}".Size(10).Colored(Color.White);

            DisplayedDetails = details?.JoinSmart("\n", string.Empty).Colored(Color.White);

#if UNITY_EDITOR
            if (isError)
                if (DisplayedDetails.IsNullOrWhiteSpace())
                    $"{DisplayedCallerSignature}\n{stackTrace}".LogErr();
                else
                    $"{DisplayedDetails}\n{DisplayedCallerSignature}\n{stackTrace}".LogErr();
#endif
        }
        catch (Exception e) {
#if ENABLE_LOGS
            $"Failed to create new debug entry:\n{e}".LogEx();
#endif
            throw;
        }
#endif
    }

    public override string ToString() =>
        new List<string> {
                $"{(IsError ? "[ERROR] " : string.Empty)}{(IsEvent ? Received ? "[RECEIVED] " : "[SENT] " : string.Empty)}DebugEntry: {DisplayedCallerSignature} {DisplayedDetails}",
                Details2,
                Details3,
                Details4,
                Details5,
                Details6,
                Details7,
                Details8,
                Details9,
                Details10,
                Details11,
                Details12,
                Details13,
                Details14,
                Details15,
                Details16,
                Details17,
                Details18,
                Details19,
                Details20,
                Details21,
                Details22,
                Details23,
                Details24,
                Details25,
                Details26,
                Details27,
                Details28,
                Details29,
                Details30
            }
            .Where(s => !s.IsNullOrEmpty())
            .Select(s => s.Unformatted())
            .JoinSmart("\n");

    public void AddDetails([NotNull] string details) {
        // @formatter:off
        if (DisplayedDetails.IsNullOrEmpty()) DisplayedDetails = details;
        else {
            TimeSpan delay          = TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble) - timestamp;
            string   displayedDelay = $"+{delay:ss}s {delay.Milliseconds:000}";

            if (Details2.IsNullOrEmpty())       { Details2 = details;  Details2Timestamp  = displayedDelay; }
            else if (Details3.IsNullOrEmpty())  { Details3 = details;  Details3Timestamp  = displayedDelay; }
            else if (Details4.IsNullOrEmpty())  { Details4 = details;  Details4Timestamp  = displayedDelay; }
            else if (Details5.IsNullOrEmpty())  { Details5 = details;  Details5Timestamp  = displayedDelay; }
            else if (Details6.IsNullOrEmpty())  { Details6 = details;  Details6Timestamp  = displayedDelay; }
            else if (Details7.IsNullOrEmpty())  { Details7 = details;  Details7Timestamp  = displayedDelay; }
            else if (Details8.IsNullOrEmpty())  { Details8 = details;  Details8Timestamp  = displayedDelay; }
            else if (Details9.IsNullOrEmpty())  { Details9 = details;  Details9Timestamp  = displayedDelay; }
            else if (Details10.IsNullOrEmpty()) { Details10 = details; Details10Timestamp = displayedDelay; }
            else if (Details11.IsNullOrEmpty()) { Details11 = details; Details11Timestamp = displayedDelay; }
            else if (Details12.IsNullOrEmpty()) { Details12 = details; Details12Timestamp = displayedDelay; }
            else if (Details13.IsNullOrEmpty()) { Details13 = details; Details13Timestamp = displayedDelay; }
            else if (Details14.IsNullOrEmpty()) { Details14 = details; Details14Timestamp = displayedDelay; }
            else if (Details15.IsNullOrEmpty()) { Details15 = details; Details15Timestamp = displayedDelay; }
            else if (Details16.IsNullOrEmpty()) { Details16 = details; Details16Timestamp = displayedDelay; }
            else if (Details17.IsNullOrEmpty()) { Details17 = details; Details17Timestamp = displayedDelay; }
            else if (Details18.IsNullOrEmpty()) { Details18 = details; Details18Timestamp = displayedDelay; }
            else if (Details19.IsNullOrEmpty()) { Details19 = details; Details19Timestamp = displayedDelay; }
            else if (Details20.IsNullOrEmpty()) { Details20 = details; Details20Timestamp = displayedDelay; }
            else if (Details21.IsNullOrEmpty()) { Details21 = details; Details21Timestamp = displayedDelay; }
            else if (Details22.IsNullOrEmpty()) { Details22 = details; Details22Timestamp = displayedDelay; }
            else if (Details23.IsNullOrEmpty()) { Details23 = details; Details23Timestamp = displayedDelay; }
            else if (Details24.IsNullOrEmpty()) { Details24 = details; Details24Timestamp = displayedDelay; }
            else if (Details25.IsNullOrEmpty()) { Details25 = details; Details25Timestamp = displayedDelay; }
            else if (Details26.IsNullOrEmpty()) { Details26 = details; Details26Timestamp = displayedDelay; }
            else if (Details27.IsNullOrEmpty()) { Details27 = details; Details27Timestamp = displayedDelay; }
            else if (Details28.IsNullOrEmpty()) { Details28 = details; Details28Timestamp = displayedDelay; }
            else if (Details29.IsNullOrEmpty()) { Details29 = details; Details29Timestamp = displayedDelay; }
            else if (Details30.IsNullOrEmpty()) { Details30 = details; Details30Timestamp = displayedDelay; }
            else                                  Details30 += $"⤵\n{details}";
        }
        // @formatter:on
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

        DisplayedCallerSignature += $" {returnValueFormatting(this.returnValue)}";
    }

    private string CleanUp(object type) {
        return Regex.Replace(type.SafeString("unknown"), @"\w+\.|`\d", string.Empty)
            .Replace("Void", "void")
            .Replace("Boolean", "bool");
    }

    public void OpenCaller() => stackFrames[0].OpenAsset(filePath, lineNumber, columnNumber);

#endif

}