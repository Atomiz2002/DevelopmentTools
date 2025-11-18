using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DevelopmentEssentials.Editor.Extensions.Unity;
using UnityEditor;

namespace DevelopmentTools.Editor.ContextMenus {

    public static class ContextMenuUtils {

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