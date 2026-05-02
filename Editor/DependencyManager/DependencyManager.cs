using DependencyManagement;
using UnityEditor;

namespace DevelopmentTools.Editor {

    [InitializeOnLoad]
    public class DependencyManager : DependencyManagement.DependencyManager {

        static DependencyManager() {
            AsmdefsDependencies.Add(new AsmdefDependencies("DevelopmentTools.asmdef", "DEVELOPMENT_TOOLS_RUNTIME_")
                .SetHardDependencies(
                    new("DevelopmentEssentials"))
                .SetSoftDependencies(
                    new("NEWTONSOFT_JSON",
                        "Newtonsoft.Json.dll"),
                    new AsmdefDependencies.SoftAsmdefDependency("ODIN_INSPECTOR",
                        "Sirenix.OdinInspector.Attributes.dll",
                        "Sirenix.OdinInspector.Editor.dll",
                        "Sirenix.Serialization.dll",
                        "Sirenix.Utilities.dll",
                        "Sirenix.Utilities.Editor.dll")));

            AsmdefsDependencies.Add(new AsmdefDependencies("DevelopmentTools.Editor.asmdef", "DEVELOPMENT_TOOLS_EDITOR_")
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
                        "Sirenix.Utilities.Editor.dll")));
        }

    }

}