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
            DuckExtensions.Factory.SaveAssembly("duckpond");
#endif
        }

        public interface IReturnOnly {
            int ReturnOnlyInt();
        }

        [Test]
        public void Can_proxy_return_only_method() {
            var x = new Swan().As<IReturnOnly>();
            Assert.IsTrue(x.Implements<IReturnOnly>());
            Assert.AreEqual(42,x.ReturnOnlyInt());
        }

        public class Swan {
            public int SideEffectCalled;
            public int OverloadIntCalled;
            public int OverloadStringCalled;
            public string Indexer;
            public string Property;

            public int ReturnOnlyInt() {
                return 42;
            }

            public string ReturnOnlyString() {
                return "foo";
            }

            public void SideEffectOnly() {
                SideEffectCalled++;
            }

            public int LotsOfArgs(int a, int b, int c, int d, int e, int f, int g) {
                return 88;
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
                get {return key;}
                set { Indexer = value+key;}
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
}
