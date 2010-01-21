using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Droog.DuckPond {
    public static class DuckExtensions {
        private static readonly object padlock = new object();
        private static IHatchery _factory;

        public static IHatchery Factory {
            get {
                if(_factory == null) {
                    lock(padlock) {
                        if(_factory == null) {
                            _factory = new Hatchery();
                        }
                    }
                }
                return _factory;
            }
            set {
                _factory = value;
            }
        }

        public static T As<T>(this object instance) {
            return (T)Factory.Create(instance, typeof(T));
        }

        public static object As(this object instance, Type interfaceType) {
            return Factory.Create(instance, interfaceType);
        }
    }
}
