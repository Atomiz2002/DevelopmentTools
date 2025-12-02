#if DEVELOPMENT_TOOLS_NEWTONSOFT_JSON
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#else
using UnityEngine;
#endif
using JetBrains.Annotations;

namespace DevelopmentTools.Editor.Extensions {

    public static class JsonExtensions {

#if DEVELOPMENT_TOOLS_NEWTONSOFT_JSON

        #region NEWTONSOFT JSON

        [Pure]
        public static T FromJSON<T>(this string json) => JsonConvert.DeserializeObject<T>(json);

        [Pure]
        public static string ToJSON<T>(this T obj, bool pretty = false) => JsonConvert.SerializeObject(obj, pretty ? Formatting.Indented : Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, ContractResolver = new SafeContractResolver() });

        [Pure]
        public static T CloneJSON<T>(this T obj) => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));

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

                return p;
            }

        }

        #endregion

#else
        #region JSON UTILITY

        [Pure]
        public static T FromJSON<T>(this string json) => JsonUtility.FromJson<T>(json);

        [Pure]
        public static string ToJSON<T>(this T obj, bool pretty = false) => JsonUtility.ToJson(obj, pretty);

        [Pure]
        public static T CloneJSON<T>(this T obj) => JsonUtility.FromJson<T>(JsonUtility.ToJson(obj));

        #endregion

#endif

    }

}
