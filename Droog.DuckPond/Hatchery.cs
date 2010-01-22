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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Droog.DuckPond {
    public class Hatchery : IHatchery {
        public class Base { }
        public struct MethodMap {
            public readonly MethodInfo InstanceMethod;
            public readonly MethodInfo InterfaceMethod;
            public readonly Type[] Parameters;

            public MethodMap(MethodInfo instanceMethod, MethodInfo interfaceMethod, Type[] parameters) {
                InstanceMethod = instanceMethod;
                InterfaceMethod = interfaceMethod;
                Parameters = parameters;
            }
        }

        private static readonly ConstructorInfo _objConstructor = typeof(object).GetConstructor(new Type[0]);
        private readonly ModuleBuilder _moduleBuilder;
        private readonly AssemblyBuilder _assemblyBuilder;

        public Hatchery() {
            var name = "DuckPond_" + Guid.NewGuid();
            var assemblyName = new AssemblyName(name);
#if DEBUG
            var access = AssemblyBuilderAccess.RunAndSave;
#else
                var access = AssemblyBuilderAccess.Run;
#endif
            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, access);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(name);
        }

#if DEBUG
        public void SaveAssembly(string assemblyName) {
            _assemblyBuilder.Save(assemblyName);
        }
#endif

        public object Create(object instance, Type interfaceType) {
            if(!interfaceType.IsInterface) {
                throw new ArgumentException(string.Format("Type {0} must be an interface type", interfaceType));
            }
            var instanceType = instance.GetType();
            var typeName = string.Format("{0}_as_{1}", instanceType.Name, interfaceType.Name);
            var duckType = _moduleBuilder.GetType(typeName) ?? ResolveDuckType(typeName, instance.GetType(), interfaceType);
            return Activator.CreateInstance(duckType, instance);
        }


        public MethodMap[] GetMap(Type instanceType, Type interfaceType) {
            var interfaceMethods = GetAllInterfaceMethods(interfaceType);
            var instanceMethods = instanceType.GetMethods();
            var maps = new List<MethodMap>();
            foreach(var interfaceMethod in interfaceMethods) {
                var instanceMethod = (from method in instanceMethods
                                      where method.ToString() == interfaceMethod.ToString()
                                      select method).FirstOrDefault();
                if(instanceMethod == null) {
                    return null;
                }
                maps.Add(new MethodMap(instanceMethod, interfaceMethod, interfaceMethod.GetParameters().Select(x => x.ParameterType).ToArray()));
            }
            return maps.ToArray();
        }

        private MethodInfo[] GetAllInterfaceMethods(Type interfaceType) {
            var methods = interfaceType.GetMethods();
            var uniqueMethods = new Dictionary<string, MethodInfo>();
            AddMethods(uniqueMethods, methods);
            foreach(var inheritedInterfaceType in interfaceType.GetInterfaces()) {
                AddMethods(uniqueMethods, inheritedInterfaceType.GetMethods());
            }
            return uniqueMethods.Values.ToArray();
        }

        private void AddMethods(IDictionary<string, MethodInfo> uniqueMethods, MethodInfo[] methods) {
            foreach(var method in methods) {
                uniqueMethods[method.ToString()] = method;
            }
        }

        public bool CanQuack(object instance, Type interfaceType) {
            return GetMap(instance.GetType(), interfaceType) != null;
        }

        public Type ResolveDuckType(string typeName, Type instanceType, Type interfaceType) {
            var typeMap = GetMap(instanceType, interfaceType);
            if(typeMap == null) {
                throw new InvalidCastException(string.Format("Class{0} does not implement all members of interface {1}", instanceType, interfaceType));
            }
            const TypeAttributes typeAttributes = TypeAttributes.AutoClass | TypeAttributes.Class | TypeAttributes.Public;
            var typeBuilder = _moduleBuilder.DefineType(typeName, typeAttributes, typeof(Base), new[] { interfaceType });
            var storageField = CreateConstructor(instanceType, typeBuilder);
            foreach(var map in typeMap) {
                CreateMethodProxy(storageField, typeBuilder, map);
            }
            var duckType = typeBuilder.CreateType();
            return duckType;
        }

        private static FieldBuilder CreateConstructor(Type instanceType, TypeBuilder typeBuilder) {
            var storageField = typeBuilder.DefineField("_wrapped", instanceType, FieldAttributes.Private | FieldAttributes.InitOnly);
            const MethodAttributes constructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var constructor = typeBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, new Type[] { instanceType });
            constructor.SetImplementationFlags(MethodImplAttributes.IL | MethodImplAttributes.Managed);
            var il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _objConstructor);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, storageField);
            il.Emit(OpCodes.Ret);
            return storageField;
        }

        private static void CreateMethodProxy(FieldInfo storageField, TypeBuilder typeBuilder, MethodMap map) {
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            var methodBuilder = typeBuilder.DefineMethod(map.InterfaceMethod.Name, methodAttributes, CallingConventions.HasThis, map.InterfaceMethod.ReturnType, map.Parameters.ToArray());
            var typeArgs = map.InterfaceMethod.GetGenericArguments();
            if(typeArgs != null && typeArgs.Length > 0) {
                var typeNames = new List<string>();
                for(var index = 0; index < typeArgs.Length; index++) {
                    typeNames.Add(string.Format("T{0}", index));
                }
                methodBuilder.DefineGenericParameters(typeNames.ToArray());
            }
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, storageField);
            for(var i = 1; i <= map.Parameters.Length; i++) {
                il.Emit(OpCodes.Ldarg_S, i);
            }
            il.Emit(OpCodes.Callvirt, map.InstanceMethod);
            il.Emit(OpCodes.Ret);
        }
    }
}
