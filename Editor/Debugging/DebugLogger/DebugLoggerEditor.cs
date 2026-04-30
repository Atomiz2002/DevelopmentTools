#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR && UNITY_EDITOR && !SIMULATE_BUILD && ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using DevelopmentTools.Editor.AttributeDrawers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.Debugging {

    [CustomEditor(typeof(DebugLogger))]
    public class DebugLoggerEditor : OdinEditor {

        private static List<string> categories;
        private static int          indexToFocus = -1;

        private DebugLogger t;
        private DebugEntry  selectedEntry;
        private int         selectedEntryOccurrences;
        /// otherwise compare StackTrace
        private bool compareCaller;
        private int page = 1;

        private List<string>                         DisplayedPinnedInfo => t.DisplayedPinnedInfo;
        private List<DebugEntry>                     DisplayedEntries    => t.DisplayedEntries;
        private Dictionary<string, List<DebugEntry>> DebugEntries        => t.debugEntries;
        private Dictionary<string, List<DebugEntry>> PausedEntries       => t.pausedEntries;
        private List<string>                         SelectedGroups      => t.SelectedGroups;

        private byte                    clearConfirm;
        private CancellationTokenSource clearConfirmCancellationTokenSource;

        protected override void OnEnable() {
            t             = (DebugLogger) target;
            selectedEntry = null;
            page          = 1;
        }

        public override void OnInspectorGUI() {
            if (categories != null) {
                DrawCategories();
                return;
            }

            DrawPinnedInfo();
            DrawButtons();
            EditorGUILayout.Space(1);
            DrawEntries();
            Repaint();
        }

        private void DrawCategories() {
            // 1. Ensure list is never empty
            if (categories.Count == 0) {
                categories.Add("");
                indexToFocus = 0;
            }

            // 2. Handle Deferred Focus
            if (indexToFocus != -1 && Event.current.type == EventType.Repaint) {
                GUI.FocusControl(".".Repeat(indexToFocus + 1));
                indexToFocus = -1;
            }

            bool enterPressed = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
            bool escapePressed = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape;

            // 3. Draw Fields
            for (int i = 0; i < categories.Count; i++) {
                GUI.SetNextControlName(".".Repeat(i + 1));
                Color color = categories[i].IsValidClassName() ? Color.green : Color.red;
                categories[i] = GUILayout.TextField(categories[i], new GUIStyle(GUI.skin.textField) { normal = { textColor = color }, focused = { textColor = color } });
            }

            string focusedControl = GUI.GetNameOfFocusedControl();
            int    focusedIndex   = focusedControl.Length - 1;

            if (focusedIndex >= 0) {
                if (enterPressed) {
                    Event.current.Use();

                    if (Event.current.control || Event.current.command) {
                        GenerateCategories();
                        return;
                    }

                    if (categories[focusedIndex].IsNullOrWhiteSpace()) // Prevent adding empty/duplicate categories
                        return;

                    int nextIndex = focusedIndex + 1;

                    if (nextIndex >= categories.Count) {
                        if (categories.HasDuplicates(categories[focusedIndex])) {
                            categories[focusedIndex] = "";
                            indexToFocus             = focusedIndex;
                        }
                        else {
                            categories.Add("");
                            indexToFocus = nextIndex;
                        }
                    }
                    else {
                        indexToFocus = nextIndex;
                    }
                }
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate"))
                GenerateCategories();

            if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)) || escapePressed)
                categories = null;

            GUILayout.EndHorizontal();
        }

        private void DrawPinnedInfo() {
            /* the spaces are for indentation (looks better) */
            string pinnedInfo = DisplayedPinnedInfo.JoinSmart("\n ", string.Empty);

            if (!pinnedInfo.IsNullOrEmpty()) {
                ColoredBoxGroupDrawer.BeginTitledBox("Pinned Info", true);
                GUILayout.Label(" " + pinnedInfo, SirenixGUIStyles.RichTextLabel);
                ColoredBoxGroupDrawer.EndBox(Color.yellow);
            }
        }

        private void DrawButtons() {
            GUILayout.BeginHorizontal();
            DrawButtonAddSeparator();
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying && !PausedEntries.Any());
            DrawButtonPause();
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            DrawButtonClear();
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        private void DrawButtonAddSeparator() {
            if (!SirenixEditorGUI.SDFIconButton("Add Separator", 22, SdfIconType.Dash))
                return;

            foreach (string group in SelectedGroups)
                if (DebugEntries.ContainsKey(group)) {
                    t.debugEntries[group].Add(DebugEntry.Separator());
                    t.UpdateDisplayedEntries();
                }
        }

        private void DrawButtonPause() {
            if (!SirenixEditorGUI.SDFIconButton("Pause", 22, SdfIconType.Pause))
                return;

            foreach (string group in SelectedGroups)
                if (DebugEntries.ContainsKey(group))
                    if (PausedEntries.TryGetValue(group, out List<DebugEntry> paused)) {
                        DebugEntries[group].AddRange(paused);
                        PausedEntries.Remove(group);

                        t.UpdateDisplayedEntries();
                    }
                    else {
                        PausedEntries[group] = new();
                    }
        }

        private void DrawButtonClear() {
            if (!SirenixEditorGUI.SDFIconButton(clearConfirm == 0
                    ? Event.current.shift || Event.current.control || Event.current.alt
                        ? "Clear All Groups".Colored(Color.red)
                        : "Clear"
                    : "Are you sure?".Colored(Color.red), 22, SdfIconType.X))
                return;

            clearConfirm++;

            clearConfirmCancellationTokenSource?.Cancel();
            clearConfirmCancellationTokenSource = new();

            UniTask.Void(async () => {
                try {
                    await UniTask.WaitForSeconds(1, cancellationToken: clearConfirmCancellationTokenSource.Token);
                    clearConfirm = 0;
                }
                catch {}
            });

            if (clearConfirm < 2)
                return;

            clearConfirmCancellationTokenSource.Cancel();
            clearConfirm = 0;
            DisplayedEntries.Clear();

            if (Event.current.shift || Event.current.control || Event.current.alt)
                DebugEntries.Clear();
            else {
                foreach (string group in SelectedGroups)
                    DebugEntries[group].Clear();
            }
        }

        private void DrawEntries() {
            const int entriesPerPage = 20;
            float     currentWidth   = 0;
            float     maxWidth       = EditorGUIUtility.currentViewWidth - 60 - 70; // 70 -> page buttons width

            SirenixEditorGUI.BeginToolbarBox();
            EditorGUILayout.Space(1);
            SirenixEditorGUI.BeginToolbarBoxHeader();

            foreach (string group in DebugEntries.Keys) {
                if (DebugEntries[group].Count == 0)
                    continue;

                bool  isActive = SelectedGroups.Contains(group);
                Color color    = DebugEntries[group][0].color;

                float btnWidth = SirenixGUIStyles.ToolbarTab.CalcSize(group).x + 25;
                float rowWidth = currentWidth + btnWidth;

                if (maxWidth < rowWidth) { // break the row
                    maxWidth -= 70; // the page buttons width
                    SirenixEditorGUI.EndToolbarBoxHeader();
                    EditorGUILayout.Space(2);
                    SirenixEditorGUI.BeginToolbarBoxHeader();
                    currentWidth = 0;
                }

                Rect rect = EditorGUILayout.GetControlRect(false, 22, GUILayout.MinWidth(btnWidth)).SubY(2);

                if (SirenixEditorGUI.SDFIconButton(rect, group, PausedEntries.ContainsKey(group) ? SdfIconType.Pause : SdfIconType.None, IconAlignment.RightOfText, new GUIStyle(SirenixGUIStyles.ToolbarTab) { normal = { textColor = color }, hover = { textColor = color } })) {
                    if (isActive)
                        SelectedGroups.Remove(group);
                    else
                        SelectedGroups.Add(group);

                    t.UpdateDisplayedEntries();
                }

                if (isActive)
                    SirenixEditorGUI.DrawRoundRect(GUILayoutUtility.GetLastRect().SubY(2).Expand(-3).SetWidth(5), color, 4);

                currentWidth += btnWidth;
            }

            #region Page controls

            if (GUILayout.Button("◀", SirenixGUIStyles.ToolbarTab, GUILayout.Width(20)))
                page = Math.Max(1, page - 1);

            page = EditorGUILayout.IntField(page, SirenixGUIStyles.ToolbarTab, GUILayout.Width(30));

            int totalPages = Mathf.CeilToInt((float) DisplayedEntries.Count / entriesPerPage);

            if (GUILayout.Button("▶", SirenixGUIStyles.ToolbarTab, GUILayout.Width(20)))
                page = Math.Min(totalPages, page + 1);

            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            #endregion

            SirenixEditorGUI.EndToolbarBoxHeader();
            EditorGUILayout.Space(2);

            Event e = Event.current;

            int start = (page - 1) * entriesPerPage;
            int end   = Math.Min(start + entriesPerPage, DisplayedEntries.Count);

            for (int i = start; i < end; i++) {
                DebugEntry entry = DisplayedEntries[i];

                DebugEntryDrawer.Draw(entry);

                Rect entryRect = GUILayoutUtility.GetLastRect();

                if (entry.IsSeparator)
                    SirenixEditorGUI.DrawRoundRect(entryRect, Color.black, 3);

                if (e.control) {
                    EditorGUIUtility.AddCursorRect(entryRect, MouseCursor.Orbit);

                    if (e.type == EventType.MouseDown && entryRect.Contains(e.mousePosition)) {
                        entry.OpenCaller();
                        e.Use();
                    }
                }

                if (selectedEntry != null
                    && (compareCaller && selectedEntry.DisplayedCallerSignature.EndAt("\n") != entry.DisplayedCallerSignature.EndAt("\n")
                        || !compareCaller && selectedEntry.DisplayedStackTrace != entry.DisplayedStackTrace))
                    SirenixEditorGUI.DrawRoundRect(entryRect, Color.black.A(.3f), 3);

                if (e.type == EventType.MouseDown) {
                    switch (e.button) {
                        case 0: // left
                            if (entryRect.Contains(e.mousePosition)) {
                                selectedEntry = compareCaller && selectedEntry?.DisplayedCallerSignature.EndAt("\n") == entry.DisplayedCallerSignature.EndAt("\n") ? null : entry; // && compareCaller to allow overwriting compare method on second different click
                                compareCaller = true;
                                Repaint();
                            }

                            break;
                        case 1: // right
                            if (entryRect.Contains(e.mousePosition)) {
                                selectedEntry = !compareCaller && selectedEntry?.DisplayedStackTrace == entry.DisplayedStackTrace ? null : entry; // && !compareCaller to allow overwriting compare method on second different click
                                compareCaller = false;
                                Repaint();
                            }

                            break;
                        case 2: // middle
                            selectedEntry = null;
                            Repaint();
                            break;
                    }
                }
            }

            EditorGUILayout.Space(-1);
            SirenixEditorGUI.EndToolbarBox();
        }

        [MenuItem("CONTEXT/DebugLogger/Edit Categories")]
        private static void EditCategories() {
            categories   = typeof(DebugLogger).GetNestedTypes().Select(t => t.Name).ToList();
            indexToFocus = categories.Count - 1;
        }

        private static void GenerateCategories() {

            #region asmref

            // @formatter:off
            const string asmdefRef = "{\n" +
                                     "    \"reference\": \"DevelopmentTools\"\n"
                                     + "}";
            // @formatter:on

            #endregion

            Directory.CreateDirectory(DebugLogger.CategoriesDir);

            foreach (string file in Directory.GetFiles(DebugLogger.CategoriesDir))
                File.Delete(file);

            categories.RemoveAll(x => x.IsNullOrWhiteSpace());

            foreach (string category in categories) {
                if (!category.IsValidClassName())
                    continue;

                #region script

                // @formatter:off
                string script = "using System.Diagnostics;\n" +
                                "using System.Runtime.CompilerServices;\n" +
                                "using DevelopmentEssentials.Extensions.CS;\n" +
                                "using UnityEngine;\n" +
                                "\n" +
                                "namespace " + nameof(DevelopmentTools) + " {\n" +
                                "\n" +
                                "    public partial class " + nameof(DebugLogger) + " {\n" +
                                "\n" +
                                "        public static class " + category + " {\n" +
                                "\n" +
                                "            private const string conditional = \"" + nameof(DebugLogger) + "\" + \"_\" + \"" + category + "\";\n" +
                                "\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void Log                  (object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   \"" + category + "\", false, false, false, details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogError             (object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   \"" + category + "\", false, false, true,  details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogSendEvent         (object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   \"" + category + "\", true,  false, false, details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogSendEventError    (object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   \"" + category + "\", true,  false, true,  details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogReceivedEvent     (object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   \"" + category + "\", true,  true,  false, details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogReceivedEventError(object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   \"" + category + "\", true,  true,  true,  details);\n" +
                                "\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void Log                  (ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), \"" + category + "\", false, false, false, details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogError             (ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), \"" + category + "\", false, false, true,  details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogSendEvent         (ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), \"" + category + "\", true,  false, false, details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogSendEventError    (ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), \"" + category + "\", true,  false, true,  details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogReceivedEvent     (ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), \"" + category + "\", true,  true,  false, details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogReceivedEventError(ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), \"" + category + "\", true, true, true, details);\n" +
                                "\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogGoodDetails(params object[] details) => AddGoodDetails(\"" + category + "\", details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogBadDetails (params object[] details) => AddBadDetails (\"" + category + "\", details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogDetails    (params object[] details) => AddDetails    (\"" + category + "\", details);\n" +
                                "        [Conditional(conditional)] [HideInCallstack] public static void LogReturn     (object returnValue)      => SetReturn     (\"" + category + "\", returnValue);\n" +
                                "\n" +
                                "        }\n" +
                                "\n" +
                                "    }\n" +
                                "\n" +
                                "}";
                // @formatter:on

                #endregion

                string scriptPath = Path.Combine(DebugLogger.CategoriesDir, category + ".cs");
                File.WriteAllText(scriptPath, script);
                AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate);
            }

            string asmdefRefPath = Path.Combine(DebugLogger.CategoriesDir, "_developmentTools.asmref");
            File.WriteAllText(asmdefRefPath, asmdefRef);
            AssetDatabase.ImportAsset(asmdefRefPath, ImportAssetOptions.ForceUpdate);

            AssetDatabase.Refresh();

            categories = null;
        }

    }

}
#endif