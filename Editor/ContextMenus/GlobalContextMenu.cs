using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Editor.AttributeDrawers;
using DevelopmentTools.Editor.Extensions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevelopmentTools.Editor.ContextMenus {

    [InitializeOnLoad]
    public static class GlobalContextMenu {

        private const string PrefsKey  = "GlobalContextMenu_Groups";
        private const string delimiter = "~~~";

        private static readonly Dictionary<string, HashSet<string>> groups = new();

        static GlobalContextMenu() {
            Load();
            EditorApplication.contextualPropertyMenu  += OnContextMenu;
            AssemblyReloadEvents.beforeAssemblyReload += Save;
        }

        private static void OnContextMenu(GenericMenu menu, SerializedProperty property) {
            object         value = property.boxedValue;
            GlobalObjectId gid   = property.serializedObject.targetObject.GlobalId();
            string         id    = gid + delimiter + property.propertyPath + delimiter + property.type;

            switch (value) {
                case Sprite sprite:   menu.AddItem(new("🔍 Preview Sprite"), false, () => PreviewTexture2DWindow.Create(sprite.ToTexture2D())); break;
                case Texture texture: menu.AddItem(new("🔍 Preview Texture"), false, () => PreviewTexture2DWindow.Create(texture)); break;
            }

            menu.AddItem(new("🔗 Modify"), false, () => {
                EditorWindow.GetWindow<LinkedGroupsEditorWindow>().Show();
            });

            menu.AddItem(new("🔗 Apply (in all scenes)"), false, () => ApplyToLinkedGroups(false));
            menu.AddItem(new("🔗 Apply (in open scenes)"), false, () => ApplyToLinkedGroups(true));

            foreach ((string group, HashSet<string> entries) in groups) {
                bool on = entries.Contains(id);

                if (entries.Count == 0) {
                    menu.AddItem(new($"🔗 {group} ({entries.Count})"), on, () => {
                        if (on)
                            entries.Remove(id);
                        else
                            entries.Add(id);
                    });

                    continue;
                }

                string type = entries.First().Split(delimiter)[^1].Replace("<", string.Empty).Replace(">", string.Empty).Replace("Pptr", string.Empty);

                if (entries.First().EndsWith(property.type)) {
                    menu.AddItem(new($"🔗 {group} ({entries.Count} - {type})"), on, () => {
                        if (on)
                            entries.Remove(id);
                        else
                            entries.Add(id);
                    });
                }
                else {
                    menu.AddDisabledItem(new($"🔗 {group} ({entries.Count} - {type})"));
                }
            }

            return;

            void ApplyToLinkedGroups(bool inOpenSceneOnly) {
                Object                    obj          = gid.ToObject();
                Dictionary<string, Scene> scenesToOpen = new Dictionary<string, Scene>();

                foreach (HashSet<string> entries in groups.Values) {
                    if (!entries.Contains(id))
                        continue;

                    foreach (string entry in entries) {
                        string[] info = entry.Split(delimiter);

                        if (!GlobalObjectId.TryParse(info[0], out GlobalObjectId goid))
                            continue;

                        Object o = goid.ToObject();
                        if (o == null) continue;

                        string scenePath = o.Is(out Component comp) ? comp.gameObject.scene.path : null;
                        if (string.IsNullOrEmpty(scenePath)) continue;

                        if (!inOpenSceneOnly && !scenesToOpen.ContainsKey(scenePath)) {
                            scenesToOpen[scenePath] = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                        }

                        Undo.RecordObject(o, "Apply to Linked Groups");
                        SerializedObject   so = new(o);
                        SerializedProperty sp = so.FindProperty(info[1]);

                        if (sp != null) {
                            sp.boxedValue = value;
                            so.ApplyModifiedProperties();
                        }
                    }
                }

                foreach (var scene in scenesToOpen.Values) {
                    EditorSceneManager.SaveScene(scene);
                    EditorSceneManager.CloseScene(scene, true);
                }

                obj.Select();
            }
        }

        private static void Save() {
            EditorPrefs.SetString(PrefsKey, groups.ToJson());
        }

        private static void Load() {
            string json = EditorPrefs.GetString(PrefsKey);

            if (string.IsNullOrEmpty(json))
                return;

            groups.FromJsonOverwrite(json);
        }

        private class LinkedGroupsEditorWindow : OdinEditorWindow {

            [ListDrawerSettings(DefaultExpandedState = true, ShowFoldout = false)]
            public List<string> LinkedGroups = new();

            protected override void OnEnable() {
                base.OnEnable();
                name         = "Linked Groups";
                LinkedGroups = groups.Keys.ToList();
            }

            [Button]
            public void Ok() {
                groups.RemoveKeys(k => !LinkedGroups.Contains(k));

                foreach (string group in LinkedGroups)
                    groups.TryAdd(group, new());

                Save();
                Close();
            }

        }

    }

}