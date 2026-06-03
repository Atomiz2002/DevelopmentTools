#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR && !SIMULATE_BUILD && ENABLE_LOGS
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Editor.AttributeDrawers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.Debugging {

    public class DebugEntryDrawer : OdinDrawer {
        private DebugEntry entry;

        private static GUIStyle        detailsTimestampStyle;
        private static GUILayoutOption width22;

        protected override void Initialize() {
            entry = (DebugEntry) Property.ValueEntry.WeakSmartValue;
        }

        public override bool CanDrawProperty(InspectorProperty property) => typeof(DebugEntry).IsAssignableFrom(property?.ValueEntry?.TypeOfValue);

        protected override void DrawPropertyLayout(GUIContent label) => Draw(entry);

        public static void Draw(DebugEntry entry) {
            detailsTimestampStyle ??= new(SirenixGUIStyles.RichTextLabel) { alignment = TextAnchor.MiddleRight };
            width22               ??= GUILayout.Width(22);

            #region b Entry

            ColoredBoxGroupDrawer.BeginColoredBox(LogTypeColor(entry));

            #region h Caller Icons

            EditorGUILayout.BeginHorizontal();

            #region v Caller Detals

            EditorGUILayout.BeginVertical();

            #region h Caller Buttons

            EditorGUILayout.BeginHorizontal();

            #region Caller

            GUILayout.Label(entry.DisplayedCallerSignature, SirenixGUIStyles.RichTextLabel);
            GUILayout.FlexibleSpace();

            #endregion

            #region v Buttons Time

            EditorGUILayout.BeginVertical();

            #region h Buttons

            EditorGUILayout.BeginHorizontal(GUILayout.Width(25));

            #region Buttons

            if (!entry.IsSeparator) {
                EditorGUILayout.Space(1);
                DrawButtonOpenCaller(entry);
                EditorGUILayout.Space(1);
                DrawButtonLogEvent(entry);
                EditorGUILayout.Space(1);
                DrawButtonLogStackTrace(entry);
                EditorGUILayout.Space(1);
                DrawButtonShowStackTrace(entry);
            }

            #endregion

            EditorGUILayout.EndHorizontal();

            #endregion

            #region Time

            GUILayout.Label(entry.DisplayedTime, new GUIStyle(SirenixGUIStyles.RichTextLabel) { alignment = TextAnchor.MiddleRight });

            #endregion

            EditorGUILayout.EndVertical();

            #endregion

            EditorGUILayout.EndHorizontal();

            #endregion

            #region Details StackTrace

            DrawDetails(entry.DisplayedDetails, string.Empty);

            if (entry.Details != null) // impossible but happens
                foreach ((string details, string timestamp) in entry.Details)
                    DrawDetails(details, timestamp);

            if (entry.showStackTrace) {
                ColoredBoxGroupDrawer.BeginColoredBox(Color.black);
                GUILayout.Label(entry.DisplayedStackTrace, SirenixGUIStyles.RichTextLabel);
                ColoredBoxGroupDrawer.EndBox();
                GUILayout.Label(entry.guid.ToString(), SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            }

            #endregion

            EditorGUILayout.EndVertical();

            #endregion

            #region Icons

            if (entry.Icons.Any()) {
                EditorGUILayout.BeginVertical(GUILayout.Width(64));

                foreach (Texture2D icon in entry.Icons.Existing())
                    PreviewTexture2DAttributeDrawer<Texture2D>.Draw(icon, outline: Color.black, thickness: 2);

                EditorGUILayout.EndVertical();
            }

            #endregion

            EditorGUILayout.EndHorizontal();

            #endregion

            ColoredBoxGroupDrawer.EndBox(GroupColor(entry));

            #endregion

            void DrawDetails(string details, string timestamp) {
                if (details.IsNullOrWhiteSpace())
                    return;

                SirenixEditorGUI.BeginBox();
                GUILayout.Label(details, SirenixGUIStyles.RichTextLabel);
                SirenixEditorGUI.EndBox();
                GUI.Label(GUILayoutUtility.GetLastRect().SubXMax(3), timestamp.Size(10), detailsTimestampStyle);
            }
        }

        private static void DrawButtonOpenCaller(DebugEntry entry) {
            Rect btn = GUILayoutUtility.GetAspectRect(1, GUIStyle.none, width22);
            if (SirenixEditorGUI.SDFIconButton(btn, SdfIconType.EyeFill, null))
                entry.OpenCaller();
        }

        private static void DrawButtonLogEvent(DebugEntry entry) {
            SdfIconType icon   = SdfIconType.CaretRightFill;
            Color       color  = GUI.color;
            string      action = "▶ Logged";

            if (entry.IsEvent)
                if (entry.Received) {
                    icon   = SdfIconType.CaretDownFill;
                    color  = System.Drawing.Color.OrangeRed.ToUnityColor();
                    action = "▼ Received";
                }
                else {
                    icon   = SdfIconType.CaretUpFill;
                    color  = System.Drawing.Color.CornflowerBlue.ToUnityColor();
                    action = "▲ Sent";
                }

            action = entry.DisplayedCallerSignature?.Unformatted().StartsWith("[Q]") ?? false ? $"{"[Q]".Colored(Color.cyan)} {action}" : action;

            GUIHelper.PushColor(color);

            Rect btn = GUILayoutUtility.GetAspectRect(1, GUIStyle.none, width22);

            if (SirenixEditorGUI.SDFIconButton(btn, icon, null))
                action.Colored(color).LOG();

            GUIHelper.PopColor();
        }

        private static void DrawButtonLogStackTrace(DebugEntry entry) {
            Rect btn = GUILayoutUtility.GetAspectRect(1, GUIStyle.none, width22);
            if (!SirenixEditorGUI.SDFIconButton(btn, SdfIconType.CodeSlash, null))
                return;

            // TODO extension method for linkful stacktrace
            (entry.stackFrames?
                 .Select(f => {
                     string filePath   = f.GetFileName() ?? "Unknown path";
                     int    lineNumber = f.GetFileLineNumber();
                     return $"{filePath.StartAt("Assets", true, "...")}:{lineNumber}".Link(filePath.StartAt("Assets"), lineNumber);
                 })
                 .Where(l => l.Unformatted().StartsWith("...") && !l.Unformatted().Contains("Core\\Core.cs"))
                 .JoinSmart("\n")
             ?? entry.DisplayedStackTrace
             + "\n\n\n").LOG();
        }

        private static void DrawButtonShowStackTrace(DebugEntry entry) {
            Rect btn = GUILayoutUtility.GetAspectRect(1, GUIStyle.none, width22);
            if (!SirenixEditorGUI.SDFIconButton(btn, SdfIconType.ChevronBarExpand, null))
                return;

            entry.showStackTrace = !entry.showStackTrace;
        }

        private static Color LogTypeColor(DebugEntry entry) =>
            entry.IsSeparator
                ? entry.color
                : entry.IsError
                    ? Color.red.A(.3f)
                    : GUI.color;

        private static Color GroupColor(DebugEntry entry) =>
            entry.IsSeparator
                ? Color.black
                : entry.color == Color.clear
                    ? GUI.color
                    : entry.color;
    }

}
#endif