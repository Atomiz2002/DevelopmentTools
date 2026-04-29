#if DEVELOPMENT_TOOLS_RUNTIME_NEWTONSOFT_JSON && UNITY_EDITOR && !SIMULATE_BUILD
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
#endif

namespace DevelopmentTools {

    public interface ICopyable {

        public void OnPaste();

    }

#if DEVELOPMENT_TOOLS_RUNTIME_NEWTONSOFT_JSON && UNITY_EDITOR && !SIMULATE_BUILD
    public static class CopyPasteExtensions {

        public static void CopyObjToClipboard<T>(this T t) =>
            EditorGUIUtility.systemCopyBuffer = t is Object obj
                ? JsonUtility.ToJson(obj, true)
                : JsonConvert.SerializeObject(t, Formatting.Indented);

        public static void PasteObjFromClipboard<T>(this T t) {
            if (t is Object obj)
                JsonUtility.FromJsonOverwrite(EditorGUIUtility.systemCopyBuffer, obj);
            else
                JsonConvert.PopulateObject(EditorGUIUtility.systemCopyBuffer, t);

            if (t is ICopyable copyable)
                copyable.OnPaste();
        }

    }
#endif

}