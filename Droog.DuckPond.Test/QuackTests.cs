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
            var x = new Swan().As<IReturnOnlyInt>();
            Assert.IsTrue(x.Implements<IReturnOnlyInt>());
            Assert.AreEqual(42, x.ReturnOnlyInt());
        }

        [Test]
        public void Can_proxy_return_only_method_string_version() {
            var x = new Swan().As<IReturnOnlyString>();
            Assert.IsTrue(x.Implements<IReturnOnlyString>());
            Assert.AreEqual("foo", x.ReturnOnlyString());
        }

        [Test]
        public void Can_proxy_sideeffect_only_method() {
            var swan = new Swan();
            var x = swan.As<ISideEffectOnly>();
            Assert.IsTrue(x.Implements<ISideEffectOnly>());
            x.SideEffectOnly();
            Assert.AreEqual(1, swan.SideEffectCalled);
        }

        [Test]
        public void Can_proxy_arg_and_return_method() {
            var swan = new Swan();
            var x = swan.As<IArgAndReturn>();
            Assert.IsTrue(x.Implements<IArgAndReturn>());
            Assert.AreEqual("foo", swan.ArgAndReturn("foo"));
            Assert.AreEqual("foo", x.ArgAndReturn("foo"));
        }

        [Test]
        public void Can_proxy_two_arg_method() {
            var swan = new Swan();
            var x = swan.As<ITwoArgs>();
            Assert.IsTrue(x.Implements<ITwoArgs>());
            x.XArgs("foo", 1);
            Assert.AreEqual(new object[] { "foo", 1 }, swan.Args);
        }

        [Test]
        public void Can_proxy_three_arg_method() {
            var swan = new Swan();
            var x = swan.As<IThreeArgs>();
            Assert.IsTrue(x.Implements<IThreeArgs>());
            x.XArgs("a", "b", 3);
            Assert.AreEqual(new object[] { "a", "b", 3 }, swan.Args);
        }

        [Test]
        public void Can_proxy_four_arg_method() {
            var swan = new Swan();
            var x = swan.As<IFourArgs>();
            Assert.IsTrue(x.Implements<IFourArgs>());
            x.XArgs(1, "b", "c", 4);
            Assert.AreEqual(new object[] { 1, "b", "c", 4 }, swan.Args);
        }

        [Test]
        public void Can_proxy_seven_arg_method() {
            var swan = new Swan();
            var x = swan.As<ISevenArgs>();
            Assert.IsTrue(x.Implements<ISevenArgs>());
            x.XArgs(1,2,3,4,5,6,7);
            Assert.AreEqual(new object[] { 1, 2, 3, 4, 5, 6, 7 }, swan.Args);
        }

    }

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
    }

    public class Swan {
        public int SideEffectCalled;
        public int OverloadIntCalled;
        public int OverloadStringCalled;
        public string Indexer;
        public string Property;
        public object[] Args;

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
            a = 24;
        }

        public void RefArgs(ref int a) {
            a = 5;
        }

        public void Overload(int x) {
            OverloadIntCalled++;
        }
        public void Overload(string x) {
            OverloadStringCalled++;
        }

        public string this[string key] {
            get { return key; }
            set { Indexer = value + key; }
        }

        public string Prop {
            get { return Property; }
            set { Property = value; }
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

}
