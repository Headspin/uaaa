using System;
using Xunit;

namespace Uaaa.Core.Tests
{

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

		[Fact]
        public void PropertySetter_Int() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            int store = 0;
            bool result = setter.Set<int>(ref store, 0);
            Assert.False(result);

            result = setter.Set<int>(ref store, 1);
            Assert.True(result);

            result = setter.Set<int>(ref store, 1);
            Assert.False(result);
        }

		[Fact]
        public void PropertySetter_IntNullable() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            int? store = null;
            bool result = setter.Set<int?>(ref store, 0);
            Assert.True(result);

            result = setter.Set<int?>(ref store, 1);
            Assert.True(result);

            result = setter.Set<int?>(ref store, 1);
            Assert.False(result);

            result = setter.Set<int?>(ref store, null);
            Assert.True(result);
        }
		[Fact]
        public void PropertySetter_DateTime() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            DateTime store = new DateTime(2014, 1, 1);
            bool result = setter.Set<DateTime>(ref store, new DateTime(2014, 1, 1));
            Assert.False(result);

            result = setter.Set<DateTime>(ref store, new DateTime(2014, 2, 1));
            Assert.True(result);

            result = setter.Set<DateTime>(ref store, new DateTime(2014, 2, 1));
            Assert.False(result);
        }

		[Fact]
        public void PropertySetter_DateTimeNullable() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            DateTime? store = new DateTime(2014, 1, 1);
            bool result = setter.Set<DateTime?>(ref store, null);
            Assert.True(result);

            result = setter.Set<DateTime?>(ref store, null);
            Assert.False(result);

            result = setter.Set<DateTime?>(ref store, new Nullable<DateTime>(new DateTime(2014, 2, 1)));
            Assert.True(result);

            result = setter.Set<DateTime?>(ref store, new Nullable<DateTime>(new DateTime(2014, 2, 1)));
            Assert.False(result);

            result = setter.Set<DateTime?>(ref store, null);
            Assert.True(result);
        }

		[Fact]
        public void PropertySetter_Enum() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            TestEnum store = TestEnum.Value0;
            bool result = setter.Set<TestEnum>(ref store, TestEnum.Value0);
            Assert.False(result);

            result = setter.Set<TestEnum>(ref store, TestEnum.Value1);
            Assert.True(result);

            result = setter.Set<TestEnum>(ref store, TestEnum.Value1);
            Assert.False(result);

            result = setter.Set<TestEnum>(ref store, TestEnum.Value0);
            Assert.True(result);
        }
		[Fact]
        public void PropertySetter_EnumNullable() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            TestEnum? store = null;
            bool result = setter.Set<TestEnum?>(ref store, null);
            Assert.False(result);

            result = setter.Set<TestEnum?>(ref store, TestEnum.Value1);
            Assert.True(result);

            result = setter.Set<TestEnum?>(ref store, TestEnum.Value1);
            Assert.False(result);

            result = setter.Set<TestEnum?>(ref store, null);
            Assert.True(result);
        }

		[Fact]
        public void PropertySetter_String() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            string store = "Value1";
            bool result = setter.Set<string>(ref store, "Value1");
            Assert.False(result);

            result = setter.Set<string>(ref store, "value1");
            Assert.True(result);

            result = setter.Set<string>(ref store, "value2");
            Assert.True(result);
        }

		[Fact]
        public void PropertySetter_StringWithComparer() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            string store = "Value1";
            bool result = setter.Set<string>(ref store, "Value1");
            Assert.False(result);

            result = setter.Set<string>(ref store, "value1", comparer: StringComparer.OrdinalIgnoreCase);
            Assert.False(result);

            result = setter.Set<string>(ref store, "value2", comparer: StringComparer.OrdinalIgnoreCase);
            Assert.True(result);
        }

		[Fact]
        public void PropertySetter_CustomType() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            Item store = null;
            bool result = setter.Set<Item>(ref store, new Item() { Value = 1 });
            Assert.True(result);

            result = setter.Set<Item>(ref store, new Item() { Value = 1 });
            Assert.False(result);

            result = setter.Set<Item>(ref store, new Item() { Value = 2 });
            Assert.True(result);

            result = setter.Set<Item>(ref store, null);
            Assert.True(result);
        }
		[Fact]
        public void PropertySetter_ChangeTrackingOff() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            Assert.False(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);
            int store = 0;
            bool result = setter.Set<int>(ref store, 1, "property1");
			Assert.True (result);
            Assert.False(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);
        }

		[Fact]
        public void PropertySetter_ChangeTrackingOneValue() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            Assert.False(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);
            int store = 0;
            setter.Init<int>(ref store, 1, "property1");
            Assert.True(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);

            setter.Set<int>(ref store, 2, "property1");
            Assert.True(setter.IsTrackingChanges);
            Assert.True(setter.IsChanged);

            setter.Set<int>(ref store, 1, "property1");
            Assert.True(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);
        }

		[Fact]
        public void PropertySetter_ChangeTrackingTwoValues() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            Assert.False(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);
            int store1 = 0;
            int store2 = 0;
            setter.Init<int>(ref store1, 1, "property1");
            setter.Init<int>(ref store2, 10, "property2");
            Assert.True(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);

            setter.Set<int>(ref store1, 2, "property1");
            Assert.True(setter.IsTrackingChanges);
            Assert.True(setter.IsChanged);

            setter.Set<int>(ref store2, 20, "property2");
            Assert.True(setter.IsTrackingChanges);
            Assert.True(setter.IsChanged);

            setter.Set<int>(ref store1, 1, "property1");
            Assert.True(setter.IsTrackingChanges);
            Assert.True(setter.IsChanged);

            setter.Set<int>(ref store2, 10, "property2");
            Assert.True(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);
        }


		[Fact]
        public void PropertySetter_ChangeTrackingAcceptChanges() {
            PropertySetter setter = new MyModel().GetPropertySetter();
            Assert.False(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);
            int store1 = 0;
            int store2 = 0;
            setter.Init<int>(ref store1, 1, "property1");
            setter.Init<int>(ref store2, 10, "property2");
            Assert.True(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);

            setter.Set<int>(ref store1, 2, "property1");
            Assert.True(setter.IsTrackingChanges);
            Assert.True(setter.IsChanged);

            setter.Set<int>(ref store2, 20, "property2");
            Assert.True(setter.IsTrackingChanges);
            Assert.True(setter.IsChanged);

            setter.AcceptChanges();
            Assert.True(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);

            setter.Set<int>(ref store1, 1, "property1");
            Assert.True(setter.IsTrackingChanges);
            Assert.True(setter.IsChanged);

            setter.Set<int>(ref store2, 10, "property2");
            Assert.True(setter.IsTrackingChanges);
            Assert.True(setter.IsChanged);

            setter.Set<int>(ref store1, 2, "property1");
            setter.Set<int>(ref store2, 20, "property2");

            Assert.True(setter.IsTrackingChanges);
            Assert.False(setter.IsChanged);
        }
    }
}
