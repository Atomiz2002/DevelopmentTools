using System.Diagnostics;
#if DEVELOPMENT_TOOLS_ODIN_INSPECTOR
using Sirenix.Utilities;
#endif

namespace DevelopmentTools.Editor.Extensions {

    public static class GeneralExtensions {

        public static string Readable(this StackFrame frame) =>
#if DEVELOPMENT_TOOLS_ODIN_INSPECTOR
            $"{frame.GetMethod().DeclaringType!.GetNiceName()}:{frame.GetMethod().Name}():{frame.GetFileLineNumber()}";
#else
            $"{frame.GetMethod().DeclaringType!.Name}:{frame.GetMethod().Name}():{frame.GetFileLineNumber()}";
#endif

    }

}