using System;
using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using JetBrains.Annotations;

namespace DevelopmentTools {

    public static class InfoContainer {

        // using object instead of List<Func<object, object((T))>> for performance reasons
        private static readonly Dictionary<object, Dictionary<Type, object>> registeredExtractors = new();
        private static readonly Dictionary<object, Dictionary<Type, object>> registeredModifiers  = new();

        public static void RegisterInfoExtractor<T, TResult>(object key, Func<T, TResult> extractor) {
            if (!registeredExtractors.TryGetValue(key, out Dictionary<Type, object> typedExtractors)) {
                typedExtractors = new();
                registeredExtractors.Add(key, typedExtractors);
            }

            if (!typedExtractors.TryGetValue(typeof(TResult), out object extractors)) {
                extractors = new List<Func<T, TResult>>();
                typedExtractors.Add(typeof(TResult), extractors);
            }

            ((List<Func<T, TResult>>) extractors).Add(extractor);
        }

        public static void RegisterInfoModifier<T>(object key, Func<T, T> modifier) {
            if (!registeredModifiers.TryGetValue(key, out Dictionary<Type, object> typedModifiers)) {
                typedModifiers = new();
                registeredModifiers.Add(key, typedModifiers);
            }

            if (!typedModifiers.TryGetValue(typeof(T), out object modifiers)) {
                modifiers = new List<Func<T, T>>();
                typedModifiers.Add(typeof(T), modifiers);
            }

            ((List<Func<T, T>>) modifiers).Add(modifier);
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
            if (!registeredModifiers.TryGetValue(key, out Dictionary<Type, object> typedExtractors))
                yield break;

            if (!typedExtractors.TryGetValue(typeof(T), out object extractors))
                yield break;

            foreach (Func<object, T> extractor in (List<Func<object, T>>) extractors)
                yield return extractor.SafeInvoke(input);
        }

        public static void ModifyInfo<T>(object key, ref T input) => input = ModifyInfo(key, input);

        [Pure]
        public static T ModifyInfo<T>(object key, T input) {
            if (!registeredModifiers.TryGetValue(key, out Dictionary<Type, object> typedModifiers))
                return input;

            if (!typedModifiers.TryGetValue(typeof(T), out object modifiers))
                return input;

            foreach (Func<object, T> modifier in (List<Func<object, T>>) modifiers)
                input = modifier.SafeInvoke(input);

            return input;
        }

    }

}