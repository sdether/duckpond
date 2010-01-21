using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Droog.DuckPond {
    public class Hatchery {
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
                var interfaceMethodSignature = interfaceMethod.GetSignature();
                var instanceMethod = (from method in instanceMethods
                                      where method.GetSignature() == interfaceMethodSignature
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
                uniqueMethods[method.GetSignature()] = method;
            }
        }

        private static bool HasSameParameters(MethodInfo instanceMethod, ParameterInfo[] interfaceParameters) {
            var parameters = instanceMethod.GetParameters();
            if(parameters.Length != interfaceParameters.Length) {
                return false;
            }
            for(var i = 0; i < parameters.Length; i++) {
                if(parameters[i].IsOut != interfaceParameters[i].IsOut) {
                    return false;
                }
                var type = parameters[i].ParameterType;
                var interfaceType = interfaceParameters[i].ParameterType;
                if(type.IsByRef != interfaceType.IsByRef) {
                    return false;
                }
                if(type.IsGenericParameter && interfaceType.IsGenericParameter) {
                    if(type.BaseType != interfaceType.BaseType) {
                        return false;
                    }
                } else if(type != interfaceType) {
                    return false;
                }
            }
            return true;
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
