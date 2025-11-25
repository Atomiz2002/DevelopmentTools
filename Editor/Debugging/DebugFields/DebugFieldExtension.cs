#if DEVELOPMENT_TOOLS_ODIN_INSPECTOR
using System;
using DevelopmentEssentials.Extensions.CS;
using JetBrains.Annotations;
using UnityEngine;

namespace DevelopmentTools.Editor.Debugging.DebugFields {

    // todo: does it work with nulls???
    public static class DebugFieldExtension {

        public static Sprite DebugField(this Sprite sprite, object fieldName) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            DebugFields.AddDebugField(fieldName.SafeString(), string.Empty, sprite.texture);
#endif
            return sprite;
        }

        public static Texture2D DebugField(this Texture2D texture, object fieldName) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            DebugFields.AddDebugField(fieldName.SafeString(), string.Empty, texture);
#endif
            return texture;
        }

        public static T DebugField<T>(this T t, object fieldName, Texture2D icon = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            DebugFields.AddDebugField(fieldName.SafeString(), t.EnsureString("\n"), icon);
#endif
            return t;
        }

        public static T DebugField<T, T2>(this T t, object fieldName, [NotNull] Func<T, T2> func, Texture2D icon = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            try {
                DebugFields.AddDebugField(fieldName.SafeString(), func(t)?.EnsureString("\n"), icon, new(2, true));
            }
            catch (Exception) {
                DebugFields.AddDebugField($"{fieldName.SafeString()} (Error)", t?.EnsureString("\n"), icon);
            }
#endif
            return t;
        }

    }

}
#endif