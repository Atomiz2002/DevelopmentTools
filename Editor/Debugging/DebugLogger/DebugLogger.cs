#if DEVELOPMENT_TOOLS_ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using EditorSettings = DevelopmentTools.Editor.Debugging.Settings.EditorSettings;

namespace DevelopmentTools.Editor.Debugging.DebugLogger {

    [CreateAssetMenu(fileName = "Debug Logger", menuName = "The Sixth Hammer/Debug Logger")]
    [HideMonoScript]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public class DebugLogger : SerializedScriptableObject {

#if UNITY_EDITOR

        [Space]
        [DisplayAsString(EnableRichText = true, Overflow = false)]
        [ListDrawerSettings(
            IsReadOnly = true,
            DraggableItems = false,
            ShowFoldout = false,
            ShowPaging = false,
            ShowItemCount = false)]
        [GUIColor(1, 1, 0)]
        public List<string> PinnedInfo = new();

        [PropertyOrder(10)]
        [HideLabel]
        [OnValueChanged(nameof(UpdateDisplayedEntries))]
        [EnumToggleButtons]
        public DebugLogGroup Group;

        [PropertyOrder(100)]
        [HorizontalGroup]
        [OnValueChanged(nameof(UpdateDisplayedEntries))]
        [Button]
        public void AddSeparator() {
            try {
                DebugEntry separator = new();
                debugEntries[Group].Add(separator);
                DisplayedEntries.Insert(0, separator);
            }
            catch (Exception e) {
                $"Failed to add separator:\n{e}".LogEx();
            }
        }

        private bool paused;

        [PropertyOrder(110)]
        [HorizontalGroup]
        [Button("@paused ? \"<color=#ff0000><b>Paused</b></color>\" : \"Pause\"")]
        private void TogglePauseEntries() {
            paused = !paused;

            if (paused)
                return;

            foreach (DebugEntry entry in pausedEntries[Group])
                DisplayedEntries.Insert(0, entry);

            pausedEntries[Group].Clear();
        }

        private byte                    clearConfirm;
        private CancellationTokenSource clearConfirmCancellationTokenSource;

        [PropertyOrder(120)]
        [HorizontalGroup]
        [DisableInEditorMode]
        [Button("@clearConfirm == 0 ? \"Clear\" : \"<color=#ff0000>Are you sure?</color>\"")]
        [GUIColor("@clearConfirm == 0 ? GUI.color : Color.red")]
        private void Clear() {
            clearConfirm++;

            clearConfirmCancellationTokenSource?.Cancel();
            clearConfirmCancellationTokenSource = new();

            Task.Run(async () => {
                try {
                    await Task.Delay(TimeSpan.FromSeconds(1), clearConfirmCancellationTokenSource.Token);
                    clearConfirm = 0;
                }
                catch (TaskCanceledException) {}
            });

            if (clearConfirm < 2)
                return;

            clearConfirmCancellationTokenSource.Cancel();
            clearConfirm = 0;
            debugEntries[Group].Clear();
            DisplayedEntries.Clear();
        }

        [ListDrawerSettings(
            IsReadOnly = true,
            DraggableItems = false,
            ShowFoldout = false,
            ShowPaging = true,
            NumberOfItemsPerPage = 20,
            ShowItemCount = true,
            ElementColor = nameof(DebugEntryColor))]
        [PropertyOrder(200)]
        [ItemNotNull]
        public List<DebugEntry> DisplayedEntries = new();

        private static DebugLogger instance;

        public static DebugLogger Instance {
            get {
                if (instance)
                    return instance;

                string guid = AssetDatabase.FindAssets($"t:{nameof(DebugLogger)}").FirstOrDefault();

                if (!string.IsNullOrEmpty(guid))
                    instance = guid.GUIDToPath().LoadAsset<DebugLogger>();

                if (!instance) {
                    instance      = CreateInstance<DebugLogger>();
                    instance.name = $"{nameof(DebugLogger).SplitPascalCase()} (Unsaved)";
                }

                Initialize();

                return instance;
            }
        }

        [OdinSerialize] [HideInInspector] private Dictionary<string, string>                  pinnedInfo    = new();
        [OdinSerialize] [HideInInspector] private Dictionary<DebugLogGroup, List<DebugEntry>> debugEntries  = new();
        [OdinSerialize] [HideInInspector] private Dictionary<DebugLogGroup, List<DebugEntry>> pausedEntries = new();

        private void UpdateDisplayedEntries() {
            EditorPrefs.SetInt(nameof(DebugLogger) + nameof(Group), (int) Group);

            DisplayedEntries = debugEntries[Group].Except(paused ? pausedEntries[Group] : new()).Reverse().ToList();

            if (EditorSettings.DebugLogger.MergeDuplicates)
                TryMergeLastEntries(Group);
        }

        [MenuItem(EditorSettings.MenuGroupPath + "Debug Logger")]
        public static void Show() {
            if (!EditorSettings.TryFocusWindow(Instance.name))
                EditorUtility.OpenPropertyEditor(Instance);
        }

        [ContextMenu("Fill list with test messages")]
        private void TestSystem() {
            Initialize();
            Log(null, Group);
            Log(null, Group, null);
        }

        private void OnEnable() {
            EditorApplication.playModeStateChanged -= PrepareForState;
            EditorApplication.playModeStateChanged += PrepareForState;
        }

        private void OnDisable() => EditorApplication.playModeStateChanged -= PrepareForState;

        [InitializeOnLoadMethod]
        public static void Initialize() {
            if (!Instance)
                return;

            // maybe store the selected group in SO if we decide to have more
            Instance.Group = (DebugLogGroup) EditorPrefs.GetInt(nameof(DebugLogger) + nameof(Group));

            foreach (DebugLogGroup group in Enum.GetValues(typeof(DebugLogGroup))) {
                Instance.debugEntries.TryAdd(group, new());
                Instance.pausedEntries.TryAdd(group, new());
            }
        }

        private void PrepareForState(PlayModeStateChange state) {
            try {
                switch (state) {
                    case PlayModeStateChange.EnteredPlayMode:
                        PinnedInfo.Clear();
                        pinnedInfo.Clear();

                        DisplayedEntries.Clear();
                        debugEntries.Clear();
                        pausedEntries.Clear();

                        Initialize();

                        break;

                    case PlayModeStateChange.ExitingPlayMode:
                        if (EditorSettings.DebugLogger.MergeDuplicates)
                            foreach (DebugLogGroup group in Enum.GetValues(typeof(DebugLogGroup)))
                                TryMergeLastEntries(group);

                        clearConfirm = 0;
                        break;
                }
            }
            catch (Exception e) {
                $"Failed to initialize DebugLogger:\n{e}".LogEx();
            }
        }
#endif

        #region Pin

        /// Use the <paramref name="key"/> to edit already pinned info
        public static void Pin([JetBrains.Annotations.NotNull] string key, params object[] info) {
#if UNITY_EDITOR

            try {
                if (info != null)
                    Instance.pinnedInfo[key] = $"{key.Colored(Color.red)} {info.Join().Flatten().SafeString()}".Bold();
                else
                    Instance.pinnedInfo.Remove(key);

                Instance.PinnedInfo = Instance.pinnedInfo.Values.ToList();
            }
            catch (Exception e) {
                $"Failed to un/pin DebugLogger info:\n{e}".LogEx();
            }

#endif
        }

        public static void Unpin([JetBrains.Annotations.NotNull] string key) => Pin(key);

        #endregion

        #region Log

        // @formatter:off
        public static void Log                  (object parameter,    DebugLogGroup group, params object[] details) => LogEntry(false, null, new[] { parameter }, group, false, false, false, details);
        public static void LogError             (object parameter,    DebugLogGroup group, params object[] details) => LogEntry(false, null, new[] { parameter }, group, false, false, true,  details);
        public static void LogSendEvent         (object parameter,    DebugLogGroup group, params object[] details) => LogEntry(false, null, new[] { parameter }, group, true,  false, false, details);
        public static void LogSendEventError    (object parameter,    DebugLogGroup group, params object[] details) => LogEntry(false, null, new[] { parameter }, group, true,  false, true,  details);
        public static void LogReceivedEvent     (object parameter,    DebugLogGroup group, params object[] details) => LogEntry(false, null, new[] { parameter }, group, true,  true,  false, details);
        public static void LogReceivedEventError(object parameter,    DebugLogGroup group, params object[] details) => LogEntry(false, null, new[] { parameter }, group, true,  true,  true,  details);

        public static void Log                  (object[] parameters, DebugLogGroup group, params object[] details) => LogEntry(false, null, parameters , group, false, false, false, details);
        public static void LogError             (object[] parameters, DebugLogGroup group, params object[] details) => LogEntry(false, null, parameters , group, false, false, true,  details);
        public static void LogSendEvent         (object[] parameters, DebugLogGroup group, params object[] details) => LogEntry(false, null, parameters , group, true,  false, false, details);
        public static void LogSendEventError    (object[] parameters, DebugLogGroup group, params object[] details) => LogEntry(false, null, parameters , group, true,  false, true,  details);
        public static void LogReceivedEvent     (object[] parameters, DebugLogGroup group, params object[] details) => LogEntry(false, null, parameters , group, true,  true,  false, details);
        public static void LogReceivedEventError(object[] parameters, DebugLogGroup group, params object[] details) => LogEntry(false, null, parameters , group, true,  true,  true,  details);

        public static void LogGoodDetails(object details)     => AddLastEntryDetails(null, details.ToString().Colored(Color.green));
        public static void LogBadDetails (object details)     => AddLastEntryDetails(null, details.ToString().Colored(Color.red));
        public static void LogDetails    (object details)     => AddLastEntryDetails(null, details);
        public static void LogReturn     (object returnValue) => SetEntryReturn     (null, returnValue);
        // @formatter:on

        #endregion

        private static void LogEntry(bool isQuantum, [CanBeNull] StackTrace stackTrace, [CanBeNull] object[] parameters,
            DebugLogGroup group, bool isEvent, bool isReceived, bool isError, params object[] details) {
#if UNITY_EDITOR
            try {
                DebugEntry newEntry = new(isQuantum, stackTrace, parameters, isEvent, isReceived, isError, details);

                if (EditorSettings.DebugLogger.MergeDuplicates)
                    TryMergeLastEntries(group);

                Instance.debugEntries[group].Add(newEntry);
                Instance.AddDisplayedEntry(group, newEntry);
            }
            catch (Exception e) {
                $"Failed to add DebugLog entry:\n{isQuantum}\n{stackTrace}\n{parameters}\n{isEvent}\n{isReceived}\n{isError}\n{details}\n{e}".LogEx();
            }
#endif
#if !UNITY_EDITOR
        new List<string> {
                $"New DebugEntry: {stackTrace ?? new StackTrace(2, true)}",
                $"Q: {isQuantum}",
                $"Params: {parameters?.SafeString("none")}",
                $"Details: {details?.SafeString("none")}",
                $"IsEvent: {isEvent}",
                $"Received: {isReceived}",
                $"IsError: {isError}"
            }.Join("\n")
            .Log();
#endif
        }

        private static void AddLastEntryDetails([CanBeNull] StackTrace stackTrace, object details) {
#if UNITY_EDITOR
            try {
                if (TryGetEntryWithMatchingCaller(stackTrace, out DebugEntry entry, out _))
                    entry.AddDetails(details.SafeString());
            }
            catch (Exception e) {
                $"Failed to add more details to last entry:\n{e}".LogEx();
            }
#endif
#if !UNITY_EDITOR
        $"Adding details: {details.SafeString("no details")} | to | {stackTrace ?? new StackTrace(2, true)}".Log();
#endif
        }

        private static void SetEntryReturn([CanBeNull] StackTrace stackTrace, object returnValue) {
#if UNITY_EDITOR
            try {
                if (TryGetEntryWithMatchingCaller(stackTrace, out DebugEntry entry, out DebugLogGroup group))
                    entry.SetReturn(returnValue);

                if (EditorSettings.DebugLogger.MergeDuplicates)
                    TryMergeLastEntries(group);
            }
            catch (Exception e) {
                $"Failed to set LogReturn to last entry:\n{e}".LogEx();
            }
#endif
#if !UNITY_EDITOR
        $"Set return value:\n{returnValue} | to | {stackTrace ?? new StackTrace(2, true)}".Log();
#endif
        }

#if UNITY_EDITOR

        private static void TryMergeLastEntries(DebugLogGroup group) {
            if (Instance.debugEntries[group].Count < 2) return;

            DebugEntry lastEntry    = Instance.debugEntries[group][^1];
            DebugEntry preLastEntry = Instance.debugEntries[group][^2];

            if (lastEntry.IsSeparator) return;
            if (preLastEntry.IsSeparator) return;

            if (preLastEntry.DisplayedCallerSignature != lastEntry.DisplayedCallerSignature) return;
            if (!IsMatchingAllDetails(preLastEntry, lastEntry)) return;
            if (!IsMatchingReturnValue(preLastEntry, lastEntry)) return;

            preLastEntry.IncreaseRepetitions();
            Instance.debugEntries[group].RemoveAt(Instance.debugEntries[group].Count - 1);

            if (group != Instance.Group
                || Instance.paused
                || Instance.DisplayedEntries.Count <= 1)
                return;

            Instance.DisplayedEntries[1].IncreaseRepetitions();
            Instance.DisplayedEntries.RemoveAt(0);
        }

        private static bool TryGetEntryWithMatchingCaller([CanBeNull] StackTrace stackTrace, out DebugEntry entry, out DebugLogGroup group) {
            stackTrace ??= new(3, true);

            foreach (DebugLogGroup g in Instance.debugEntries.Keys)
                if (Instance.debugEntries[g].LastOrDefault(e => IsSameFrameCaller(stackTrace, e.stackTrace)).Var(out entry) != null) {
                    group = g;
                    return true;
                }

            entry = null;
            group = default;
            return false;
        }

        private void AddDisplayedEntry(DebugLogGroup group, DebugEntry newEntry) {
            if (paused && !newEntry.IsSeparator)
                pausedEntries[group].Add(newEntry);
            else if (group == Group)
                DisplayedEntries.Insert(0, newEntry);
        }

        private static bool IsSameFrameCaller(StackTrace traceA, StackTrace traceB) => traceA?.GetFrame(0)?.GetMethod() == traceB?.GetFrame(0)?.GetMethod();

        private static bool IsMatchingAllDetails(DebugEntry lastEntry, DebugEntry newEntry) =>
            IsMatchingDetails(lastEntry.DisplayedDetails, newEntry.DisplayedDetails)
            && IsMatchingDetails(lastEntry.Details2, newEntry.Details2)
            && IsMatchingDetails(lastEntry.Details3, newEntry.Details3)
            && IsMatchingDetails(lastEntry.Details4, newEntry.Details4)
            && IsMatchingDetails(lastEntry.Details5, newEntry.Details5)
            && IsMatchingDetails(lastEntry.Details6, newEntry.Details6)
            && IsMatchingDetails(lastEntry.Details7, newEntry.Details7)
            && IsMatchingDetails(lastEntry.Details8, newEntry.Details8)
            && IsMatchingDetails(lastEntry.Details9, newEntry.Details9);

        private static bool IsMatchingDetails(string lastDetails, string newDetails) =>
            lastDetails == newDetails || lastDetails != null && newDetails != null && lastDetails.StartsWith(newDetails);

        private static bool IsMatchingReturnValue(DebugEntry lastEntry, DebugEntry newEntry) =>
            lastEntry.returnValue == newEntry.returnValue;

        private Color DebugEntryColor(int index, Color defaultColor) =>
            DisplayedEntries[index].IsSeparator
                ? Color.black
                : defaultColor;

#endif

    }

}
#endif