#if UNITY_EDITOR && !SIMULATE_BUILD
using System;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using JetBrains.Annotations;

namespace DevelopmentTools.Editor.Debugging.StateDebugger {

    public static class DebugStateExtensions {

        public static T DebugState<T>(this T t, string name = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            StateDebuggerWindow.DebugState(t, name);
#endif
            return t;
        }

        public static T DebugState<T, T2>(this T t, string name, [NotNull] Func<T, T2> func) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            try {
                StateDebuggerWindow.DebugState(func(t), name);
            }
            catch (Exception e) {
                $"Failed to debug state for {name}. {e}".LogEx();
            }
#endif
            return t;
        }

        public static T DebugState<T, T2>(this T t, [NotNull] Func<T, T2> func, string name = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            try {
                StateDebuggerWindow.DebugState(func(t), name);
            }
            catch (Exception e) {
                $"Failed to debug state for {name}. {e}".LogEx();
            }
#endif
            return t;
        }

    }

}
#endif