﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uaaa;

namespace UaaaTest {
    [TestClass]
    public class ItemsTest {

        public class Item : Model {
            private int _value1 = 0;
            private int _value2 = 0;
            public int Value1 { get { return _value1; } set { Property.Set<int>(ref _value1, value); } }
            public int Value2 { get { return _value2; } set { Property.Set<int>(ref _value2, value); } }


            protected override ChangeManager CreateChangeManager() {
                return new ChangeManager();
            }

            protected override void SetInitialValues() {
                base.SetInitialValues();
                Property.Init<int>(ref _value1, _value1, "Value1");
                Property.Init<int>(ref _value2, _value2, "Value2");
            }
        }

        [TestMethod]
        public void Items_Changing() {
            Item item1 = new Item();
            Item item2 = new Item();
            Item item3 = new Item();

            Items<Item> items = new Items<Item>() {
                item1, item2, item3
            };
            items.AcceptChanges();

            Assert.IsFalse(items.IsChanged, "Items collection should not be changed.");

            item1.Value1 = 10;
            Assert.IsTrue(items.IsChanged, "Items collection should be changed.");

            item1.Value1 = 0;
            Assert.IsFalse(items.IsChanged, "Items collection should not be changed.");
        }

        [TestMethod]
        public void Items_AddingRemovingNonChangedItems() {
            Item item1 = new Item();
            Item item2 = new Item();

            Items<Item> items = new Items<Item>();
            Assert.IsFalse(items.IsChanged, "Items collection should not be changed.");

            items.Add(item1);
            Assert.IsTrue(items.IsChanged, "Items collection should be changed.");

            items.Add(item2);
            Assert.IsTrue(items.IsChanged, "Items collection should be changed.");

            items.Remove(item1);
            Assert.IsTrue(items.IsChanged, "Items collection should be changed.");

            items.Remove(item2);
            Assert.IsFalse(items.IsChanged, "Items collection should not be changed.");
        }

        [TestMethod]
        public void Items_RemovingAddingNonChangedItems() {
            Item item1 = new Item();
            Item item2 = new Item();

            Items<Item> items = new Items<Item>() { item1, item2 };
            Assert.IsTrue(items.IsChanged, "Items collection should not be changed.");
            items.AcceptChanges();
            Assert.IsFalse(items.IsChanged, "Items collection should not be changed.");

            items.Remove(item1);
            Assert.IsTrue(items.IsChanged, "Items collection should be changed.");

            items.Remove(item2);
            Assert.IsTrue(items.IsChanged, "Items collection should be changed.");

            items.Add(item1);
            Assert.IsTrue(items.IsChanged, "Items collection should be changed.");

            items.Add(item2);
            Assert.IsFalse(items.IsChanged, "Items collection should be changed.");
        }

        [TestMethod]
        public void Items_AddingRemovingChangedItem() {
            Item item1 = new Item();
            Item item2 = new Item();
            Item item3 = new Item();
            item3.Value1 = 10;

            Items<Item> items = new Items<Item>() { item1, item2 };
            items.AcceptChanges();

            Assert.IsFalse(items.IsChanged, "Items collection should not be changed.");

            items.Add(item3);
            Assert.IsTrue(items.IsChanged, "Items collection should be changed.");

            items.Remove(item3);
            Assert.IsFalse(items.IsChanged, "Items collection should not be changed.");

        }
    }
}
