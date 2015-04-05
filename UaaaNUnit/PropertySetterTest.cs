using System;
using NUnit.Framework;
using Uaaa;

namespace UaaaNUnit {
	[TestFixture()]
    public class PropertySetterTest {
        public sealed class MyModel : Model {
            public PropertySetter GetPropertySetter() {
                return Property;
            }
        }

        public class Item {
            public int Value { get; set; }
            public override bool Equals(object obj) {
                Item source = obj as Item;
                if (source != null)
                    return source.Value.Equals(this.Value);
                return false;
            }
            public override int GetHashCode() {
                return this.Value.GetHashCode();
            }
        }

        public enum TestEnum {
            Value0 = 0,
            Value1 = 1,
            Value2 = 2
        }

		[Test()]
        public void PropertySetter_Int() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            int store = 0;
            bool result = setter.Set<int>(ref store, 0);
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<int>(ref store, 1);
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<int>(ref store, 1);
            Assert.IsFalse(result, "Invalid SetValue result.");
        }

		[Test()]
        public void PropertySetter_IntNullable() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            int? store = null;
            bool result = setter.Set<int?>(ref store, 0);
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<int?>(ref store, 1);
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<int?>(ref store, 1);
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<int?>(ref store, null);
            Assert.IsTrue(result, "Invalid SetValue result.");
        }
		[Test()]
        public void PropertySetter_DateTime() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            DateTime store = new DateTime(2014, 1, 1);
            bool result = setter.Set<DateTime>(ref store, new DateTime(2014, 1, 1));
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<DateTime>(ref store, new DateTime(2014, 2, 1));
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<DateTime>(ref store, new DateTime(2014, 2, 1));
            Assert.IsFalse(result, "Invalid SetValue result.");
        }

		[Test()]
        public void PropertySetter_DateTimeNullable() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            DateTime? store = new DateTime(2014, 1, 1);
            bool result = setter.Set<DateTime?>(ref store, null);
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<DateTime?>(ref store, null);
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<DateTime?>(ref store, new Nullable<DateTime>(new DateTime(2014, 2, 1)));
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<DateTime?>(ref store, new Nullable<DateTime>(new DateTime(2014, 2, 1)));
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<DateTime?>(ref store, null);
            Assert.IsTrue(result, "Invalid SetValue result.");
        }

		[Test()]
        public void PropertySetter_Enum() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            TestEnum store = TestEnum.Value0;
            bool result = setter.Set<TestEnum>(ref store, TestEnum.Value0);
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<TestEnum>(ref store, TestEnum.Value1);
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<TestEnum>(ref store, TestEnum.Value1);
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<TestEnum>(ref store, TestEnum.Value0);
            Assert.IsTrue(result, "Invalid SetValue result.");
        }
		[Test()]
        public void PropertySetter_EnumNullable() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            TestEnum? store = null;
            bool result = setter.Set<TestEnum?>(ref store, null);
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<TestEnum?>(ref store, TestEnum.Value1);
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<TestEnum?>(ref store, TestEnum.Value1);
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<TestEnum?>(ref store, null);
            Assert.IsTrue(result, "Invalid SetValue result.");
        }

		[Test()]
        public void PropertySetter_String() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            string store = "Value1";
            bool result = setter.Set<string>(ref store, "Value1");
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<string>(ref store, "value1");
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<string>(ref store, "value2");
            Assert.IsTrue(result, "Invalid SetValue result.");
        }

		[Test()]
        public void PropertySetter_StringWithComparer() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            string store = "Value1";
            bool result = setter.Set<string>(ref store, "Value1");
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<string>(ref store, "value1", comparer: StringComparer.OrdinalIgnoreCase);
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<string>(ref store, "value2", comparer: StringComparer.OrdinalIgnoreCase);
            Assert.IsTrue(result, "Invalid SetValue result.");
        }

		[Test()]
        public void PropertySetter_CustomType() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            Item store = null;
            bool result = setter.Set<Item>(ref store, new Item() { Value = 1 });
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<Item>(ref store, new Item() { Value = 1 });
            Assert.IsFalse(result, "Invalid SetValue result.");

            result = setter.Set<Item>(ref store, new Item() { Value = 2 });
            Assert.IsTrue(result, "Invalid SetValue result.");

            result = setter.Set<Item>(ref store, null);
            Assert.IsTrue(result, "Invalid SetValue result.");
        }
		[Test()]
        public void PropertySetter_ChangeTrackingOff() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            Assert.IsFalse(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");
            int store = 0;
            bool result = setter.Set<int>(ref store, 1, "property1");
			Assert.IsTrue (result, "Invalid property setter return value.");
            Assert.IsFalse(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");
        }

		[Test()]
        public void PropertySetter_ChangeTrackingOneValue() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            Assert.IsFalse(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");
            int store = 0;
            setter.Init<int>(ref store, 1, "property1");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store, 2, "property1");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsTrue(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store, 1, "property1");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");
        }

		[Test()]
        public void PropertySetter_ChangeTrackingTwoValues() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            Assert.IsFalse(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");
            int store1 = 0;
            int store2 = 0;
            setter.Init<int>(ref store1, 1, "property1");
            setter.Init<int>(ref store2, 10, "property2");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store1, 2, "property1");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsTrue(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store2, 20, "property2");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsTrue(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store1, 1, "property1");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsTrue(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store2, 10, "property2");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");
        }


		[Test()]
        public void PropertySetter_ChangeTrackingAcceptChanges() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            Assert.IsFalse(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");
            int store1 = 0;
            int store2 = 0;
            setter.Init<int>(ref store1, 1, "property1");
            setter.Init<int>(ref store2, 10, "property2");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store1, 2, "property1");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsTrue(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store2, 20, "property2");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsTrue(setter.IsChanged, "Invalid IsChanged value");

            setter.AcceptChanges();
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store1, 1, "property1");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsTrue(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store2, 10, "property2");
            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsTrue(setter.IsChanged, "Invalid IsChanged value");

            setter.Set<int>(ref store1, 2, "property1");
            setter.Set<int>(ref store2, 20, "property2");

            Assert.IsTrue(setter.IsTrackingChanges, "Invalid IsChangeTracking value");
            Assert.IsFalse(setter.IsChanged, "Invalid IsChanged value");
        }
    }
}
