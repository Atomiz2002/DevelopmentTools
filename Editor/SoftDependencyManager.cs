using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor {

    public class SoftDependencyManager : AssetPostprocessor {

        private const string definesPrefix = "DEVELOPMENT_TOOLS_"; // PREFIXES THE SOFT DEPENDENCY DEFINES
        private static readonly Dictionary<string, List<SoftDependency>> asmdefSoftDependencies = new() {
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
        };

        // TODO works every other time
        // TODO safeguard sirenix dependent code with #if
        // TODO if missing is set, it doesnt get unset automatically when the dependency is later added
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths) {
            bool modified = false;

            foreach ((string asmdef, List<SoftDependency> softDependencies) in asmdefSoftDependencies)
                ReferenceSoftDependenciesForAssembly(asmdef, softDependencies, ref modified);

            if (modified)
                AssetDatabase.Refresh();
        }

        private static void ReferenceSoftDependenciesForAssembly(string packageAsmdef, List<SoftDependency> softDependencies, ref bool modified) {
            string packageAsmdefPath = AssetDatabase.FindAssets($"t:AssemblyDefinitionAsset {Path.GetFileNameWithoutExtension(packageAsmdef)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => Path.GetFileName(p) == packageAsmdef); // kind of an unnecessary check?

            if (string.IsNullOrEmpty(packageAsmdefPath))
                throw new("Failed to find packageAsmdef");

            AsmdefData asmdefData = JsonUtility.FromJson<AsmdefData>(File.ReadAllText(packageAsmdefPath, Encoding.UTF8));
            bool       modifiedThis = false;

            foreach (SoftDependency softDependency in softDependencies)
                ReferenceSoftDependencies(asmdefData, ref modifiedThis, softDependency);

            if (modifiedThis) {
                File.WriteAllText(packageAsmdefPath, JsonUtility.ToJson(asmdefData, true), Encoding.UTF8);
                modified = true;
            }
        }

        private static void ReferenceSoftDependencies(AsmdefData asmdefData, ref bool modified, SoftDependency softDependency) {
            const string GUID_Prefix = "GUID:";

            bool foundAllDependencies = true;

            foreach ((string dependency, bool located) in softDependency.dependencies) {
                AsmdefData.VersionDefine missing = AsmdefData.VersionDefine.Missing(softDependency.define, dependency);

                if (located) {
                    asmdefData.versionDefines.RemoveAll(vd => vd.define == missing.define);

                    List<string> references = dependency.EndsWith(".dll")
                        ? asmdefData.precompiledReferences
                        : asmdefData.references;

                    if (!references.Select(reference => reference.StartsWith(GUID_Prefix)
                            ? JsonUtility.FromJson<AsmdefData>(File.ReadAllText(AssetDatabase.GUIDToAssetPath(reference[GUID_Prefix.Length..]), Encoding.UTF8)).name
                            : reference).Contains(dependency))
                        references.Add(dependency);
                }
                else {
                    foundAllDependencies = false;

                    if (asmdefData.versionDefines.RemoveAll(vd => vd.define == softDependency.define) > 0)
                        modified = true;

                    if (asmdefData.versionDefines.Any(vd => vd.define == missing.define))
                        continue;

                    asmdefData.versionDefines.Insert(0, missing);
                }

                modified = true;
            }

            if (foundAllDependencies && !asmdefData.versionDefines.Select(vd => vd.define).Contains(softDependency.define))
                asmdefData.versionDefines.Add(AsmdefData.VersionDefine.Located(softDependency.define));
        }

        [Serializable]
        private class AsmdefData {

            public string name;

            // General Options
            public bool   allowUnsafeCode;
            public bool   autoReferenced;
            public bool   noEngineReferences;
            public bool   overrideReferences;
            public string rootNamespace;

            /// ASMDEFs
            public List<string> references;

            /// DLLs
            public List<string> precompiledReferences;

            // Platforms
            public List<string> includePlatforms;
            public List<string> excludePlatforms;

            public List<string>        defineConstraints;
            public List<VersionDefine> versionDefines = new();

            [Serializable]
            public class VersionDefine {

                public string name;
                public string expression;
                public string define;

                private VersionDefine(string name, string expression, string define) {
                    this.name       = name;
                    this.expression = expression;
                    this.define     = define;
                }

                public static VersionDefine Located(string define)                   => new("Unity", string.Empty, define);
                public static VersionDefine Missing(string define, string reference) => new("Unity", $"Missing {reference}", define);

                public override string ToString() => $"{define} | {name} | {expression}";

            }

        }

        private class SoftDependency {

            public readonly string                   define;
            public readonly Dictionary<string, bool> dependencies;

            public SoftDependency(string define, params string[] dependencies) {
                this.define = definesPrefix + define;

                this.dependencies = dependencies.Select(dependency =>
                        (dependency, located: !string.IsNullOrEmpty(dependency.EndsWith(".dll")
                            ? AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(dependency))
                                .FirstOrDefault(guid => Path.GetFileName(AssetDatabase.GUIDToAssetPath(guid)) == dependency)
                            : AssetDatabase.FindAssets($"t:AssemblyDefinitionAsset {dependency}")
                                .FirstOrDefault(guid => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) == dependency))))
                    .ToDictionary(guid => guid.dependency, guid => guid.located);
            }

            public override string ToString() => $"{define} {string.Join(", ", dependencies.Select(d => $"{d.Key}: {d.Value}"))}";

        }

    }

}