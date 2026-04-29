using System.Diagnostics;
using System.Runtime.CompilerServices;
using DevelopmentEssentials.Extensions.CS;
using UnityEngine;

namespace DevelopmentTools {

    public partial class DebugLogger {

        public static class Managers {

            private const string conditional = nameof(DebugLogger) + "_" + nameof(Managers);

// @formatter:off
        [Conditional(conditional)] [HideInCallstack] public static void Log                  (object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   nameof(Managers), false, false, false, details);
        [Conditional(conditional)] [HideInCallstack] public static void LogError             (object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   nameof(Managers), false, false, true,  details);
        [Conditional(conditional)] [HideInCallstack] public static void LogSendEvent         (object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   nameof(Managers), true,  false, false, details);
        [Conditional(conditional)] [HideInCallstack] public static void LogSendEventError    (object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   nameof(Managers), true,  false, true,  details);
        [Conditional(conditional)] [HideInCallstack] public static void LogReceivedEvent     (object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   nameof(Managers), true,  true,  false, details);
        [Conditional(conditional)] [HideInCallstack] public static void LogReceivedEventError(object parameter = null, params object[] details) => LogEntry(false, null, new[] { parameter },   nameof(Managers), true,  true,  true,  details);

        [Conditional(conditional)] [HideInCallstack] public static void Log                  (ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), nameof(Managers), false, false, false, details);
        [Conditional(conditional)] [HideInCallstack] public static void LogError             (ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), nameof(Managers), false, false, true,  details);
        [Conditional(conditional)] [HideInCallstack] public static void LogSendEvent         (ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), nameof(Managers), true,  false, false, details);
        [Conditional(conditional)] [HideInCallstack] public static void LogSendEventError    (ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), nameof(Managers), true,  false, true,  details);
        [Conditional(conditional)] [HideInCallstack] public static void LogReceivedEvent     (ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), nameof(Managers), true,  true,  false, details);
        [Conditional(conditional)] [HideInCallstack] public static void LogReceivedEventError(ITuple parameters,       params object[] details) => LogEntry(false, null, parameters?.ToArray(), nameof(Managers), true, true, true, details);

        [Conditional(conditional)] [HideInCallstack] public static void LogGoodDetails(params object[] details) => AddGoodDetails(nameof(Managers), details);
        [Conditional(conditional)] [HideInCallstack] public static void LogBadDetails (params object[] details) => AddBadDetails (nameof(Managers), details);
        [Conditional(conditional)] [HideInCallstack] public static void LogDetails    (params object[] details) => AddDetails    (nameof(Managers), details);
        [Conditional(conditional)] [HideInCallstack] public static void LogReturn     (object returnValue)      => SetReturn     (nameof(Managers), returnValue);
// @formatter:on

        }

    }

}