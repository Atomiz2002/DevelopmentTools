using UnityEditor;
using static DevelopmentEssentials.Editor.DependencyManagement.SoftDependencyManager;
using static DevelopmentEssentials.Editor.DependencyManagement.SoftDependencyManager.AsmdefDependencies;

namespace DevelopmentTools.Editor {

    public class SoftDependencyManager : AssetPostprocessor {

        private static readonly AsmdefDependencies runtimeDependencies = new AsmdefDependencies("DevelopmentTools.asmdef", "DEVELOPMENT_TOOLS_RUNTIME_")
            .SetSoftDependencies(new AsmdefDependency("ODIN_INSPECTOR",
                "Sirenix.OdinInspector.Attributes.dll",
                "Sirenix.OdinInspector.Editor.dll",
                "Sirenix.Serialization.dll",
                "Sirenix.Utilities.dll",
                "Sirenix.Utilities.Editor.dll"));

        private static readonly AsmdefDependencies editorDependencies = new AsmdefDependencies("DevelopmentTools.Editor.asmdef", "DEVELOPMENT_TOOLS_EDITOR_")
            .SetSoftDependencies(
                new("COMPONENT_NAMES", "ComponentNames"),
                new("UNITY_2D_SPRITE", "Unity.2D.Sprite.Editor"),
                new("UNITY_ADDRESSABLES", "Unity.Addressables", "Unity.Addressables.Editor"),
                new("UNITY_URP", "Unity.RenderPipelines.Universal.Runtime"),
                new("UNITY_UI", "UnityEngine.UI"),
                // precompiled
                new("NEWTONSOFT_JSON", "Newtonsoft.Json.dll"),
                new("ODIN_INSPECTOR",
                    "Sirenix.OdinInspector.Attributes.dll",
                    "Sirenix.OdinInspector.Editor.dll",
                    "Sirenix.Serialization.dll",
                    "Sirenix.Utilities.dll",
                    "Sirenix.Utilities.Editor.dll")
            );

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths) {
            runtimeDependencies.ReferenceDependencies();
            editorDependencies.ReferenceDependencies();
        }

    }

}