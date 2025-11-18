using System.Diagnostics;
using Sirenix.Utilities;

namespace DevelopmentTools.Editor.Extensions {

    public static class GeneralExtensions {

        public static string Readable(this StackFrame frame) =>
            $"{frame.GetMethod().DeclaringType!.GetNiceName()}:{frame.GetMethod().Name}():{frame.GetFileLineNumber()}";

    }

}