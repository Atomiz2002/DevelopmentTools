#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Settings;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevelopmentTools.DevelopmentTools.Editor.Debugging.RealtimeDebugger {

    public class RealtimeDebuggerWindow : EditorWindow {

        private const  BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static bool         autoOpenedWindowOnce;

        [OdinSerialize]
        private Dictionary<object, PropertyTree> cachedProperties = new();

        private static          Vector2                  scrollPosition;
        private static readonly Dictionary<object, bool> gameObjectFoldouts         = new();
        private static readonly Dictionary<object, bool> behavioursFoldouts         = new();
        private static readonly Dictionary<object, bool> behavioursPrivatesFoldouts = new();

        [MenuItem(EngineSettings.MenuGroupPath + "Realtime Debugger")]
        public static void TryShowWindow() {
            // TSHEditorSettings.TryShowWindow(GetWindow<RealtimeDebuggerWindow>(), "Realtime Debugger");
            GetWindow<RealtimeDebuggerWindow>().name = "Realtime Debugger";
        }

        public static void CacheProperty(object obj) => GetWindow<RealtimeDebuggerWindow>().cachedProperties.TryAdd(obj, PropertyTree.Create(obj));

        private void OnEnable() {
            SceneManager.sceneLoaded -= CachePropertiesOnSceneLoad;
            SceneManager.sceneLoaded += CachePropertiesOnSceneLoad;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= CachePropertiesOnSceneLoad;
        }

        private void CachePropertiesOnSceneLoad(Scene scene, LoadSceneMode loadSceneMode) {
            CacheProperties();

            if (autoOpenedWindowOnce || !cachedProperties.Any()) return;

            TryShowWindow();
            autoOpenedWindowOnce = true;
        }

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

            foreach ((object obj, PropertyTree propertyTree) in cachedProperties.Where(kvp => kvp.Key != null)) {
                Object o           = obj as Object;
                bool   isDestroyed = obj is Object && !o;

                if (!o && obj == null)
                    continue;

                obj.GetType().TryGet(out RealtimeDebugAttribute rootAttribute);

                if (FailedDebugCondition(rootAttribute, propertyTree.RootProperty))
                    continue;

                Dictionary<InspectorProperty, RealtimeDebugAttribute> properties = new();

                if (rootAttribute == null) {
                    foreach (InspectorProperty property in propertyTree.EnumerateTree()) {
                        RealtimeDebugAttribute propAttribute = property.Attributes.GetAttribute<RealtimeDebugAttribute>();

                        if (propAttribute == null)
                            continue;

                        if (FailedDebugCondition(propAttribute, property))
                            continue;

                        properties.Add(property, propAttribute);
                    }

                    if (!properties.Any())
                        continue;
                }
                else {
                    properties.Add(propertyTree.RootProperty, rootAttribute);
                }

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();

                gameObjectFoldouts.TryAdd(obj, true);
                gameObjectFoldouts[obj] = EditorGUILayout.Foldout(gameObjectFoldouts[obj], o ? o.name : obj.GetHashCode().SafeString(), true, gameObjectNameStyle);

                if (o && GUILayout.Button("Select", GUILayout.ExpandWidth(false))) o.SelectAndPing();
                if (o && GUILayout.Button("Ping", GUILayout.ExpandWidth(false))) o.Ping();

                EditorGUILayout.EndHorizontal();

                if (gameObjectFoldouts[obj]) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.BeginHorizontal();

                    behavioursFoldouts.TryAdd(obj, true);
                    behavioursFoldouts[obj] = EditorGUILayout.Foldout(behavioursFoldouts[obj], obj.GetType().Name, true, behaviourNameStyle);

                    if (GUILayout.Button("Open Script", GUILayout.ExpandWidth(false)))
                        OpenScript(obj);

                    EditorGUILayout.EndHorizontal();

                    if (behavioursFoldouts[obj] && !isDestroyed) {
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
                            if (rootAttribute is { AsString: true } || propAttribute.AsString) {
                                foreach (InspectorProperty child in property.Children) {
                                    string str = child.ValueEntry.WeakSmartValue.SafeString();
                                    InfoContainer.ModifyInfo(typeof(RealtimeDebuggerWindow), ref str);
                                    EditorGUILayout.LabelField(str, new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true });

                                    // if (!ugh) {
                                    //     ugh = true;
                                    //     EditorGUILayout.EndHorizontal();
                                    // }
                                }
                            }
                            else {
                                property.Draw();
                            }

                            // if (ugh)
                            //     EditorGUILayout.BeginHorizontal();

                            if (GUILayout.Button(">_", GUILayout.ExpandWidth(false)))
                                OpenScript(obj, property.Name);

                            EditorGUILayout.EndHorizontal();
                        }

                        propertyTree.EndDraw();

                        if (rootAttribute != null) {
                            behavioursPrivatesFoldouts.TryAdd(obj, false);
                            behavioursPrivatesFoldouts[obj] = EditorGUILayout.BeginFoldoutHeaderGroup(behavioursPrivatesFoldouts[obj], "Private/Hidden");

                            if (behavioursPrivatesFoldouts[obj]) {
                                foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                             .Where(f => !properties.Keys.Any(p => p.IsTreeRoot ? p.Tree.EnumerateTree().Any(tp => tp.Name == f.Name) : p.Name == f.Name))) {
                                    if (o) {
                                        SerializedObject so = new(o);
                                        so.Update();
                                        SerializedProperty prop = so.FindProperty(field.Name);
                                        if (prop != null)
                                            EditorGUILayout.PropertyField(prop);
                                        else
                                            EditorGUILayout.LabelField(field.Name, field.GetValue(o).EnsureString());

                                        so.ApplyModifiedProperties();
                                    }
                                    else {
                                        EditorGUILayout.LabelField(field.Name, field.GetValue(obj).EnsureString());
                                    }
                                }
                            }

                            EditorGUILayout.EndFoldoutHeaderGroup();
                        }
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

            InfoContainer.ModifyInfo(typeof(RealtimeDebuggerWindow), ref attribute.Label);

            if (!attribute.Label.IsNullOrEmpty()) {
                EditorGUILayout.LabelField(attribute.Label,
                    new GUIStyle(EditorStyles.boldLabel) {
                        richText  = true,
                        alignment = TextAnchor.MiddleCenter
                    });

                SirenixEditorGUI.HorizontalLineSeparator(InfoContainer.ExtractInfo<Color>(typeof(RealtimeDebuggerWindow), attribute.Label).FirstOrDefault());
                GUILayout.Space(4);
            }
        }

        private void CacheProperties() {
            cachedProperties = cachedProperties.NonNullKeys();

            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                bool         debugBehaviour = behaviour.GetType().IsDefined<RealtimeDebugAttribute>(true);
                MemberInfo[] members        = behaviour.GetType().GetMembers(Flags);

                if (!debugBehaviour && !members.Any(m => m.TryGet(out RealtimeDebugAttribute _)))
                    continue;

                cachedProperties.TryAdd(behaviour, PropertyTree.Create(behaviour));
            }

            cachedProperties = cachedProperties.Reverse().ToDictionary();
        }

        // todo can return incorrect line
        private static void OpenScript(object obj, string target = null) {
            MonoScript monoScript;

            switch (obj) {
                case MonoBehaviour mb:    monoScript = MonoScript.FromMonoBehaviour(mb); break;
                case ScriptableObject so: monoScript = MonoScript.FromScriptableObject(so); break;
                default:                  return;
            }

            string targetMatchPattern = @$"{target ?? monoScript.name}(?!\w)";

            string[] lines      = monoScript.text.Split('\n');
            int      targetLine = lines.IndexOf(lines.First(line => Regex.IsMatch(line, targetMatchPattern))) + 1;

            AssetDatabase.OpenAsset(monoScript, targetLine);
        }

    }

}
#endif