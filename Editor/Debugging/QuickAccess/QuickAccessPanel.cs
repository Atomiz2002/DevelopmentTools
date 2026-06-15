#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR && !SIMULATE_BUILD
using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Settings;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Color = System.Drawing.Color;

namespace DevelopmentTools.Editor.Debugging.QuickAccess {

    public class QuickAccessPanel : EditorWindow {

        private static          List<GlobalObjectId>                             pinned           = new();
        private static          List<GlobalObjectId>                             history          = new(); // todo store groups of selected objects
        private static readonly List<(GlobalObjectId id, Object o, bool pinned)> elements         = new();
        private static readonly List<GlobalObjectId>                             queuedForRemoval = new();

        private static Vector2 scroll;
        private static bool    stayOpen;
        private static int     selectedIndex;

        private const           float elementScale                = 1.1f;
        private const           float elementScaleSelected        = 1.3f;
        private static readonly float elementHeight               = EditorGUIUtility.singleLineHeight * elementScale;
        private static readonly float elementExpandByOnHover      = EditorGUIUtility.singleLineHeight / 4;
        private static readonly float elementLabelExpandByOnHover = elementExpandByOnHover * 1.5f;
        private static readonly float elementIconExpandByOnHover  = EditorGUIUtility.singleLineHeight * 2;

        private static GUIStyle dragToPin;
        private static GUIStyle pinnedPin;
        private static GUIStyle unpinnedPin;
        private static GUIStyle pinnedElement;
        private static GUIStyle pinnedElementSelected;
        private static GUIStyle historyElement;
        private static GUIStyle historyElementSelected;
        private static GUIStyle historyElementPinned;

        public static new void Show(bool stayOpen) {
            QuickAccessPanel.stayOpen = stayOpen;
            GetWindow<QuickAccessPanel>("Quick Access").Show();
        }

        [InitializeOnLoadMethod]
        private static void Initialize() {
            // ToolbarGUIInjector.AddToolbarPopupButton(ToolbarGUIInjector.ToolbarSide.RightOfPlay, "Quick Access", 110, DrawPopupGUI, 300, 700); // TODO: BROKEN

            Load();
            Selection.selectionChanged += () => {
                Object selected = Selection.activeObject.n() ?? Selection.objects.FirstOrDefault();

                if (!selected)
                    return;

                if (history.Contains(selected.GlobalId(out GlobalObjectId id)))
                    history.Remove(id);

                history.Insert(0, id);

                history = history.Distinct().ToList();
                Save();
            };
        }

        [MenuItem(EngineSettings.MenuGroupPath + "Quick Access Panel %q")]
        private static void OpenWindow(MenuCommand command) => Show(false);

        private void OnEnable() {
            Load();

            InitializeStyles();

            selectedIndex = (elements.Count > 1 && Selection.activeObject.GlobalId().Equals(elements[0].id) ? 1 : 0) + elements.Count(x => x.pinned);
            scroll        = Vector2.zero;
        }

        private static void InitializeStyles() {
            GUIStyle label            = GUI.skin.label;
            int      fontSize         = Mathf.FloorToInt(label.fontSize * elementScale);
            int      fontSizeSelected = Mathf.FloorToInt(label.fontSize * elementScaleSelected);

            UnityEngine.Color white = UnityEngine.Color.white;
            UnityEngine.Color cyan  = UnityEngine.Color.cyan;
            UnityEngine.Color gold  = Color.Gold.ToUnityColor();

            dragToPin = new(label) {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize  = 22
            };

            pinnedElement = new(label) {
                fontStyle = FontStyle.Bold,
                fontSize  = fontSize,
                normal    = { textColor = white },
                hover     = { textColor = cyan }
            };

            pinnedElementSelected = new(pinnedElement) {
                fontSize  = fontSizeSelected,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = gold },
                hover     = { textColor = gold }
            };

            historyElement = new(GUI.skin.label) {
                fontSize = fontSize,
                hover    = { textColor = cyan }
            };

            historyElementSelected = new(historyElement) {
                fontSize  = fontSizeSelected,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = cyan },
                hover     = { textColor = cyan }
            };

