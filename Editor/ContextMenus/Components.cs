using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.DevelopmentTools.Editor.Extensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
#if DEVELOPMENT_TOOLS_EDITOR_COMPONENT_NAMES
using ComponentNames;
#endif

namespace DevelopmentTools.DevelopmentTools.Editor.ContextMenus {

    public static class Components {

        [MenuItem("CONTEXT/Component/Paste Component Values (swap)", priority = 505)]
        private static void SwapWithClipboard(MenuCommand command) {
            if (!command.context.Is(out Component target)) return;

            // 1. Snapshot target state (Editor-fidelity)
            string targetData = target.ToEditorJson();

            // 2. Apply current clipboard to target
            ComponentUtility.PasteComponentValues(target);

            // 3. Move old target state to clipboard via hidden buffer
            GameObject temp = new() { hideFlags = HideFlags.HideAndDontSave };

            try {
                Component buffer = temp.AddComponent(target.GetType());
                buffer.FromEditorJson(targetData);
                ComponentUtility.CopyComponent(buffer);
            }
            finally {
                Object.DestroyImmediate(temp);
            }
        }

        private static readonly HashSet<Component> selectedComponents = new();
        private static          bool               transferNames;

        [MenuItem("GameObject/Transfer Components")]
        private static void SwapComponents() {
            GameObject g1 = (GameObject) Selection.objects[0];
            GameObject g2 = (GameObject) Selection.objects[1];

            List<Component> c1 = g1.GetComponents<Component>().Where(c => !c.Is<Transform>()).ToList();
            List<Component> c2 = g2.GetComponents<Component>().Where(c => !c.Is<Transform>()).ToList();

            GenericMenu menu = new();

            menu.AddDisabledItem(new(g1.name));
            foreach (Component c in c1)
                AddComponentItem(menu, $"1: {GetName(c)}", c);

            menu.AddSeparator(string.Empty);

            menu.AddDisabledItem(new(g2.name));
            foreach (Component c in c2)
                AddComponentItem(menu, $"2: {GetName(c)}", c);

            menu.AddSeparator(string.Empty);

            if (selectedComponents.Any()) {
                menu.AddItem(new("Transfer"), false, () => {
                    string undoName = $"Transfer {g1.name}[{c1.Count}] <-> {g2.name}[{c2.Count}]";

                    Undo.SetCurrentGroupName(undoName);
                    int undoGroupIndex = Undo.GetCurrentGroup();

                    Undo.RecordObject(g1, undoName);
                    Undo.RecordObject(g2, undoName);

                    foreach (Component c in selectedComponents) {
                        GameObject target = c.gameObject == g1 ? g2 : g1;
                        ComponentUtility.CopyComponent(c);
                        if (target.TryGetComponent(c.GetType(), out Component tc))
                            ComponentUtility.PasteComponentValues(tc);
                        else
                            ComponentUtility.PasteComponentAsNew(target);

                        Undo.DestroyObjectImmediate(c);
                    }

                    Undo.CollapseUndoOperations(undoGroupIndex);

                    selectedComponents.Clear();
                });
            }
            else {
                menu.AddItem(new("Swap All"), false, () => {
                    SwapAll(g1, g2);
                });

                menu.AddItem(new("Swap Names"), transferNames, () => {
                    transferNames = !transferNames;
                    SwapComponents();
                });
            }

            EditorApplication.delayCall += () => {
                menu.DropDown(new(500, 500, 100, 100));
            };

            return;

            string GetName(Component c) =>
#if DEVELOPMENT_TOOLS_EDITOR_COMPONENT_NAMES
                c.GetName()
                ??
#endif
                c.GetType().Name;
        }

        private static void AddComponentItem(GenericMenu menu, string label, Component c) {
            menu.AddItem(new(label), selectedComponents.Contains(c), () => {
                if (!selectedComponents.Add(c))
                    selectedComponents.Remove(c);

                SwapComponents();
            });
        }

        private static void SwapAll(GameObject g1, GameObject g2) {
            string undoName = $"Swap {g1.name} <=> {g2.name}";

            Undo.SetCurrentGroupName(undoName);
            int undoGroupIndex = Undo.GetCurrentGroup();

            Undo.RecordObject(g1, undoName);
            Undo.RecordObject(g2, undoName);

            GameObject tempG1 = new(g1.name);
            Undo.RegisterCreatedObjectUndo(tempG1, undoName);

            MoveComponents(g1, tempG1);
            MoveComponents(g2, g1);
            MoveComponents(tempG1, g2);

            if (transferNames) {
                transferNames = false;
                g1.name       = g2.name;
                g2.name       = tempG1.name;
            }

            Undo.DestroyObjectImmediate(tempG1);
            Undo.CollapseUndoOperations(undoGroupIndex);
        }

        private static void MoveComponents(GameObject source, GameObject target) {
            // 1. Handle Transform first (Copy Values)
            ComponentUtility.CopyComponent(source.transform);
            ComponentUtility.PasteComponentValues(target.transform);

            // 2. Handle everything else (Move)
            Component[] components = source.GetComponents<Component>()
                .Where(c => !c.Is<Transform>())
                .ToArray();

            foreach (Component c in components) {
                ComponentUtility.CopyComponent(c);
                ComponentUtility.PasteComponentAsNew(target);
                Undo.DestroyObjectImmediate(c);
            }
        }

        [MenuItem("GameObject/Swap Components", true)]
        private static bool SwapComponentValidator() {
            return Selection.objects.Length == 2 && Selection.objects[0].Is<GameObject>() && Selection.objects[1].Is<GameObject>();
        }

    }

}