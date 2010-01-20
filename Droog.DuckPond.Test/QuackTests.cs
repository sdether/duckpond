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
            Assert.AreEqual(1,swan.SideEffectCalled);
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
    public class ReturnOnlyImpl : IReturnOnlyInt {
        private readonly Swan _wrapped;
        public ReturnOnlyImpl(Swan wrapped) { _wrapped = wrapped; }
        public int ReturnOnlyInt() { return _wrapped.ReturnOnlyInt(); }
    }

    public class Swan {
        public int SideEffectCalled;
        public int OverloadIntCalled;
        public int OverloadStringCalled;
        public string Indexer;
        public string Property;
        public int ArgFuncCalled = -1;
        public int ReturnOnlyInt() {
            return 42;
        }

        public string ReturnOnlyString() {
            return "foo";
        }

        public void SideEffectOnly() {
            SideEffectCalled++;
        }

        public void TwoArgs(object a, object b) {
            ArgFuncCalled = 2;
        }

        public void ThreeArgs(string a, object b, int c) {
            ArgFuncCalled = 3;
        }

        public void FourArgs(int a, string b, object c, int d) {
            ArgFuncCalled = 4;
        }

        public void SevenArgs(int a, int b, int c, int d, int e, int f, int g) {
            ArgFuncCalled = 7;
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
}
