/*
 * DuckPond 
 * Copyright (C) 2010 Arne F. Claassen
 * http://www.claassen.net/geek/blog geekblog [at] claassen [dot] net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;

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
