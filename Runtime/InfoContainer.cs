using System;
using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using JetBrains.Annotations;

namespace DevelopmentTools {

    public static class InfoContainer {

        // using object instead of List<Func<object, object((T))>> for performance reasons
        private static readonly Dictionary<Type, Dictionary<Type, object>> registeredExtractors = new();
        private static readonly Dictionary<Type, Dictionary<Type, object>> registeredModifiers  = new();

        public static void RegisterInfoExtractor<T, TResult>(Type type, Func<T, TResult> extractor) {
            if (!registeredExtractors.TryGetValue(type, out Dictionary<Type, object> typedExtractors)) {
                typedExtractors = new();
                registeredExtractors.Add(type, typedExtractors);
            }

            if (!typedExtractors.TryGetValue(typeof(TResult), out object extractors)) {
                extractors = new();
                typedExtractors.Add(typeof(TResult), extractors);
            }

            ((List<Func<T, TResult>>) extractors).Add(extractor);
        }

        public static void RegisterInfoModifier<T, TResult>(Type type, Func<T, TResult> modifier) {
            if (!registeredModifiers.TryGetValue(type, out Dictionary<Type, object> typedModifiers)) {
                typedModifiers = new();
                registeredModifiers.Add(type, typedModifiers);
            }

            if (!typedModifiers.TryGetValue(typeof(TResult), out object modifiers)) {
                modifiers = new();
                typedModifiers.Add(typeof(TResult), modifiers);
            }

            ((List<Func<T, TResult>>) modifiers).Add(modifier);
        }

        public static void ExtractAndModifyInfo<T>(Type type, ref string input, ref List<T> output, bool add = true) {
            ExtractInfo(type, input, ref output, add);
            ModifyInfo(type, ref input);
        }

        public static void ExtractInfo<T>(Type type, object input, ref List<T> output, bool add = true) {
            if (add)
                output.AddRange(ExtractInfo<T>(type, input));
            else
                output = ExtractInfo<T>(type, input).ToList();
        }

        [Pure]
        public static IEnumerable<T> ExtractInfo<T>(Type type, object input) {
            if (!registeredModifiers.TryGetValue(type, out Dictionary<Type, object> typedExtractors))
                yield break;

            if (!typedExtractors.TryGetValue(typeof(T), out object extractors))
                yield break;

            foreach (Func<object, T> extractor in (List<Func<object, T>>) extractors)
                yield return extractor.SafeInvoke(input);
        }

        public static void ModifyInfo<T>(Type type, ref T input) => input = ModifyInfo(type, input);

        [Pure]
        public static T ModifyInfo<T>(Type type, T input) {
            if (!registeredModifiers.TryGetValue(type, out Dictionary<Type, object> typedModifiers))
                return input;

            if (!typedModifiers.TryGetValue(typeof(T), out object modifiers))
                return input;

            foreach (Func<object, T> modifier in (List<Func<object, T>>) modifiers)
                input = modifier.SafeInvoke(input);

            return input;
        }

    }

}