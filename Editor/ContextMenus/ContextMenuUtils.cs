using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DevelopmentEssentials.Editor.Extensions.Unity;
using DevelopmentEssentials.Editor.Helpers.Unity;
using DevelopmentEssentials.Extensions.CS;
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Editor.Helpers;
using DevelopmentTools.Settings;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.ContextMenus {

    public static class ContextMenuUtils {

        /// <inheritdoc cref="EngineSettings.MenuGroupPath"/>
        public const string MenuGroupPath = EngineSettings.MenuGroupPath + "Helpers/";

        [MenuItem(MenuGroupPath + "Log StackTraces with Links ^x")]
        public static void LogStackTracesWithLinks() {
            if (!EditorHelper.IsConsoleFocused()) {
                EditorApplication.ExecuteMenuItem("Edit/Cut"); // the default action of the shortcut we are overriding
                return;
            }

            EditorApplication.ExecuteMenuItem("Edit/Copy");
            GUIUtility.systemCopyBuffer.LinkPaths().LOG();
        }

        // TODO override Ctrl C to collapse repeating logs like this -> [Repeated X times]

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
            const string pattern = @"(Rethrow as _: .*|.*[:\.].*\(.*\))";
            return Regex.Split(GUIUtility.systemCopyBuffer, @"\r?\n")
                .Where(line => !Regex.IsMatch(line.Trim(), pattern) && !line.IsNullOrWhiteSpace())
                .Join("\n");
        }

        [MenuItem(MenuGroupPath + "Clear Console &x")]
        public static void ClearConsole() {
            Assembly   assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            Type       type     = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo method   = type.GetMethod("Clear");
            method.SafeInvoke(new(), null);
        }

        [MenuItem("Assets/Development Tools/Set Texture PPU")]
        public static void ShowWindow() {
            List<string> selectedTexturesGUIDs = AssetDatabaseHelper.GetSelectedGUIDsRecursively<Texture2D>();

            if (selectedTexturesGUIDs.IsEmpty()) {
                "No Texture2Ds selected".LogErr();
                return;
            }

            GenericDialogEditorWindow.Show("Set Texture PPU", new("Pixels Per Unit", selectedTexturesGUIDs[0].GUIDToPath().LoadAssetImporter<TextureImporter>()!.spritePixelsPerUnit), ppu => {
                AssetDatabaseHelper.BulkEdit(selectedTexturesGUIDs,
                    guid => {
                        if (!guid.GUIDToPath().TryLoadAssetImporter(out TextureImporter importer))
                            return;

                        importer.spritePixelsPerUnit = (float) ppu;
                        importer.SaveAndReimport();
                    });
            });
        }

    }

}