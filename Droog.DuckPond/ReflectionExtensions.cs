using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Droog.DuckPond {
    public static class ReflectionExtensions {
        public static string GetSignature(this MethodInfo methodInfo) {
            return methodInfo.ToString();
        }
    }
}
