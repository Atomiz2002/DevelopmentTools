using System;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace DevelopmentTools {

    public static class FilteredConsoleLogger {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Initialize() {
#if !ENABLE_LOGS
            Debug.unityLogger.logEnabled = false;
#else
            Debug.unityLogger.logHandler = new FilteredLogger(Debug.unityLogger.logHandler);
#endif
        }

        private class FilteredLogger : ILogHandler {

            private readonly ILogHandler originalLogHandler;

            private static readonly string[] blacklist = {
                "Disposing UnityDB",
                "Compiling...",
                "Compilation complete.",
            };

            private static readonly string[] blacklistError = {
                "Unable to perform online search",
                "apkanalyzer failed to estimate the apk size",
                "Some objects were not cleaned up when closing the scene. (Did you spawn new GameObjects from OnDestroy?)",
                "Sentry: (Error) sentry-cli: error: Project not found. Ensure that you configured the correct project and organization.",
                "Missing shader. PostProcessing render passes will not execute. Check for missing reference in the renderer resources.",
            };

            private static readonly string[] blacklistException = {
                "AppleCoreNativeMac assembly",
                "The given key 'Steam' was not present in the dictionary.",
                "Unity Remote requirements check failed",
                "(0,0): Burst error BC1091: External and internal calls are not allowed inside static constructors: Interop.BCrypt.BCryptGenRandom(System.IntPtr hAlgorithm, byte* pbBuffer, int cbBuffer, int dwFlags)",
            };

            public FilteredLogger(ILogHandler original) => originalLogHandler = original;

            [HideInCallstack]
            public void LogFormat(LogType logType, Object context, string format, params object[] args) {
                string message = string.Format(format, args);

#if SENTRY && !UNITY_EDITOR
            Sentry.SentrySdk.CaptureMessage(message.Unformatted(), logType switch {
                LogType.Error     => Sentry.SentryLevel.Error,
                LogType.Assert    => Sentry.SentryLevel.Debug,
                LogType.Warning   => Sentry.SentryLevel.Warning,
                LogType.Log       => Sentry.SentryLevel.Info,
                _                 => Sentry.SentryLevel.Info
            });
#endif

#if !ENABLE_LOGS
                return;
#endif

#if ONLY_EXCEPTIONS
            string stackTrace = new StackTrace().SafeString();

            if (logType != LogType.Error && !stackTrace.Contains(nameof(ExtendedLogger)) && !stackTrace.Contains(".P[T] ("))
                return;
#endif

                switch (logType) {
                    case LogType.Log when blacklist.Any(str => message.Contains(str)):
                    case LogType.Error when blacklistError.Any(str => message.Contains(str)):
                        return;
                    default:
                        originalLogHandler.LogFormat(logType, context, format, args);
                        break;
                }
            }

            [HideInCallstack]
            public void LogException(Exception exception, Object context) {
#if SENTRY && !UNITY_EDITOR // TODO test added sentry tags
            exception.AddSentryTag("username", StateSystem.Instance.stateRemote.profile.username);
            exception.AddSentryTag("email", StateSystem.Instance.stateLocal.email);
            Sentry.SentrySdk.CaptureException(exception);
#endif

#if !ENABLE_LOGS
                return;
#endif

                if (blacklistException.Any(str => exception.Message.Contains(str)))
                    return;

// #if UNITY_EDITOR && !SIMULATE_BUILD
//             if (TSHEditorSettings.FocusExceptions && Application.isPlaying) {
//                 if (!consoleWindow)
//                     consoleWindow = EditorExtensions.TryFocusWindow("Console");
//
//                 if (!consoleWindow)
//                     EditorApplication.ExecuteMenuItem("Window/General/Console");
//             }
// #endif

                // string message = exception.Message.Unformatted();
                // originalLogHandler.LogException(new(message, exception), context);

#if UNITY_EDITOR && !SIMULATE_BUILD
                // Exception ex = exception;

                originalLogHandler.LogException(exception, context);

                // while (ex.InnerException != null) {
                //     string message = $"  ↳{ex.Message.Unformatted()}";
                //     originalLogHandler.LogException(new _(message), context);
                //     ex = ex.InnerException;
                // }
#endif
            }

            private class _ : Exception {

                public _(string message) : base(message) {}

            }

        }

    }

}