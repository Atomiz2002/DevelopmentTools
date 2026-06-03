using System;

namespace DevelopmentTools.Editor.Debugging.RealtimeDebugger {

    public static class RealtimeDebugExtension {

        public static T RealtimeDebug<T>(this T obj, Func<T, bool> condition = null) {
#if UNITY_EDITOR && !SIMULATE_BUILD
            RealtimeDebuggerWindow.CacheProperty(obj, condition ?? (_ => true));
#endif
            return obj;
        }

    }

}