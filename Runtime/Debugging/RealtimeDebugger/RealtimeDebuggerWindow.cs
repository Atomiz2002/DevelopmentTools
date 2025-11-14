#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using EditorSettings = DevelopmentTools.Debugging.Settings.EditorSettings;

namespace DevelopmentTools.Debugging.RealtimeDebugger {

    public class RealtimeDebuggerWindow : EditorWindow {

        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        [OdinSerialize]
        private Dictionary<MonoBehaviour, PropertyTree> cachedProperties = new();

        private static          Vector2                         scrollPosition;
        private static readonly Dictionary<GameObject, bool>    gameObjectFoldouts = new();
        private static readonly Dictionary<MonoBehaviour, bool> behavioursFoldouts = new();

        [MenuItem(EditorSettings.MenuGroupPath + "Realtime Debugger")]
        public static void TryShowWindow() => EditorSettings.TryShowWindow(GetWindow<RealtimeDebuggerWindow>(), "Realtime Debugger");

        private void OnEnable() {
            SceneManager.sceneLoaded -= CachePropertiesOnSceneLoad;
            SceneManager.sceneLoaded += CachePropertiesOnSceneLoad;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= CachePropertiesOnSceneLoad;
        }

        private void CachePropertiesOnSceneLoad(Scene scene, LoadSceneMode loadSceneMode) => CacheProperties();

        protected void OnInspectorUpdate() => Repaint();

        protected void OnGUI() {
            if (GUILayout.Button("Cache Properties"))
                CacheProperties();

            DrawCachedProperties();
        }

        private void DrawCachedProperties() {
            GUIStyle gameObjectNameStyle = new(EditorStyles.foldout) { fontSize = 14, fontStyle = FontStyle.Bold };
            GUIStyle behaviourNameStyle  = new(EditorStyles.foldout) { fontSize = 12, fontStyle = FontStyle.Bold };

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach ((MonoBehaviour behaviour, PropertyTree propertyTree) in cachedProperties.Reverse()) {
                if (!behaviour)
                    continue;

                behaviour.GetType().TryGet(out RealtimeDebugAttribute rootAttribute);

                if (FailedDebugCondition(rootAttribute, propertyTree.RootProperty))
                    continue;

                Dictionary<InspectorProperty, RealtimeDebugAttribute> properties = new();

                foreach (InspectorProperty property in propertyTree.EnumerateTree(false, true).Skip(1)) {
                    RealtimeDebugAttribute propAttribute = property.Attributes.GetAttribute<RealtimeDebugAttribute>();

                    if (propAttribute == null)
                        continue;

                    if (FailedDebugCondition(propAttribute, property))
                        continue;

                    properties.Add(property, propAttribute);
                }

                if (!properties.Any())
                    continue; // dont draw if no properties meet their condition

                GameObject gameObject = behaviour.gameObject;

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();

                gameObjectFoldouts.TryAdd(gameObject, true);
                gameObjectFoldouts[gameObject] = EditorGUILayout.Foldout(gameObjectFoldouts[gameObject], gameObject.name, true, gameObjectNameStyle);

                if (GUILayout.Button("Select", GUILayout.ExpandWidth(false))) gameObject.SelectAndPing();
                if (GUILayout.Button("Ping", GUILayout.ExpandWidth(false))) gameObject.Ping();

                EditorGUILayout.EndHorizontal();

                if (gameObjectFoldouts[gameObject]) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.BeginHorizontal();

                    behavioursFoldouts.TryAdd(behaviour, true);
                    behavioursFoldouts[behaviour] = EditorGUILayout.Foldout(behavioursFoldouts[behaviour], behaviour.GetType().Name, true, behaviourNameStyle);

                    if (GUILayout.Button("Open Script", GUILayout.ExpandWidth(false)))
                        OpenScript(behaviour);

                    EditorGUILayout.EndHorizontal();

                    if (behavioursFoldouts[behaviour]) {
                        propertyTree.UpdateTree();

                        propertyTree.BeginDraw(true);

                        TryDrawLabel(rootAttribute, propertyTree.RootProperty);

                        foreach ((InspectorProperty property, RealtimeDebugAttribute propAttribute) in properties) {
                            TryDrawLabel(propAttribute, property);

                            EditorGUILayout.BeginHorizontal();

                            // if (!propAttribute.ValueGetter.IsNullOrEmpty()) {
                            //     ValueResolver<string> labelResolver = ValueResolver.Get<string>(property, propAttribute.ValueGetter);
                            //     if (!labelResolver.HasError)
                            //         propAttribute.Label = labelResolver.GetValue();
                            //     else
                            //         labelResolver.DrawError();
                            // }

                            // bool ugh = false;
                            if (rootAttribute is { AsString: true } || propAttribute.AsString)
                                foreach (InspectorProperty child in property.Children) {
                                    string str = child.ValueEntry.WeakSmartValue.SafeString();
                                    EditorGUILayout.LabelField(str, new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true });

                                    // if (!ugh) {
                                    //     ugh = true;
                                    //     EditorGUILayout.EndHorizontal();
                                    // }
                                }
                            else
                                property.Draw();

                            // if (ugh)
                            //     EditorGUILayout.BeginHorizontal();

                            if (GUILayout.Button(">_", GUILayout.ExpandWidth(false))) OpenScript(behaviour, property.Name);
                            EditorGUILayout.EndHorizontal();
                        }

                        propertyTree.EndDraw();
                    }

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private static bool FailedDebugCondition(RealtimeDebugAttribute attribute, InspectorProperty property) {
            if (attribute == null) return false;
            if (attribute.DebugCondition.IsNullOrEmpty()) return false;

            ValueResolver<bool> result = ValueResolver.Get<bool>(property, attribute.DebugCondition);

            if (result.HasError) {
                result.DrawError();
                return true;
            }

            return !result.GetValue();
        }

        private static void TryDrawLabel(RealtimeDebugAttribute attribute, InspectorProperty property) {
            if (attribute == null)
                return;

            if (!attribute.LabelGetter.IsNullOrEmpty()) {
                ValueResolver<string> labelResolver = ValueResolver.Get<string>(property, attribute.LabelGetter);

                if (!labelResolver.HasError)
                    attribute.Label = labelResolver.GetValue();
                else
                    labelResolver.DrawError();
            }

            if (!attribute.Label.IsNullOrEmpty()) {
                EditorGUILayout.LabelField(attribute.Label,
                    new GUIStyle(EditorStyles.boldLabel) {
                        richText  = true,
                        alignment = TextAnchor.MiddleCenter
                    });

                GUILayout.Space(4);
            }
        }

        private void CacheProperties() {
            cachedProperties = cachedProperties.ExistingKeys();

            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                bool         debugBehaviour = behaviour.GetType().IsDefined<RealtimeDebugAttribute>();
                MemberInfo[] members        = behaviour.GetType().GetMembers(Flags);

                if (!debugBehaviour && !members.Any(m => m.TryGet(out RealtimeDebugAttribute _)))
                    continue;

                cachedProperties.TryAdd(behaviour, PropertyTree.Create(behaviour));
            }
        }

        private static void OpenScript(MonoBehaviour script, string target = null) {
            MonoScript monoScript         = MonoScript.FromMonoBehaviour(script);
            string     targetMatchPattern = @$"{target ?? monoScript.name}(?!\w)";

            string[] lines      = monoScript.text.Split('\n');
            int      targetLine = lines.IndexOf(lines.First(line => Regex.IsMatch(line, targetMatchPattern))) + 1;

            AssetDatabase.OpenAsset(monoScript, targetLine);
        }

    }

}
#endif