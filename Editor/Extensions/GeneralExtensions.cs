using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build;
#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
using Sirenix.Utilities;
#endif

namespace DevelopmentTools.DevelopmentTools.Editor.Extensions {

    public static class GeneralExtensions {

        public static string Readable(this StackFrame frame) =>
#if DEVELOPMENT_TOOLS_EDITOR_ODIN_INSPECTOR
            $"{frame.GetMethod().DeclaringType!.GetNiceName()}:{frame.GetMethod().Name}():{frame.GetFileLineNumber()}";
#else
            $"{frame.GetMethod().DeclaringType!.Name}:{frame.GetMethod().Name}():{frame.GetFileLineNumber()}";
#endif

        public static NamedBuildTarget ToNamed(this BuildTargetGroup buildTargetGroup) => NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

    }

}