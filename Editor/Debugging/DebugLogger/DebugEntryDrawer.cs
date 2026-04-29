#if UNITY_EDITOR && !SIMULATE_BUILD && ENABLE_LOGS
using System.Linq;
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

            DrawDetails(entry.Details2, entry.Details2Timestamp);
            DrawDetails(entry.Details3, entry.Details3Timestamp);
            DrawDetails(entry.Details4, entry.Details4Timestamp);
            DrawDetails(entry.Details5, entry.Details5Timestamp);
            DrawDetails(entry.Details6, entry.Details6Timestamp);
            DrawDetails(entry.Details7, entry.Details7Timestamp);
            DrawDetails(entry.Details8, entry.Details8Timestamp);
            DrawDetails(entry.Details9, entry.Details9Timestamp);
            DrawDetails(entry.Details10, entry.Details10Timestamp);
            DrawDetails(entry.Details11, entry.Details11Timestamp);
            DrawDetails(entry.Details12, entry.Details12Timestamp);
            DrawDetails(entry.Details13, entry.Details13Timestamp);
            DrawDetails(entry.Details14, entry.Details14Timestamp);
            DrawDetails(entry.Details15, entry.Details15Timestamp);
            DrawDetails(entry.Details16, entry.Details16Timestamp);
            DrawDetails(entry.Details17, entry.Details17Timestamp);
            DrawDetails(entry.Details18, entry.Details18Timestamp);
            DrawDetails(entry.Details19, entry.Details19Timestamp);
            DrawDetails(entry.Details20, entry.Details20Timestamp);
            DrawDetails(entry.Details21, entry.Details21Timestamp);
            DrawDetails(entry.Details22, entry.Details22Timestamp);
            DrawDetails(entry.Details23, entry.Details23Timestamp);
            DrawDetails(entry.Details24, entry.Details24Timestamp);
            DrawDetails(entry.Details25, entry.Details25Timestamp);
            DrawDetails(entry.Details26, entry.Details26Timestamp);
            DrawDetails(entry.Details27, entry.Details27Timestamp);
            DrawDetails(entry.Details28, entry.Details28Timestamp);
            DrawDetails(entry.Details29, entry.Details29Timestamp);
            DrawDetails(entry.Details30, entry.Details30Timestamp);

            if (entry.showStackTrace) {
                ColoredBoxGroupDrawer.BeginColoredBox(Color.black);
                GUILayout.Label(entry.DisplayedStackTrace, SirenixGUIStyles.RichTextLabel);
                ColoredBoxGroupDrawer.EndBox();
                GUILayout.Label(entry.guid.ToString().Size(15), SirenixGUIStyles.RichTextLabel);
            }

            #endregion

            EditorGUILayout.EndVertical();

            #endregion

            #region Icons

            if (entry.Icon1) {
                EditorGUILayout.BeginVertical(GUILayout.Width(64));

                PreviewTexture2DAttributeDrawer<Texture2D>.Draw(entry.Icon1, outline: Color.black, thickness: 2);

                if (entry.Icon2) {
                    PreviewTexture2DAttributeDrawer<Texture2D>.Draw(entry.Icon2, outline: Color.black, thickness: 2);

                    if (entry.Icon3) {
                        PreviewTexture2DAttributeDrawer<Texture2D>.Draw(entry.Icon3, outline: Color.black, thickness: 2);

                        if (entry.Icon4) {
                            PreviewTexture2DAttributeDrawer<Texture2D>.Draw(entry.Icon4, outline: Color.black, thickness: 2);

                            if (entry.Icon5)
                                PreviewTexture2DAttributeDrawer<Texture2D>.Draw(entry.Icon5, outline: Color.black, thickness: 2);
                        }
                    }
                }

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
                    ? Color.red.A(.15f)
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