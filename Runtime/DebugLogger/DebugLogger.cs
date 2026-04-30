#if DEVELOPMENT_TOOLS_RUNTIME_ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using DevelopmentTools.Settings;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Color = System.Drawing.Color;
#if UNITY_EDITOR
using UnityEditor;
#if ENABLE_LOGS
using System.Linq;
#if !SIMULATE_BUILD
using System.Threading;
using System.Reflection;
#endif

#endif
#endif

namespace DevelopmentTools {

    [SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
    [CreateAssetMenu(fileName = nameof(DebugLogger), menuName = "Atomiz/" + nameof(DebugLogger))]
#if DEVELOPMENT_TOOLS_RUNTIME_ODIN_INSPECTOR
    [HideMonoScript]
    public partial class DebugLogger : SerializedScriptableObject {

#else
    public class DebugLogger : ScriptableObject {
#endif

#if UNITY_EDITOR
#if !SIMULATE_BUILD && ENABLE_LOGS

        public static readonly List<Color> colors = new() {
            Color.YellowGreen,
            Color.OrangeRed,
            Color.Cyan,
            Color.Violet,
            Color.Coral,
            Color.Crimson,
            Color.Bisque,
            Color.Gold,
            Color.MediumSlateBlue,
            Color.DarkKhaki,
            Color.DeepSkyBlue,
            Color.DarkGray,
            Color.Aquamarine,
            Color.LightBlue,
            Color.CadetBlue
        };

        public List<string>     DisplayedPinnedInfo = new();
        public List<DebugEntry> DisplayedEntries    = new();

        public List<string> SelectedGroups = new();

        public Dictionary<string, string>           pinnedInfo    = new();
        public Dictionary<string, List<DebugEntry>> debugEntries  = new();
        public Dictionary<string, List<DebugEntry>> pausedEntries = new();

        [MenuItem(EngineSettings.MenuGroupPath + "Debug Logger &#e")]
        public static void TryShowWindow() => EngineSettings.TryShowWindow(I);

        [ContextMenu("Fill list with test messages")]
        private void TestSystem() {
            Initialize();
            LogEntry(false, null, null, SelectedGroups[0], false, false, false);
            LogEntry(true, null, null, SelectedGroups[0], true, true, true);
        }

        private void OnDisable() => EditorApplication.playModeStateChanged -= PrepareForState;

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize() {
            EditorApplication.playModeStateChanged -= I.PrepareForState;
            EditorApplication.playModeStateChanged += I.PrepareForState;
        }

        private void PrepareForState(PlayModeStateChange state) {
            try {
                switch (state) {
                    case PlayModeStateChange.EnteredPlayMode:
                        DisplayedPinnedInfo.Clear();
                        pinnedInfo.Clear();

                        DisplayedEntries.Clear();
                        debugEntries.Clear();
                        pausedEntries.Clear();

                        Initialize();
                        break;

                    case PlayModeStateChange.ExitingPlayMode:
                        EditorUtility.SetDirty(this);
                        AssetDatabase.SaveAssetIfDirty(this);
                        break;
                }
            }
            catch (Exception e) {
                $"Failed to initialize DebugLogger:\n{e}".LogException();
            }
        }

        public static AsyncLocal<Guid> key = new();

        public static readonly string DebugLoggerDir = Path.Combine(Application.dataPath, nameof(DebugLogger));
        public static readonly string GroupsDir      = Path.Combine(DebugLoggerDir, "Groups");

        private static DebugLogger i;
        private static DebugLogger I {
            get {
                i = AssetDatabase.LoadAssetAtPath<DebugLogger>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:{nameof(DebugLogger)}").FirstOrDefault()));
                if (i)
                    return i;

                Directory.CreateDirectory(DebugLoggerDir);
                AssetDatabase.CreateAsset(i = CreateInstance<DebugLogger>(), Path.Combine(DebugLoggerDir, nameof(DebugLogger) + ".asset").RelativePath());

                return i;
            }
        }
#else
        [MenuItem(EngineSettings.MenuGroupPath + "Debug Logger (ENABLE_LOGS is off) &#e")]
        public static void ContextMenuItem() {}

        [MenuItem(EngineSettings.MenuGroupPath + "Debug Logger (ENABLE_LOGS is off) &#e", true)]
        public static bool DisabledContextMenuItem() => false;
#endif
#endif

        #region Pin

        /// Use the <paramref name="key"/> to edit already pinned info
        [Conditional(EngineSettings.ENABLE_LOGS)]
        public static void Pin([JetBrains.Annotations.NotNull] string key, params object[] info) {
#if ENABLE_LOGS
#if UNITY_EDITOR && !SIMULATE_BUILD
            try {
                if (info != null)
                    I.pinnedInfo[key] = $"{key.Colored(Color.Red)} {info.JoinSmart().Flatten().SafeString()}".Bold();
                else
                    I.pinnedInfo.Remove(key);

                I.DisplayedPinnedInfo = I.pinnedInfo.Values.ToList();
            }
            catch (Exception e) {
                $"Failed to un/pin DebugLogger info:\n{e}".LogException();
            }
#else
            if (info != null)
                $"Pinned info: {key} {info.Join().Flatten().SafeString()}".Log();
#endif
#endif
        }

        [Conditional(EngineSettings.ENABLE_LOGS)]
        public void Unpin([JetBrains.Annotations.NotNull] string key) => Pin(key);

        #endregion

        protected static void LogEntry(bool isQuantum, [CanBeNull] StackTrace stackTrace, [CanBeNull] object[] parameters, string group, bool isEvent, bool isReceived, bool isError, params object[] details) {
#if ENABLE_LOGS
#if UNITY_EDITOR && !SIMULATE_BUILD
            try {
                key.Value = Guid.NewGuid();

                Color entryColor = I.debugEntries.TryGetValue(group, out List<DebugEntry> entry)
                    ? entry[0].color.ToCsColor()
                    : colors[I.debugEntries.Keys.Count % colors.Count];

                I.AddEntry(group, new(key.Value, isQuantum, entryColor.ToUnityColor(), stackTrace, parameters, isEvent, isReceived, isError, details));
            }
            catch (Exception e) {
                $"Failed to add DebugLog entry:\n{isQuantum}\n{stackTrace}\n{parameters}\n{isEvent}\n{isReceived}\n{isError}\n{details}\n{e}".LogException();
            }
#else
            (
                $"{(isError ? "[ERROR] " : string.Empty)}{(isQuantum ? "[Q] " : string.Empty)}{(isEvent ? isReceived ? "[RECEIVED] " : "[SENT] " : string.Empty)}DebugEntry: ({(parameters is { Length: > 0 } ? string.Join(", ", parameters.Select(p => p.SafeString("null"))) : "none")}){stackTrace ?? new StackTrace(2, true)}\n" + (details is { Length: > 0 } ? string.Join(", ", details.Select(d => d.SafeString("null"))) : "none")
            ).Log();
#endif
#endif
        }

        protected static void AddIcon([CanBeNull] string specificGroup, params Texture2D[] textures) {
#if ENABLE_LOGS
#if UNITY_EDITOR && !SIMULATE_BUILD
            try {
                if (textures == null || textures.Length == 0)
                    return;

                I.GetEntryByKeyGUID(specificGroup).AddIcons(textures);
            }
            catch (Exception e) {
                $"Failed to add textures to ({key.Value}) entry: {textures.n()?.Join(t => t.name)}.\n\n{e}".LogException();
            }
#endif
#endif
        }

        protected static void AddGoodDetails([CanBeNull] string specificGroup, object[] details) => AddLastEntryDetails(specificGroup, details.EnsureString("null").Colored(Color.Lime));
        protected static void AddBadDetails([CanBeNull] string specificGroup, object[] details)  => AddLastEntryDetails(specificGroup, details.EnsureString("null").Colored(Color.Red));
        protected static void AddDetails([CanBeNull] string specificGroup, object[] details)     => AddLastEntryDetails(specificGroup, details.EnsureString("null"));

        private static void AddLastEntryDetails([CanBeNull] string specificGroup, string details) {
#if ENABLE_LOGS
#if UNITY_EDITOR && !SIMULATE_BUILD
            try {
                if (details.IsNullOrWhiteSpace())
                    return;

                I.GetEntryByKeyGUID(specificGroup).AddDetails(details);
            }
            catch (Exception e) {
                $"Failed to add more details to ({key.Value}) entry:\n{details}\n\n{e}".LogException();
            }
#else
            $"[+details+] {details.SafeString("broken details")} -> {new StackTrace(2, true)}".Log();
#endif
#endif
        }

        protected static void SetReturn([CanBeNull] string specificGroup, object returnValue) {
#if ENABLE_LOGS
#if UNITY_EDITOR && !SIMULATE_BUILD
            try {
                I.GetEntryByKeyGUID(specificGroup).SetReturn(returnValue);
            }
            catch (Exception e) {
                $"Failed to set LogReturn to ({key.Value}) entry:\n{e}".LogException();
            }
#else
            $"[-return-] {returnValue} -> {new StackTrace(2, true)}".Log();
#endif
#endif
        }

        private static HashSet<string> symbols;

        public static HashSet<string> GetSymbols() {
            if (symbols != null)
                return symbols;

            symbols = new();

            foreach (Type type in typeof(DebugLogger).GetNestedTypes())
                symbols.Add(nameof(DebugLogger) + "_" + type.Name);

            return symbols;
        }

#if UNITY_EDITOR && !SIMULATE_BUILD && ENABLE_LOGS
        private DebugEntry GetEntryByKeyGUID([CanBeNull] string specificGroup) =>
            specificGroup.IsNullOrEmpty()
                ? debugEntries.Keys.Select(g => debugEntries[g].FirstOrDefault(e => e.guid == key.Value)).FirstOrDefault(e => e != null)
                : debugEntries[specificGroup].FirstOrDefault(e => e.guid == key.Value);

        public void AddEntry(string group, DebugEntry newEntry) {
            if (!debugEntries.ContainsKey(group))
                debugEntries[group] = new();

            if (pausedEntries.TryGetValue(group, out List<DebugEntry> paused)) {
                paused.Add(newEntry);
                return;
            }

            debugEntries[group].Add(newEntry);

            UpdateDisplayedEntries();
        }

        public void UpdateDisplayedEntries() {
            DisplayedEntries.Clear();

            foreach ((string group, List<DebugEntry> entries) in debugEntries)
                if (SelectedGroups.Contains(group))
                    DisplayedEntries.AddRange(entries.Except(pausedEntries.TryGetValue(group, out List<DebugEntry> paused) ? paused : new()));

            DisplayedEntries.Sort((a, b) => b.timestamp.CompareTo(a.timestamp));
        }

        public static (string className, string methodName, string suffix) GetHumanReadableCaller(StackFrame frame) {
            if (frame == null) return ("Unknown", "Unknown", string.Empty);

            MethodBase method = frame.GetMethod();
            if (method == null) return ("Unknown", "Unknown", string.Empty);

            Type   declaringType = method.DeclaringType;
            string methodName    = method.Name;
            string className     = "Global";

            if (declaringType != null) {
                // Fix: If the class itself is compiler-generated (DisplayClass or StateMachine),
                // the REAL class is the one that contains this generated one.
                if (declaringType.Name.StartsWith("<") && declaringType.DeclaringType != null) {
                    className = declaringType.DeclaringType.Name;
                }
                else {
                    className = declaringType.Name;
                }
            }

            className = FormatClassName(className);

            // 1. Async/Iterator State Machines
            if (methodName == "MoveNext" && declaringType != null) {
                return (className, ExtractBetweenBrackets(declaringType.Name), "enumerator");
            }

            // 2. Local Functions
            if (methodName.Contains("g__")) {
                string parent   = ExtractBetweenBrackets(methodName);
                int    subStart = methodName.IndexOf("g__") + 3;
                int    subEnd   = methodName.LastIndexOf('|'); // Use LastIndexOf for safety
                string subName  = subEnd > subStart ? methodName.Substring(subStart, subEnd - subStart) : "local";
                return (className, parent, subName);
            }

            // 3. Lambdas (inside DisplayClasses or tagged with b__)
            if (methodName.Contains("b__") || declaringType.Name.Contains("DisplayClass")) {
                string cleanMethod = methodName.StartsWith("<") ? ExtractBetweenBrackets(methodName) : methodName;
                return (className, cleanMethod, "lambda");
            }

            return (className, methodName, string.Empty);
        }

        private static string ExtractBetweenBrackets(string input) {
            int start = input.IndexOf('<') + 1;
            int end   = input.IndexOf('>');
            return start > 0 && end > start ? input.Substring(start, end - start) : input;
        }

        private static string FormatClassName(string name) =>
            name.StartsWith("<") ? ExtractBetweenBrackets(name) : name;
#endif

    }

}
#endif