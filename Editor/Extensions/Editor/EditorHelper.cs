using System;
using System.Collections.Generic;
using System.Linq;
using DevelopmentEssentials.Extensions.CS;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace DevelopmentTools.Editor.Extensions.Editor {

    public static class EditorHelper {

        public static void AddSymbol(string symbol) {
            // current target group
            NamedBuildTarget target         = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string           currentDefines = PlayerSettings.GetScriptingDefineSymbols(target);
            List<string>     defines        = new(currentDefines.Split(';'));

            if (!defines.Contains(symbol)) {
                defines.Add(symbol);
            }

            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines.ToArray()));
            CodeEditor.CurrentEditor.SyncAll();
        }

        public static void RemoveSymbol(string symbol) {
            // current target group
            NamedBuildTarget targetGroup    = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string           currentDefines = PlayerSettings.GetScriptingDefineSymbols(targetGroup);
            List<string>     defines        = new(currentDefines.Split(';'));

            if (defines.Contains(symbol)) {
                defines.Remove(symbol);
            }

            PlayerSettings.SetScriptingDefineSymbols(targetGroup, string.Join(";", defines.ToArray()));
            CodeEditor.CurrentEditor.SyncAll();
        }

        public static bool IsSymbolDefined(string symbol) {
            NamedBuildTarget targetGroup    = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string           currentDefines = PlayerSettings.GetScriptingDefineSymbols(targetGroup);
            List<string>     defines        = new(currentDefines.Split(';'));

            return defines.Contains(symbol);
        }

        public static void GUILayoutLine() {
            GUILayout.Space(10);
            Rect rect3 = EditorGUILayout.GetControlRect(false, 2);
            rect3.height = 2;
            EditorGUI.DrawRect(rect3, Color.gray);
            GUILayout.Space(10);
        }

        public static void DrawDropZone<T>(Rect rect, Action<T[]> onAcceptDrag) {
            Event evt = Event.current;

            if (!rect.Contains(evt.mousePosition))
                return;

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Link;

            if (evt.type == EventType.DragPerform && DragAndDrop.objectReferences.All(x => x is T)) {
                DragAndDrop.AcceptDrag();
                onAcceptDrag.SafeInvoke(DragAndDrop.objectReferences.Cast<T>().ToArray());
            }

            evt.Use();
        }

        public static void DrawDashedBorder(Rect rect, float dashLength, float gapLength, float thickness, Color color) {
            Handles.BeginGUI();
            Handles.color = color;
            DrawDashedLine(new(rect.x, rect.y), new(rect.x + rect.width, rect.y), dashLength, gapLength, thickness);
            DrawDashedLine(new(rect.x + rect.width, rect.y), new(rect.x + rect.width, rect.y + rect.height), dashLength, gapLength, thickness);
            DrawDashedLine(new(rect.x + rect.width, rect.y + rect.height), new(rect.x, rect.y + rect.height), dashLength, gapLength, thickness);
            DrawDashedLine(new(rect.x, rect.y + rect.height), new(rect.x, rect.y), dashLength, gapLength, thickness);
            Handles.EndGUI();
        }

        private static void DrawDashedLine(Vector3 start, Vector3 end, float dashLength, float gapLength, float thickness) {
            float   distance      = Vector3.Distance(start, end);
            Vector3 direction     = (end - start).normalized;
            float   currentLength = 0f;

            while (currentLength < distance) {
                Vector3 dashStart       = start + direction * currentLength;
                float   remainingLength = distance - currentLength;
                float   segmentLength   = Mathf.Min(dashLength, remainingLength);
                Vector3 dashEnd         = dashStart + direction * segmentLength;
                Handles.DrawAAPolyLine(thickness, dashStart, dashEnd);
                currentLength += dashLength + gapLength;
            }
        }

    }

}