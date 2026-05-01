using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using JetBrains.Annotations;

namespace DevelopmentTools {

    public static class InfoContainer {

        // using object instead of List<Func<object, object((T))>> for performance reasons
        private static readonly Dictionary<object, Dictionary<Type, object>> registeredExtractors = new();
        private static readonly Dictionary<object, Dictionary<Type, object>> registeredModifiers  = new();

        /// <inheritdoc cref="RegisterInfo"/>
        public static void RegisterInfoExtractor<T, TResult>(object key, Func<T, TResult> extractor) => RegisterInfo(key, extractor, registeredExtractors);

        /// <inheritdoc cref="RegisterInfo"/>
        public static void RegisterInfoModifier<T>(object key, Func<T, T> modifier) => RegisterInfo(key, modifier, registeredModifiers);

        /// <param name="key_s">can also be a collection</param>
        private static void RegisterInfo<T, TResult>(object key_s, Func<T, TResult> func, Dictionary<object, Dictionary<Type, object>> registers) {
            if (key_s is IEnumerable enumerable and not string) {
                object[] collection = enumerable.Cast<object>().ToArray();

                foreach (object k in collection)
                    Register(k);
            }
            else
                Register(key_s);

            return;

            void Register(object key) {
                if (!registers.TryGetValue(key, out Dictionary<Type, object> keyed)) {
                    keyed = new();
                    registers.Add(key, keyed);
                }

                if (!keyed.TryGetValue(typeof(TResult), out object funcs)) {
                    funcs = new List<Func<T, TResult>>();
                    keyed.Add(typeof(TResult), funcs);
                }

                ((List<Func<T, TResult>>) funcs).Add(func);
            }
        }

        public static void ExtractAndModifyInfo<T>(object key, ref string input, ref List<T> output, bool add = true) {
            ExtractInfo(key, input, ref output, add);
            ModifyInfo(key, ref input);
        }

        public static void ExtractInfo<T>(object key, object input, ref List<T> output, bool add = true) {
            if (add)
                output.AddRange(ExtractInfo<T>(key, input));
            else
                output = ExtractInfo<T>(key, input).ToList();
        }

        [Pure]
        public static IEnumerable<T> ExtractInfo<T>(object key, object input) {
            if (!registeredModifiers.TryGetValue(key, out Dictionary<Type, object> keyedExtractors))
                yield break;

            if (!keyedExtractors.TryGetValue(typeof(T), out object extractors))
                yield break;

            foreach (Func<object, T> extractor in (List<Func<object, T>>) extractors)
                yield return extractor.SafeInvoke(input);
        }

        public static void ModifyInfo<T>(object key, ref T input) => input = ModifyInfo(key, input);

        [Pure]
        public static T ModifyInfo<T>(object key, T input) {
            if (!registeredModifiers.TryGetValue(key, out Dictionary<Type, object> keyedModifiers))
                return input;

            if (!keyedModifiers.TryGetValue(typeof(T), out object modifiers))
                return input;

            foreach (Func<object, T> modifier in (List<Func<object, T>>) modifiers)
                input = modifier.SafeInvoke(input);

            return input;
        }

    }

}