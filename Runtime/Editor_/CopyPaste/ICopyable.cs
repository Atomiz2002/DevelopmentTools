using UnityEditor;
using UnityEngine;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace DevelopmentTools.Editor_.CopyPaste {

    public interface ICopyable {

        public void OnPaste();

    }

    public static class CopyPasteExtensions {

        // Must use JsonUtility/EditorJsonUtility for UnityEngine.Object types.
        // Use EditorJsonUtility in Editor for better serialization of Object types.
        public static void CopyToClipboard<T>(this T t) {
            if (t is Object obj) {
#if UNITY_EDITOR
                EditorGUIUtility.systemCopyBuffer = EditorJsonUtility.ToJson(obj, true);
#else
                EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(obj, true);
#endif
            }
            else {
#if NEWTONSOFT_JSON
                EditorGUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(t, Formatting.Indented);
#else
                // Fallback to JsonUtility for structs/classes if Newtonsoft not present,
                // but this will only work if T is Serializable and simple.
                EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(t);
#endif
            }
        }

        public static void PasteFromClipboard<T>(this T t) {
            if (t is Object obj) {
                // Use JsonUtility.FromJsonOverwrite for Object types for best results on paste.
                // EditorJsonUtility doesn't have FromJsonOverwrite.
                JsonUtility.FromJsonOverwrite(EditorGUIUtility.systemCopyBuffer, obj);
            }
            else {
#if NEWTONSOFT_JSON
                // Use Newtonsoft for general classes/structs (better handling).
                JsonConvert.PopulateObject(EditorGUIUtility.systemCopyBuffer, t);
#else
                // Fallback: This requires T to be a reference type (class)
                // and a separate instance if JsonUtility is to overwrite properties,
                // but it's often simpler to just use FromJsonOverwrite on the target instance
                // if JsonUtility is the only option, which PopuplateObject does not do.
                // The most reliable approach without Newtonsoft is to require the user
                // to use a separate method for non-Object types or accept limited functionality.
                // For a direct rewrite, we use JsonUtility.FromJsonOverwrite as a suboptimal fallback
                // for non-Object reference types if Newtonsoft is missing.
                JsonUtility.FromJsonOverwrite(EditorGUIUtility.systemCopyBuffer, t);
#endif
            }

            if (t is ICopyable copyable)
                copyable.OnPaste();
        }

    }

}