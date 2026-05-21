using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
#if DEVELOPMENT_TOOLS_EDITOR_NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif
#if UNITY_EDITOR && !SIMULATE_BUILD
using UnityEditor;
#endif

namespace DevelopmentTools.Editor.Extensions {

    public static class JsonExtensions {

#if DEVELOPMENT_TOOLS_EDITOR_NEWTONSOFT_JSON

        /// <inheritdoc cref="FromJson"/>
        public static void FromJsonOverwrite<T>(this Object objectToOverwrite, string json) {
            if (typeof(Object).IsAssignableFrom(typeof(T)))
                JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            else
                JsonConvert.PopulateObject(json, objectToOverwrite, SafeContractResolver.Settings);
        }

        /// <inheritdoc cref="FromJson"/>
        public static void FromJsonOverwrite<T>(this string json, Object objectToOverwrite) {
            if (typeof(Object).IsAssignableFrom(typeof(T)))
                JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            else
                JsonConvert.PopulateObject(json, objectToOverwrite, SafeContractResolver.Settings);
        }

        /// Automatically uses unity's JsonUtility for unity <see cref="Object"/>s and newtonsoft's JsonConvert for the rest
        [Pure]
        public static T FromJson<T>(this string json) => typeof(Object).IsAssignableFrom(typeof(T)) ? JsonUtility.FromJson<T>(json) : JsonConvert.DeserializeObject<T>(json, SafeContractResolver.Settings);

        /// <inheritdoc cref="FromJson"/>
        [Pure]
        public static string ToJson<T>(this T obj, bool prettyPrint = false) => typeof(Object).IsAssignableFrom(typeof(T)) ? JsonUtility.ToJson(obj, prettyPrint) : JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None, SafeContractResolver.Settings);

        private class SafeContractResolver : DefaultContractResolver {

            /// All that just because Unity throws on calling deprecated properties
            protected override JsonProperty CreateProperty(MemberInfo m, MemberSerialization s) {
                JsonProperty p = base.CreateProperty(m, s);
                p.ShouldSerialize = o => {
                    try {
                        _ = p.ValueProvider?.GetValue(o);
                        return true;
                    }
                    catch {
                        return false;
                    }
                };

                // include privates
                // FieldInfo f = m as FieldInfo;
                // if (f != null)
                //     p.Readable = p.Writable = true;

                return p;
            }

            public static JsonSerializerSettings Settings => new() {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver      = new SafeContractResolver(),
                // TypeNameHandling      = TypeNameHandling.All
            };

        }

#else
        /// <inheritdoc cref="FromJson"/>
        public static void FromJsonOverwrite(this Object objectToOverwrite, string json) => JsonUtility.FromJsonOverwrite(json, objectToOverwrite);

        /// <inheritdoc cref="FromJson"/>
        public static void FromJsonOverwrite(this string json, Object objectToOverwrite) => JsonUtility.FromJsonOverwrite(json, objectToOverwrite);

        /// Uses Unity's JsonUtility.<br/>Install Newtonsoft JSON to automatically use unity's JsonUtility for unity <see cref="Object"/>s and newtonsoft's JsonConvert for the rest
        [Pure]
        public static T FromJson<T>(this string json) => JsonUtility.FromJson<T>(json);

        /// <inheritdoc cref="FromJson"/>
        [Pure]
        public static string ToJson<T>(this T obj, bool prettyPrint = false) => JsonUtility.ToJson(obj, prettyPrint);

#endif

#if UNITY_EDITOR && !SIMULATE_BUILD
        /// <summary>
        /// Deep clones values from one Unity Object to another using Editor serialization.
        /// </summary>
        public static void CopyValues(Object source, Object destination) {
            if (source == null || destination == null) return;
            string json = EditorJsonUtility.ToJson(source);
            EditorJsonUtility.FromJsonOverwrite(json, destination);
        }

        /// <summary>
        /// Creates an identical string representation of a Unity Object,
        /// including private/internal fields not seen by JsonUtility.
        /// </summary>
        public static string ToEditorJson(this Object obj, bool prettyPrint = false) =>
            EditorJsonUtility.ToJson(obj, prettyPrint);

        /// <summary>
        /// Overwrites a Unity Object using Editor-fidelity JSON.
        /// </summary>
        public static void FromEditorJson(this Object obj, string json) =>
            EditorJsonUtility.FromJsonOverwrite(json, obj);
#endif

        /// <inheritdoc cref="FromJson"/>
        [Pure]
        public static T CloneJson<T>(this T obj) => obj.ToJson().FromJson<T>();

    }

}