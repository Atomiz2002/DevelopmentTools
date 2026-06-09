using System;
using DevelopmentEssentials.Extensions.CS;

namespace DevelopmentTools.Editor.Debugging.RealtimeDebugger {

    public static class RealtimeDebugExtension {

        public static T RealtimeDebug<T>(this T obj, Func<T, bool> condition = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            RealtimeDebuggerWindow.CacheProperty(obj, condition ?? (_ => true));
#endif
            return obj;
        }

        public static T RealtimeDebug<T, T2>(this T obj, Func<T, T2> @as = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            if (@as != null)
                RealtimeDebuggerWindow.CacheProperty(@as.InvokeSafe(obj));
            else
                RealtimeDebuggerWindow.CacheProperty(obj);
#endif
            return obj;
        }

    }

}