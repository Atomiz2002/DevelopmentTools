#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using System;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevelopmentTools.Editor.Debugging.DebugFields {

    // todo: does it work with nulls???
    public static class DebugFieldExtension {

        public static Sprite DebugField(this Sprite sprite, object fieldName = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            DebugFields.AddDebugField(fieldName.SafeString(sprite.n()?.name), string.Empty, sprite.texture);
#endif
            return sprite;
        }

        public static Texture DebugField(this Texture texture, object fieldName = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            DebugFields.AddDebugField(fieldName.SafeString(texture.n()?.name), string.Empty, texture.Read());
#endif
            return texture;
        }

        public static Texture2D DebugField(this Texture2D texture, object fieldName = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            DebugFields.AddDebugField(fieldName.SafeString(texture.n()?.name), string.Empty, texture);
#endif
            return texture;
        }

        public static T DebugField<T>(this T t, object fieldName = null, Texture2D icon = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            DebugFields.AddDebugField(fieldName.SafeString(t is Object o && o ? o.name : null), t.EnsureString(separator: "\n"), icon);
#endif
            return t;
        }

        public static T DebugField<T, T2>(this T t, [CanBeNull] object fieldName, [NotNull] Func<T, T2> func, Texture2D icon = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            try {
                DebugFields.AddDebugField(fieldName.SafeString(t is Object o && o ? o.name : null), func(t)?.EnsureString(separator: "\n"), icon, new(2, true));
            }
            catch (Exception) {
                DebugFields.AddDebugField($"{fieldName.SafeString(t is Object o && o ? o.name : null)} (Error)", t?.EnsureString(separator: "\n"), icon);
            }
#endif
            return t;
        }

    }

}
#endif