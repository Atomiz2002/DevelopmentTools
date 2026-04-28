using JetBrains.Annotations;
using UnityEngine;

#if DEVELOPMENT_TOOLS_EDITOR_NEWTONSOFT_JSON
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif

namespace DevelopmentTools.Editor.Extensions {

    public static class JsonExtensions {

        public static void FromJsonOverwrite(this Object objectToOverwrite, string json) => JsonUtility.FromJsonOverwrite(json, objectToOverwrite);

        public static void FromJsonOverwrite(this string json, Object objectToOverwrite) => JsonUtility.FromJsonOverwrite(json, objectToOverwrite);

#if DEVELOPMENT_TOOLS_EDITOR_NEWTONSOFT_JSON

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
                ContractResolver = new SafeContractResolver(),
                // TypeNameHandling      = TypeNameHandling.All
            };

        }

#else

        /// Uses Unity's JsonUtility.<br/>Install Newtonsoft JSON to automatically use unity's JsonUtility for unity <see cref="Object"/>s and newtonsoft's JsonConvert for the rest
        [Pure]
        public static T FromJson<T>(this string json) => JsonUtility.FromJson<T>(json);

        /// <inheritdoc cref="FromJson"/>
        [Pure]
        public static string ToJson<T>(this T obj, bool prettyPrint = false) => JsonUtility.ToJson(obj, prettyPrint);

#endif

        /// <inheritdoc cref="FromJson"/>
        [Pure]
        public static T CloneJson<T>(this T obj) => obj.ToJson().FromJson<T>();

    }

}