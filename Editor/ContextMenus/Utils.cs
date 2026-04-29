using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentEssentials.Extensions.Unity.ExtendedLogger;
using DevelopmentTools.Runtime.Settings;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.Editor.ContextMenus {

    public static class ContextMenuUtils {

        public const string MenuGroupPath = EngineSettings.MenuGroupPath + "Helpers/";

        [MenuItem(MenuGroupPath + "Log StackTraces with Links ^x")]
        public static void LogStackTracesWithLinks() {
            if (!EditorHelper.IsConsoleFocused()) {
                EditorApplication.ExecuteMenuItem("Edit/Cut"); // the default action of the shortcut we are overriding
                return;
            }

            EditorApplication.ExecuteMenuItem("Edit/Copy");
            string clipboard = GUIUtility.systemCopyBuffer;
            ExtendedLogger.ReplacePathsWithLinks(ref clipboard);
            clipboard.LOG();
        }

        [MenuItem(MenuGroupPath + "Copy With StackTraces Unformatted ^#&c")]
        public static void CopyWithStackTracesUnformatted() {
            if (!EditorHelper.IsConsoleFocused()) {
                EditorApplication.ExecuteMenuItem("Assets/Copy Path"); // the default action of the shortcut we are overriding
                return;
            }

            EditorApplication.ExecuteMenuItem("Edit/Copy");
            GUIUtility.systemCopyBuffer = GUIUtility.systemCopyBuffer.Unformatted();
        }

        [MenuItem(MenuGroupPath + "Copy Without Stack Traces Raw ^&c")]
        public static void CopyWithoutStackTracesRaw() {
            if (!EditorHelper.IsConsoleFocused()) {
                EditorApplication.ExecuteMenuItem("Assets/Copy Path"); // the default action of the shortcut we are overriding
                return;
            }

            EditorApplication.ExecuteMenuItem("Edit/Copy");
            GUIUtility.systemCopyBuffer = CleanUpStackTraces();
        }

        [MenuItem(MenuGroupPath + "Copy Without Stack Traces Rich ^#c")]
        public static void CopyWithoutStackTracesUnformatted() {
            if (!EditorHelper.IsConsoleFocused()) {
                EditorHelper.FocusConsole(); // the default action of the shortcut we are overriding
                return;
            }

            EditorApplication.ExecuteMenuItem("Edit/Copy");
            GUIUtility.systemCopyBuffer = CleanUpStackTraces().Unformatted();
        }

        private static string CleanUpStackTraces() {
            const string pattern = @".*[:\.].*\(.*\)";
            return string.Join("\n",
                Regex.Split(GUIUtility.systemCopyBuffer, @"\r?\n")
                    .Where(line => !Regex.IsMatch(line.Trim(), pattern) && !line.IsNullOrWhiteSpace()));
        }

        [MenuItem(MenuGroupPath + "Clear Console &x")]
        public static void ClearConsole() {
            Assembly   assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            Type       type     = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo method   = type.GetMethod("Clear");
            method!.Invoke(new(), null);
        }

        [Pure]
        public static List<string> GetSelectedGUIDsRecursively(string filter = "") {
            List<string> selectedGUIDs = new(Selection.assetGUIDs);
            if (selectedGUIDs.Count == 0) return new();

            foreach (string guid in selectedGUIDs.ToArray()) {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (AssetDatabase.IsValidFolder(path)) // If it's a folder, get all texture GUIDs inside it
                    selectedGUIDs.AddRange(AssetDatabase.FindAssets(filter, new[] { path }));
            }

            return selectedGUIDs;
        }

        public static void BulkEdit(List<string> guids, Action guidsAction) {
            AssetDatabase.StartAssetEditing();

            guidsAction();

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (guids[0].LoadAssetByGUID() is not DefaultAsset)
                AssetDatabase.ImportAsset(guids[0].GUIDToPath(), ImportAssetOptions.ForceUpdate);
        }

        public static void BulkEdit(List<string> guids, Action<string> guidsAction) {
            AssetDatabase.StartAssetEditing();

            foreach (string guid in guids) guidsAction(guid);

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (guids[0].LoadAssetByGUID() is not DefaultAsset)
                AssetDatabase.ImportAsset(guids[0].GUIDToPath(), ImportAssetOptions.ForceUpdate);
        }

    }

}