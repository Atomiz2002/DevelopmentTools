using UnityEditor;

namespace DevelopmentTools.Editor {

    [InitializeOnLoad]
    public static class SoftDependencyRegisterer {

        static SoftDependencyRegisterer() {
            DevelopmentEssentials.Editor.DependencyManagement.SoftDependencyManager.RegisterAsmdefSoftDependencies(
                "DEVELOPMENT_TOOLS_",
                new() {
                    {
                        "DevelopmentTools.Editor.asmdef",
                        new() {
                            new("COMPONENT_NAMES", "ComponentNames"),
                            new("UNITY_2D_SPRITE", "Unity.2D.Sprite.Editor"),
                            new("UNITY_ADDRESSABLES", "Unity.Addressables", "Unity.Addressables.Editor"),
                            new("UNITY_URP", "Unity.RenderPipelines.Universal.Runtime"),
                            // precompiled
                            new("NEWTONSOFT_JSON", "Newtonsoft.Json.dll"),
                            new("ODIN_INSPECTOR", "Sirenix.OdinInspector.Attributes.dll", "Sirenix.OdinInspector.Editor.dll", "Sirenix.Serialization.dll", "Sirenix.Utilities.dll", "Sirenix.Utilities.Editor.dll")
                        }
                    },
                    {
                        "DevelopmentTools.asmdef",
                        new() {
                            // precompiled
                            new("ODIN_INSPECTOR", "Sirenix.OdinInspector.Attributes.dll", "Sirenix.OdinInspector.Editor.dll", "Sirenix.Serialization.dll", "Sirenix.Utilities.dll", "Sirenix.Utilities.Editor.dll")
                        }
                    }
                });
        }

    }

}