using System;
using System.Collections.Generic;

namespace Uaaa.Components.Collections
{
    /// <summary>
    /// Indexed collection of items.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TItem">Item type.</typeparam>
    public class Index<TKey, TItem> : Items<TItem>
    {
        private readonly Dictionary<TKey, TItem> itemsByKey = new Dictionary<TKey, TItem>();
        private readonly Func<TItem, TKey> resolveKeyFunc;
        /// <summary>
        /// Instance constructor.
        /// </summary>
        /// <param name="resolveKey"></param>
        public Index(Func<TItem, TKey> resolveKey)
        {
            resolveKeyFunc = resolveKey;
        }
        /// <summary>
        /// Returns true if item with provided key is present in collection.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
            => itemsByKey.ContainsKey(key);
        /// <summary>
        /// Returns item with provided key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TItem GetItem(TKey key)
            => itemsByKey[key];
        /// <summary>
        /// Returns item with provided key if present in collection.
        /// Default(TItem) is returned if item is not present.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TItem TryGetItem(TKey key)
            => ContainsKey(key) ? GetItem(key) : default(TItem);

        #region -=Base class members=-
        /// <see cref="Items{TItem}.ClearItems()"/>
        protected override void ClearItems()
        {
            itemsByKey.Clear();
            base.ClearItems();
        }
        /// <see cref="Items{TItem}.InsertItem(int, TItem)"/>
        protected override void InsertItem(int index, TItem item)
        {
            TKey key = resolveKeyFunc(item);
            this.itemsByKey[key] = item;
            base.InsertItem(index, item);
        }
        /// <see cref="Items{TItem}.RemoveItem(int)"/>
        protected override void RemoveItem(int index)
        {
            TKey key = resolveKeyFunc(this[index]);
            itemsByKey.Remove(key);
            base.RemoveItem(index);
        }
        /// <see cref="Items{TItem}.SetItem(int, TItem)"/>
        protected override void SetItem(int index, TItem item)
        {
            TKey oldItemKey = resolveKeyFunc(this[index]);
            itemsByKey.Remove(oldItemKey);

            TKey key = resolveKeyFunc(item);
            itemsByKey[key] = item;

            base.SetItem(index, item);
        }
        #endregion
    }
}