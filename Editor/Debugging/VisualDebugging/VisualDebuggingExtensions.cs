using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.Debugging.VisualDebugging {

    public static class VisualDebuggerExtensions {

        private static readonly Color[]                   colors;
        private static readonly Dictionary<string, Color> assignedColors = new();

        static VisualDebuggerExtensions() {
            Type colorType = typeof(Color);

            colors = colorType.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == colorType)
                .Select(p => (Color) p.GetValue(null))
                .ToArray();
        }

        // use stack trace if issues with same caller member (method) name from different classes. most probable in behaviours
        public static void QueueDraw(Action draw, int index = 0, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0) =>
            VisualDebugger.drawRequests[Caller(member, line, index)] = draw;

        public static Vector2 Draw(this Vector3 v3, Vector2 offset, int thickness = 1, Color color = default, int index = 0, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0) =>
            v3.V2().Draw(offset, thickness, color, index, member, line);

        public static Vector2Int Draw(this Vector2Int v2, Vector2 offset, int thickness = 1, Color color = default)
            => Draw((Vector2) v2, offset, thickness, color).Int();

        public static Vector2Int Draw(this Vector2Int v2, Vector2Int offset, int thickness = 1, Color color = default)
            => Draw((Vector2) v2, offset, thickness, color).Int();

        /// Colors with A &lt; 5 will be set to A = .5
        public static Vector2 Draw(this Vector2 v2, Vector2 offset, int thickness = 1, Color color = default, int index = 0, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0) {
            Caller(member, line, index, out string caller);

            if (color.a < 5 && !assignedColors.TryGetValue(caller, out color)) {
                color                  = colors.Random().A(.5f);
                assignedColors[caller] = color;
            }

            QueueDraw(() => {
                Handles.color = color;
                Handles.DrawLine(offset, offset + v2, thickness);
            });

            return v2;
        }

        private static string Caller(string member, int line, int index)                    => member + line + index;
        private static string Caller(string member, int line, int index, out string caller) => caller = member + line + index;

    }

}