            historyElementPinned = new(historyElement) {
                fontStyle = FontStyle.Bold
            };
        }

        private void OnGUI() => DrawGUI(this, stayOpen);

        private static void DrawPopupGUI() => DrawGUI(null, true);

        private static void DrawGUI(QuickAccessPanel window, bool stayOpen) {
            if (window) {
                QuickAccessPanel.stayOpen = stayOpen;

                if (!QuickAccessPanel.stayOpen && !Event.current.control) {
                    window.Close();
                    return;
                }

                if (Event.current.alt) {
                    selectedIndex = -1;
                    window.Close();
                    return;
                }
            }

            // Q to iterate history elements, W to iterate backwards
            if (Event.current.type != EventType.Layout) { // prevent index changes during Layout to fix GUI error
                if (history.Any() && Event.current.type == EventType.KeyDown) {
                    switch (Event.current.keyCode) {
                        case KeyCode.Q:
                            if (Event.current.shift)
                                selectedIndex--;
                            else
                                selectedIndex++;

                            break;

                        case KeyCode.W:
                            selectedIndex--;
                            break;
                    }

                    selectedIndex = (selectedIndex + elements.Count) % elements.Count;
                    scroll.y      = elementHeight * (selectedIndex - window.position.height / elementHeight / 3); // /2 for center, /3 for a little higher
                    Event.current.Use();
                }
            }

            // Rect windowRect = new(Vector2.zero, window.position.size);
            // EditorHelper.DrawDropZone<Object>(windowRect,
            //     dropped => {
            //         pinned.AddRange(dropped);
            //         pinned = pinned.Distinct().ToList();
            //     });

            // DrawCopyPaste(); // not fully working yet

            if (!elements.Any())
                return;

            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawElements();

            EditorGUILayout.EndScrollView();

            // if (!pinned.Any())
            //     DrawDropZoneUI(windowRect);

            window.n()?.Repaint();
        }

        private static void DrawElements() {
            Rect container   = EditorGUILayout.GetControlRect(false, elements.Count * elementHeight); // reserves the space
            Rect elementRect = container;

            SirenixEditorGUI.DrawRoundRect(elementRect.SetHeight(elements.Count(x => x.pinned) * elementHeight), UnityEngine.Color.black.A(.2f), 4, Color.Gold.ToUnityColor().A(.4f), 1);

            elementRect.height = elementHeight;

            Rect pinRect   = elementRect;
            Rect iconRect  = elementRect;
            Rect labelRect = elementRect;

            pinRect.width   =  elementHeight;
            iconRect.width  =  elementHeight;
            labelRect.width -= (elementHeight + 2) * 2;

            pinRect.x   += 3;
            iconRect.x  += iconRect.width + 2;
            labelRect.x += (elementHeight + 2) * 2;

            for (int i = 0; i < elements.Count; i++) {
                (_, Object element, bool isPinned) = elements[i];
                bool hovered = Event.current.type != EventType.Layout && elementRect.Contains(Event.current.mousePosition);

                if (hovered)
                    selectedIndex = i;

                GUIStyle style = isPinned
                    ? historyElementPinned
                    : historyElement;

                GUIHelper.PushColor((isPinned && i >= elements.Count(x => x.pinned) ? Color.Gold.ToUnityColor() : GUI.color).A(.5f));
                GUI.DrawTexture(pinRect.AlignCenterXY(16), EditorGUIUtility.IconContent(EditorUtility.IsPersistent(element) ? "Project" : "UnityEditor.SceneHierarchyWindow").image);
                GUIHelper.PopColor();

                element.GetIcon().DrawIcon(iconRect.AlignCenterXY(16), ScaleMode.ScaleToFit);
                GUI.Label(labelRect, element.name, style);

                elementRect.y = pinRect.y = iconRect.y = labelRect.y += elementHeight;
            }

            DrawSelectedElement(container, elementRect, pinRect, iconRect, labelRect);
        }

        private static void DrawSelectedElement(Rect container, Rect elementRect, Rect pinRect, Rect iconRect, Rect labelRect) {
            if (selectedIndex < 0)
                return;

            (GlobalObjectId id, Object selectedElement, bool isPinned) = elements[selectedIndex];
            GUIStyle         selectedElementStyle = isPinned ? pinnedElementSelected : historyElementSelected;
            IHaveIconPreview selectedElementIcon  = selectedElement.GetIcon();
            // float    selectedElementIconAspect = selectedElementIcon ? (float) selectedElementIcon.width / selectedElementIcon.height : 1;

            float selectedElementRectY = selectedIndex * elementHeight;

            elementRect.y = pinRect.y = iconRect.y = labelRect.y = selectedElementRectY;

            elementRect = elementRect.Expand(0, elementExpandByOnHover);
            iconRect    = iconRect.Expand(elementIconExpandByOnHover).AddX(labelRect.width - elementIconExpandByOnHover);
            // iconRect.width =  iconRect.height * selectedElementIconAspect;
            iconRect.x -= iconRect.width - iconRect.height;
            iconRect.y =  Mathf.Min(Mathf.Max(iconRect.y, 0), Mathf.Max(iconRect.height, container.yMax) - iconRect.height);
            labelRect  =  labelRect.Expand(0, elementLabelExpandByOnHover).SubXMin(elementHeight);

            SirenixEditorGUI.DrawRoundRect(
                elementRect,
                EditorHelper.BackgroundColor(),
                4,
                selectedElementStyle.hover.textColor,
                2);

            if (isPinned) {
                SdfIconType icon = pinRect.Contains(Event.current.mousePosition) ? SdfIconType.PinAngleFill : SdfIconType.PinFill;
                if (SirenixEditorGUI.SDFIconButton(pinRect, icon, GUI.skin.label))
                    Unpin(id);
            }
            else {
                SdfIconType icon = pinRect.Contains(Event.current.mousePosition) ? SdfIconType.PinAngle : SdfIconType.Pin;
                if (SirenixEditorGUI.SDFIconButton(pinRect, icon, GUI.skin.label))
                    Pin(id);
            }

            GUI.Label(labelRect, selectedElement.name, selectedElementStyle);

            if (selectedElementIcon?.Icon) {
                SirenixEditorGUI.DrawRoundRect(
                    iconRect,
                    (EditorHelper.BackgroundColor() * (Mathf.PingPong((float) EditorApplication.timeSinceStartup / 3, 0.4f) + 0.5f)).A(1),
                    4,
                    selectedElementStyle.hover.textColor,
                    2);

                selectedElementIcon.DrawIcon(iconRect.Expand(-2), ScaleMode.ScaleToFit);
            }

            if (Event.current.isMouse) {
                if (Event.current.OnLeftClick(elementRect)) {
                    selectedElement.Ping();
                }
                else if (Event.current.OnContextClick(elementRect)) {
                    if (isPinned)
                        Unpin(id);
                    else
                        Pin(id);
                }
            }
        }

        private static void DrawDropZoneUI(Rect windowRect) {
            EditorHelper.DrawDashedBorder(windowRect, 10f, 5f, 5f, Color.Gray.ToUnityColor());
            EditorGUI.LabelField(windowRect, "Drag here to pin", dragToPin);
        }

        private static void DrawCopyPaste() {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy")) EditorPrefs.GetString(nameof(pinned)).CopyObjToClipboard();
            if (GUILayout.Button("Paste")) EditorPrefs.SetString(nameof(pinned), EditorGUIUtility.systemCopyBuffer);
            GUILayout.EndHorizontal();
        }

        public new void Close() {
            base.Close();

            pinned.RemoveAll(queuedForRemoval.Contains);
            queuedForRemoval.Clear();

            Save();

            if (selectedIndex >= 0)
                elements[selectedIndex].o.SelectAndPing();

            EditorUtility.FocusProjectWindow();
        }

        private static void Load() {
            pinned  = DeserializeList(EditorPrefs.GetString(nameof(pinned)));
            history = DeserializeList(EditorPrefs.GetString(nameof(history)));
            FillElements();
        }

        private static void Save() {
            EditorPrefs.SetString(nameof(pinned), SerializeList(pinned));
            EditorPrefs.SetString(nameof(history), SerializeList(history));
            FillElements();
        }

        private static void FillElements() {
            elements.Clear();

            foreach (GlobalObjectId id in pinned)
                if (id.ToObject(out Object o))
                    elements.Add(id, o, true);

            foreach (GlobalObjectId id in history)
                if (id.ToObject(out Object o))
                    elements.Add(id, o, false);
        }

        private const string LIST_SEPARATOR = ";";

        private static string SerializeList(List<GlobalObjectId> list) => list.Join(LIST_SEPARATOR);

        private static List<GlobalObjectId> DeserializeList(string data) {
            if (data.IsNullOrEmpty())
                return new();

            return data.Split(LIST_SEPARATOR)
                .Select(idStr => GlobalObjectId.TryParse(idStr, out GlobalObjectId id) ? id : default)
                .Distinct()
                .ToList();
        }

        private static void Pin(GlobalObjectId id) {
            if (pinned.Contains(id))
                return;

            pinned.Add(id);
            Save();
        }

        private static void Unpin(GlobalObjectId id) {
            pinned.Remove(id);
            Save();
        }

        // [MenuItem("CONTEXT/QuickAccessPanel/Copy")]
        // private static void Copy() {}

    }

}
#endif