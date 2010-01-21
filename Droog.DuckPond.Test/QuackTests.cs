using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Droog.DuckPond.Test {

    [TestFixture]
    public class QuackTests {

        [TestFixtureTearDown]
        public void GlobalTeardown() {
#if DEBUG
            DuckExtensions.Factory.SaveAssembly("duckpond.dll");
#endif
        }

        [Test]
        public void Can_proxy_return_only_method_int_version() {
            var duck = new Swan().As<IReturnOnlyInt>();
            Assert.IsTrue(duck.Implements<IReturnOnlyInt>());
            Assert.AreEqual(42, duck.ReturnOnlyInt());
        }

        [Test]
        public void Can_proxy_return_only_method_string_version() {
            var duck = new Swan().As<IReturnOnlyString>();
            Assert.IsTrue(duck.Implements<IReturnOnlyString>());
            Assert.AreEqual("foo", duck.ReturnOnlyString());
        }

        [Test]
        public void Can_proxy_sideeffect_only_method() {
            var swan = new Swan();
            var duck = swan.As<ISideEffectOnly>();
            Assert.IsTrue(duck.Implements<ISideEffectOnly>());
            duck.SideEffectOnly();
            Assert.AreEqual(1, swan.SideEffectCalled);
        }

        [Test]
        public void Can_proxy_arg_and_return_method() {
            var swan = new Swan();
            var duck = swan.As<IArgAndReturn>();
            Assert.IsTrue(duck.Implements<IArgAndReturn>());
            Assert.AreEqual("foo", swan.ArgAndReturn("foo"));
            Assert.AreEqual("foo", duck.ArgAndReturn("foo"));
        }

        [Test]
        public void Can_proxy_two_arg_method() {
            var swan = new Swan();
            var duck = swan.As<ITwoArgs>();
            Assert.IsTrue(duck.Implements<ITwoArgs>());
            duck.XArgs("foo", 1);
            Assert.AreEqual(new object[] { "foo", 1 }, swan.Args);
        }

        [Test]
        public void Can_proxy_three_arg_method() {
            var swan = new Swan();
            var duck = swan.As<IThreeArgs>();
            Assert.IsTrue(duck.Implements<IThreeArgs>());
            duck.XArgs("a", "b", 3);
            Assert.AreEqual(new object[] { "a", "b", 3 }, swan.Args);
        }

        [Test]
        public void Can_proxy_four_arg_method() {
            var swan = new Swan();
            var duck = swan.As<IFourArgs>();
            Assert.IsTrue(duck.Implements<IFourArgs>());
            duck.XArgs(1, "b", "c", 4);
            Assert.AreEqual(new object[] { 1, "b", "c", 4 }, swan.Args);
        }

        [Test]
        public void Can_proxy_seven_arg_method() {
            var swan = new Swan();
            var duck = swan.As<ISevenArgs>();
            Assert.IsTrue(duck.Implements<ISevenArgs>());
            duck.XArgs(1,2,3,4,5,6,7);
            Assert.AreEqual(new object[] { 1, 2, 3, 4, 5, 6, 7 }, swan.Args);
        }

        [Test]
        public void Can_proxy_out_arg_method() {
            var swan = new Swan();
            var duck = swan.As<IOutArgs>();
            Assert.IsTrue(duck.Implements<IOutArgs>());
            int y;
            duck.OutArgs(out y);
            Assert.AreEqual(swan.OutArgValue,y);
        }

        [Test]
        public void Can_proxy_ref_arg_method() {
            var swan = new Swan();
            var duck = swan.As<IRefArgs>();
            Assert.IsTrue(duck.Implements<IRefArgs>());
            var y = 37;
            duck.RefArgs(ref y);
            Assert.AreNotEqual(37,y);
            Assert.AreEqual(swan.RefArgValue, y);
            Assert.AreEqual(37,swan.RefArgIn);
        }

        [Test]
        public void Can_proxy_a_single_overloaded_method() {
            var swan = new Swan();
            var duck = swan.As<IOneOfTheOverloads>();
            Assert.IsTrue(duck.Implements<IOneOfTheOverloads>());
            duck.Overload(1);
            Assert.AreEqual(1, swan.OverloadIntCalled);
            Assert.AreEqual(0, swan.OverloadStringCalled);
        }

        [Test]
        public void Can_proxy_a_multiple_overloaded_methods() {
            var swan = new Swan();
            var duck = swan.As<IBothOverloads>();
            Assert.IsTrue(duck.Implements<IBothOverloads>());
            duck.Overload(1);
            Assert.AreEqual(1, swan.OverloadIntCalled);
            Assert.AreEqual(0, swan.OverloadStringCalled);
            duck.Overload("foo");
            Assert.AreEqual(1, swan.OverloadIntCalled);
            Assert.AreEqual(1, swan.OverloadStringCalled);
        }

        [Test]
        public void Can_proxy_indexer_property() {
            var swan = new Swan();
            var duck = swan.As<IIndexer>();
            Assert.IsTrue(duck.Implements<IIndexer>());
            duck["key"] = "bar";
            Assert.AreEqual("barkey",swan.Indexer);
            swan.Indexer = "baz";
            Assert.AreEqual("baz",duck["key"]);
        }

        [Test]
        public void Can_proxy_property_get_and_set() {
            var swan = new Swan();
            var duck = swan.As<IProp>();
            Assert.IsTrue(duck.Implements<IProp>());
            duck.Prop = "baz";
            Assert.AreEqual("baz", swan.Property);
            Assert.AreEqual("baz", duck.Prop);
        }

        [Test]
        public void Can_proxy_property_get_only() {
            var swan = new Swan();
            var duck = swan.As<IPropGetOnly>();
            Assert.IsTrue(duck.Implements<IPropGetOnly>());
            swan.Property = "baz";
            Assert.AreEqual("baz", duck.Prop);
        }

        [Test]
        public void Can_proxy_all_methods_on_swan() {
            var swan = new Swan();
            var duck = swan.As<ISwan>();
            Assert.IsTrue(duck.Implements<ISwan>());
        }

        [Test]
        public void Can_proxy_generic_getter_method() {
            var swan = new Swan();
            var duck = swan.As<IGenericGetterMethod>();
            Assert.IsTrue(duck.Implements<IGenericGetterMethod>());
            swan.GenericValue = "foo";
            Assert.AreEqual("foo", duck.GenericGetter<string>());
        }

        [Test]
        public void Can_proxy_generic_setter_method() {
            var swan = new Swan();
            var duck = swan.As<IGenericSetterMethod>();
            Assert.IsTrue(duck.Implements<IGenericSetterMethod>());
            duck.GenericSetter("foo");
            Assert.AreEqual("foo", swan.GenericValue);
        }

        [Test]
        public void Can_proxy_return_method_on_generic_class_with_generic_interface() {
            var swan = new GenericSwan<string>();
            var duck = swan.As<IGenericGetValue<string>>();
            Assert.IsTrue(duck.Implements<IGenericGetValue<string>>());
            swan.Value = "baz";
            Assert.AreEqual("baz", duck.GetValue());
        }

        [Test]
        public void Can_proxy_return_method_on_generic_class_with_non_generic_interface_inheriting_generic_interface() {
            var swan = new GenericSwan<string>();
            var duck = swan.As<IStringGetValueViaGenericInterface>();
            Assert.IsTrue(duck.Implements<IStringGetValueViaGenericInterface>());
            swan.Value = "baz";
            Assert.AreEqual("baz", duck.GetValue());
        }

        [Test]
        public void Can_proxy_return_method_on_generic_class_with_non_generic_interface() {
            var swan = new GenericSwan<string>();
            var duck = swan.As<IStringGetValue>();
            Assert.IsTrue(duck.Implements<IStringGetValue>());
            swan.Value = "baz";
            Assert.AreEqual("baz", duck.GetValue());
        }

        [Test]
        public void Can_proxy_setter_method_on_generic_class_with_generic_interface() {
            var swan = new GenericSwan<string>();
            var duck = swan.As<IGenericSetValue<string>>();
            Assert.IsTrue(duck.Implements<IGenericSetValue<string>>());
            duck.SetValue("baz");
            Assert.AreEqual("baz", swan.Value);
        }

        [Test]
        public void Can_proxy_setter_method_on_generic_class_with_non_generic_interface_inheriting_generic_interface() {
            var swan = new GenericSwan<string>();
            var duck = swan.As<IStringSetValueViaGenericInterface>();
            Assert.IsTrue(duck.Implements<IStringSetValueViaGenericInterface>());
            duck.SetValue("baz");
            Assert.AreEqual("baz", swan.Value);
        }

        [Test]
        public void Can_proxy_setter_method_on_generic_class_with_non_generic_interface() {
            var swan = new GenericSwan<string>();
            var duck = swan.As<IStringSetValue>();
            Assert.IsTrue(duck.Implements<IStringSetValue>());
            duck.SetValue("baz");
            Assert.AreEqual("baz", swan.Value);
        }
    }

    #region Interfaces
    public interface IReturnOnlyInt {
        int ReturnOnlyInt();
    }
    public interface IReturnOnlyString {
        string ReturnOnlyString();
    }
    public interface ISideEffectOnly {
        void SideEffectOnly();
    }
    public interface IArgAndReturn {
        string ArgAndReturn(string a);
    }
    public interface ITwoArgs {
        void XArgs(object a, object b);
    }
    public interface IThreeArgs {
        void XArgs(string a, object b, int c);
    }
    public interface IFourArgs {
        void XArgs(int a, string b, object c, int d);
    }
    public interface ISevenArgs {
        void XArgs(int a, int b, int c, int d, int e, int f, int g);
    }
    public interface IOutArgs {
        void OutArgs(out int a);
    }
    public interface IRefArgs {
        void RefArgs(ref int a);
    }
    public interface IOneOfTheOverloads {
        void Overload(int x);
    }
    public interface IBothOverloads {
        void Overload(int x);
        void Overload(string x);
    }
    public interface IIndexer {
        string this[string key] { get; set; }
    }
    public interface IProp {
        string Prop { get; set; }
    }
    public interface IPropGetOnly {
        string Prop { get; }
    }
    public interface IGenericGetterMethod {
        T GenericGetter<T>();
    }
    public interface IGenericSetterMethod {
        void GenericSetter<T>(T value);
    }
    public interface ISwan {
        int ReturnOnlyInt();
        string ReturnOnlyString();
        void SideEffectOnly();
        void XArgs(object a, object b);
        void XArgs(string a, object b, int c);
        void XArgs(int a, string b, object c, int d);
        void XArgs(int a, int b, int c, int d, int e, int f, int g);
        void OutArgs(out int a);
        void RefArgs(ref int a);
        void Overload(int x);
        void Overload(string x);
        string this[string key] { get; set; }
        string Prop { get; set; }
        T GenericGetter<T>();
        void GenericSetter<T>(T value);
    }
    public interface IGenericGetValue<T> {
        T GetValue();
    }
    public interface IStringGetValueViaGenericInterface : IGenericGetValue<string> { }
    public interface IStringGetValue {
        string GetValue();
    }
    public interface IGenericSetValue<T> {
        void SetValue(T value);
    }
    public interface IStringSetValueViaGenericInterface : IGenericSetValue<string> { }
    public interface IStringSetValue {
        void SetValue(string value);
    }
    #endregion

    #region Classes to be proxied
    public class Swan {
        public int OutArgValue = 24;
        public int RefArgValue = 42;
        public int RefArgIn = 0;
        public int SideEffectCalled;
        public int OverloadIntCalled;
        public int OverloadStringCalled;
        public string Indexer;
        public string Property;
        public object[] Args;
        public object GenericValue;

        public int ReturnOnlyInt() {
            return 42;
        }
        public string ReturnOnlyString() {
            return "foo";
        }
        public void SideEffectOnly() {
            SideEffectCalled++;
        }
        public string ArgAndReturn(string a) {
            return a;
        }
        public void XArgs(object a, object b) {
            Args = new object[] { a, b };
        }
        public void XArgs(string a, object b, int c) {
            Args = new object[] { a, b, c };
        }
        public void XArgs(int a, string b, object c, int d) {
            Args = new object[] { a, b, c, d };
        }
        public void XArgs(int a, int b, int c, int d, int e, int f, int g) {
            Args = new object[] { a, b, c, d, e, f, g };
        }
        public void OutArgs(out int a) {
            a = OutArgValue;
        }
        public void RefArgs(ref int a) {
            RefArgIn = a;
            a = RefArgValue;
        }
        public void Overload(int x) {
            OverloadIntCalled++;
        }
        public void Overload(string x) {
            OverloadStringCalled++;
        }
        public string this[string key] {
            get { return Indexer; }
            set { Indexer = value + key; }
        }
        public string Prop {
            get { return Property; }
            set { Property = value; }
        }
        public T GenericGetter<T>() {
            return (T) GenericValue;
        }
        public void GenericSetter<T>(T value) {
            GenericValue = value;
        }
    }

    public class GenericSwan<T> {
        public T Value;

        public T GetValue() {
            return Value;
        }

        public void SetValue(T value) {
            Value = value;
        }
    }
    #endregion

    #region Test Implementations for IL comparison
    public class ReturnOnlyImpl : IReturnOnlyInt {
        private readonly Swan _wrapped;
        public ReturnOnlyImpl(Swan wrapped) { _wrapped = wrapped; }
        public int ReturnOnlyInt() { return _wrapped.ReturnOnlyInt(); }
    }

    public class ArgAndReturnImpl : IArgAndReturn {
        private readonly Swan _wrapped;
        public ArgAndReturnImpl(Swan wrapped) { _wrapped = wrapped; }
        public string ArgAndReturn(string a) { return _wrapped.ArgAndReturn(a); }
    }
    #endregion
}
