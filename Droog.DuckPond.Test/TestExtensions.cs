using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Droog.DuckPond.Test {
    public static class TestExtensions {
        public static bool Implements<T>(this object instance) {
            if(instance == null) {
                return false;
            }
            var t = typeof(T);
            return t.IsInterface && t.IsAssignableFrom(instance.GetType());
        }
    }
}
