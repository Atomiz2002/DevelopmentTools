using DevelopmentEssentials.Editor.DependencyManagement;
using UnityEditor;
using static DevelopmentEssentials.Editor.DependencyManagement.AsmdefDependencies;

namespace DevelopmentTools.Editor {

    public class DevelopmentToolsSoftDependencyManager : SoftDependencyManager {

        private static readonly AsmdefDependencies runtimeDependencies = new AsmdefDependencies("DevelopmentTools.asmdef", "DEVELOPMENT_TOOLS_RUNTIME_")
            .SetHardDependencies(
                new("DevelopmentEssentials"))
            .SetSoftDependencies(
                new("NEWTONSOFT_JSON",
                    "Newtonsoft.Json.dll"),
                new SoftAsmdefDependency("ODIN_INSPECTOR",
                    "Sirenix.OdinInspector.Attributes.dll",
                    "Sirenix.OdinInspector.Editor.dll",
                    "Sirenix.Serialization.dll",
                    "Sirenix.Utilities.dll",
                    "Sirenix.Utilities.Editor.dll"));

        private static readonly AsmdefDependencies editorDependencies = new AsmdefDependencies("DevelopmentTools.Editor.asmdef", "DEVELOPMENT_TOOLS_EDITOR_")
            .SetHardDependencies(
                new("DevelopmentEssentials"),
                new("DevelopmentEssentials.Editor"),
                new("DevelopmentTools"),
                new("UniTask"))
            .SetSoftDependencies(
                new("COMPONENT_NAMES",
                    "ComponentNames"),
                new("UNITY_2D_SPRITE",
                    "Unity.2D.Sprite.Editor"),
                new("UNITY_ADDRESSABLES",
                    "Unity.Addressables",
                    "Unity.Addressables.Editor"),
                new("UNITY_URP",
                    "Unity.RenderPipelines.Universal.Runtime"),
                new("UNITY_UI",
                    "UnityEngine.UI"),
                new("NEWTONSOFT_JSON",
                    "Newtonsoft.Json.dll"),
                new("ODIN_INSPECTOR",
                    "Sirenix.OdinInspector.Attributes.dll",
                    "Sirenix.OdinInspector.Editor.dll",
                    "Sirenix.Serialization.dll",
                    "Sirenix.Utilities.dll",
                    "Sirenix.Utilities.Editor.dll"));

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths) {
            runtimeDependencies.ReferenceDependencies();
            editorDependencies.ReferenceDependencies();
            AssetDatabase.Refresh();
        }

    }

}