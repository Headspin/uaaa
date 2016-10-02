using Xunit;

namespace Uaaa.Core.Tests
{
    public class ItemsTests {

        /// <summary>
        /// Simple item that does not support change tracking (only property changed notifications).
        /// </summary>
        public class SimpleItem : Model {
            private int value1 = 0;
            public int Value1 { get { return value1; } set { Property.Set<int>(ref value1, value); } }
        }
        /// <summary>
        /// Item that supports change tracking functionality.
        /// </summary>
        public class Item : Model {
            private int value1 = 0;
            private int value2 = 0;
            public int Value1 { get { return value1; } set { Property.Set<int>(ref value1, value); } }
            public int Value2 { get { return value2; } set { Property.Set<int>(ref value2, value); } }


            protected override ChangeManager CreateChangeManager() {
                return new ChangeManager();
            }

            protected override void OnSetInitialValues() {
                base.OnSetInitialValues();
                Property.Init<int>(ref value1, value1, "Value1");
                Property.Init<int>(ref value2, value2, "Value2");
            }
        }

		[Fact]
        public void Items_Changing() {
            Item item1 = new Item();
            Item item2 = new Item();
            Item item3 = new Item();

            Items<Item> items = new Items<Item>() {
                item1, item2, item3
            };
            items.AcceptChanges();

            Assert.False(items.IsChanged);

            item1.Value1 = 10;
            Assert.True(items.IsChanged);

            item1.Value1 = 0;
            Assert.False(items.IsChanged);
        }

		[Fact]
        public void Items_AddingRemovingNonChangedItems() {
            Item item1 = new Item();
            Item item2 = new Item();

            Items<Item> items = new Items<Item>();
            Assert.False(items.IsChanged);

            items.Add(item1);
            Assert.True(items.IsChanged);

            items.Add(item2);
            Assert.True(items.IsChanged);

            items.Remove(item1);
            Assert.True(items.IsChanged);

            items.Remove(item2);
            Assert.False(items.IsChanged);
        }

		[Fact]
        public void Items_RemovingAddingNonChangedItems() {
            Item item1 = new Item();
            Item item2 = new Item();

            Items<Item> items = new Items<Item>() { item1, item2 };
            Assert.True(items.IsChanged);
            items.AcceptChanges();
            Assert.False(items.IsChanged);

            items.Remove(item1);
            Assert.True(items.IsChanged);

            items.Remove(item2);
            Assert.True(items.IsChanged);

            items.Add(item1);
            Assert.True(items.IsChanged);

            items.Add(item2);
            Assert.False(items.IsChanged);
        }

		[Fact]
        public void Items_AddingRemovingChangedItem() {
            Item item1 = new Item();
            Item item2 = new Item();
            Item item3 = new Item();
            item3.Value1 = 10;

            Items<Item> items = new Items<Item>() { item1, item2 };
            items.AcceptChanges();

            Assert.False(items.IsChanged);

            items.Add(item3);
            Assert.True(items.IsChanged);

            items.Remove(item3);
            Assert.False(items.IsChanged);

        }

		[Fact]
        public void Items_AcceptChangesAfterAddingItems() {
            SimpleItem item1 = new SimpleItem();
            SimpleItem item2 = new SimpleItem();

            Items<SimpleItem> items = new Items<SimpleItem>();
            Assert.False(items.IsChanged);

            items.Add(item1);
            items.Add(item2);
            Assert.True(items.IsChanged);

            items.AcceptChanges();
            Assert.False(items.IsChanged);

            items.Remove(item2);
            Assert.True(items.IsChanged);

            items.AcceptChanges();
            Assert.False(items.IsChanged);

        }

		[Fact]
        public void Items_AcceptChangesAfterRemovingItems() {
            SimpleItem item1 = new SimpleItem();
            SimpleItem item2 = new SimpleItem();

            Items<SimpleItem> items = new Items<SimpleItem>() { item1, item2 };
            items.AcceptChanges();
            Assert.False(items.IsChanged);
            items.Remove(item1);
            Assert.True(items.IsChanged);

            items.Remove(item2);
            Assert.True(items.IsChanged);

            items.AcceptChanges();
            Assert.False(items.IsChanged);
        }
    }
}
