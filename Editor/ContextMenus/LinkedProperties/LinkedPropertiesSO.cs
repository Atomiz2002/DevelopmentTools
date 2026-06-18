using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Editor.ContextMenus {

    public class LinkedPropertiesSO : ScriptableObject {

        public List<LinkedPropertyGroup> LinkedGroups = new();

        [InitializeOnLoadMethod]
        private static void Initialize() => EditorApplication.contextualPropertyMenu += OnContextMenu;

        private static LinkedPropertiesSO i;
        public static  LinkedPropertiesSO I => DevelopmentEssentials.EditorHelper.InstanceSO(ref i);

        private static void OnContextMenu(GenericMenu menu, SerializedProperty prop) {
            Object target = prop.serializedObject.targetObject;
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (target == null || PrefabUtility.IsPartOfPrefabAsset(target) || prefabStage != null)
                return;

            I.LinkedGroups.RemoveAll(g => g.Name.IsNullOrWhiteSpace());
            HashSet<string> groups = new();
            foreach (LinkedPropertyGroup group in I.LinkedGroups)
                if (!groups.Add(group.Name))
                    group.Name += "2";

            GlobalObjectId globalId        = target.GlobalId();
            LinkedProperty linkedProp      = new(prop);
            object         linkedPropValue = linkedProp.GetPropertyValue(prop, out bool isArray);

            if (I.LinkedGroups.Any(g => g.Contains(linkedProp))) {
                menu.AddItem(new("🔗 Pull ▼"), false, () => PullValue(globalId, linkedProp, isArray));
                menu.AddItem(new("🔗 Push ▲ (in open scenes)"), false, () => PushValueInOpenScenes(globalId, linkedProp, linkedPropValue, isArray));
                menu.AddItem(new("🔗 ▲ Push (in all scenes)"), false, () => PushValueInAllScenes(globalId, linkedProp, linkedPropValue, isArray));
            }

            menu.AddItem(new("🔗 Edit"), false, () => I.Select());

            if (I.LinkedGroups.Any())
                menu.AddSeparator(string.Empty);

            foreach (LinkedPropertyGroup group in I.LinkedGroups) {
                if (group.LinkedProperties.Count == 0) {
                    menu.AddItem(new($"🔗 {group.Name} (empty)"), false, () => AddToGroup(group, linkedProp));

                    continue;
                }

                bool on = group.Contains(linkedProp);

                string type = group.LinkedProperties.First().DisplayType;

                if (group.LinkedProperties.First().Type == prop.type) {
                    menu.AddItem(new($"🔗 {group.Name} ({group.LinkedProperties.Count} - {type})"), on, () => {
                        if (on)
                            group.Remove(linkedProp);
                        else if (!group.Contains(linkedProp))
                            AddToGroup(group, linkedProp);
                    });
                }
                else {
                    menu.AddDisabledItem(new($"🔗 {group.Name} ({group.LinkedProperties.Count} - {type})"));
                }
            }
        }

        private static void PushValueInOpenScenes(GlobalObjectId gid, LinkedProperty lid, object value, bool isArray) => PushValue(true, gid, lid, value, isArray);
        private static void PushValueInAllScenes(GlobalObjectId gid, LinkedProperty lid, object value, bool isArray)  => PushValue(false, gid, lid, value, isArray);

        private static void PushValue(bool inOpenScenesOnly, GlobalObjectId gid, LinkedProperty lid, object value, bool isArray) {
            Object                    obj          = gid.ToObject();
            Dictionary<string, Scene> scenesToOpen = new();

            foreach (LinkedPropertyGroup group in I.LinkedGroups.Where(group => group.Contains(lid))) {
                foreach (LinkedProperty entry in group.LinkedProperties) {
                    Object o = entry.GlobalId.ToObject();

                    if (!o)
                        continue;

                    string scenePath = o.Is(out Component comp) ? comp.gameObject.scene.path : null;

                    if (scenePath == null)
                        continue;

                    if (!inOpenScenesOnly && !scenesToOpen.ContainsKey(scenePath))
                        scenesToOpen[scenePath] = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                    Undo.RecordObject(o, $"🔗 Push {(inOpenScenesOnly ? "(in open scenes)" : "(in all scenes)")}");
                    entry.SetPropertyValue(value, isArray, o);
                }

                break;
            }

            foreach (Scene scene in scenesToOpen.Values) {
                EditorSceneManager.SaveScene(scene);
                EditorSceneManager.CloseScene(scene, true);
            }

            obj.Select();
        }

        private static void PullValue(GlobalObjectId gid, LinkedProperty lid, bool isArray) {
            Object o = gid.ToObject();

            if (o == null)
                return;

            object value = null;

            LinkedPropertyGroup group               = I.LinkedGroups.FirstOrDefault(group => group.Contains(lid));
            LinkedProperty      firstDifferentEntry = group?.LinkedProperties.FirstOrDefault(entry => !entry.GlobalId.Equals(gid));

            if (firstDifferentEntry != null)
                value = firstDifferentEntry.GetPropertyValue();

            Undo.RecordObject(o, "🔗 Pull");
            lid.SetPropertyValue(value, isArray, o);

            o.Select();
        }

        private static void AddToGroup(LinkedPropertyGroup group, LinkedProperty linkedProp) {
            foreach (LinkedPropertyGroup g in I.LinkedGroups)
                g.Remove(linkedProp);

            group.Add(linkedProp);
        }

    }

}