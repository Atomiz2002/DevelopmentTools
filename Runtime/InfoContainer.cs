using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DevelopmentEssentials.Extensions.CS;
using JetBrains.Annotations;

namespace DevelopmentTools {

    public static class InfoContainer {

        // using object instead of List<Func<object, object((T))>> for performance reasons
        private static readonly Dictionary<object, Dictionary<Type, object>> registeredExtractors = new();
        private static readonly Dictionary<object, Dictionary<Type, object>> registeredModifiers  = new();

        /// <inheritdoc cref="RegisterInfo"/>
        public static void RegisterInfoExtractor<TResult>(object key, Func<object, TResult> extractor) => RegisterInfo(key, extractor, registeredExtractors);

        /// <inheritdoc cref="RegisterInfo"/>
        public static void RegisterInfoModifier<T>(object key, Func<T, T> modifier) => RegisterInfo(key, modifier, registeredModifiers);

        /// <param name="key">can also be a collection or a tuple</param>
        private static void RegisterInfo<T, TResult>(object key, Func<T, TResult> func, Dictionary<object, Dictionary<Type, object>> registers) {
            foreach (object k in key.Enumerate()) {
                if (k == null)
                    continue;

                if (!registers.TryGetValue(k, out Dictionary<Type, object> keyed)) {
                    keyed = new();
                    registers.Add(k, keyed);
                }

                if (!keyed.TryGetValue(typeof(TResult), out object funcs)) {
                    funcs = new List<Func<T, TResult>>();
                    keyed.Add(typeof(TResult), funcs);
                }

                ((List<Func<T, TResult>>) funcs).Add(func);
            }
        }

        public static void ExtractAndModifyInfo<T, TResult>(object key, ref T input, ref List<TResult> output, bool add = true) {
            ExtractInfo(key, input, ref output, add);
            ModifyInfo(key, ref input);
        }

        public static void ExtractInfo<TResult>(object key, object input, ref List<TResult> output, bool add = true) {
            if (add)
                output.AddRange(ExtractInfo<TResult>(key, input));
            else
                output = ExtractInfo<TResult>(key, input).ToList();
        }

        [Pure]
        public static IEnumerable<TResult> ExtractInfo<TResult>(object key, object input) {
            foreach (object k in key.Enumerate()) {
                if (!registeredExtractors.TryGetValue(k, out Dictionary<Type, object> keyedExtractors))
                    continue;

                if (!keyedExtractors.TryGetValue(typeof(TResult), out object extractors))
                    continue;

                foreach (Func<object, TResult> extractor in (List<Func<object, TResult>>) extractors)
                    yield return extractor.SafeInvoke(input);
            }
        }

        public static void ModifyInfo<T>(object key, ref T input) => input = ModifyInfo(key, input);

        [Pure]
        public static T ModifyInfo<T>(object key, T input) {
            foreach (object k in key.Enumerate()) {
                if (!registeredModifiers.TryGetValue(k, out Dictionary<Type, object> keyedModifiers))
                    continue;

                if (!keyedModifiers.TryGetValue(typeof(T), out object modifiers))
                    continue;

                foreach (Func<T, T> modifier in (List<Func<T, T>>) modifiers)
                    input = modifier.SafeInvoke(input);
            }

            return input;
        }

    }

}