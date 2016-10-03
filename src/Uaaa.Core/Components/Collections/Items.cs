using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Uaaa
{
    /// <summary>
    /// Observable collection of TItem.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class Items<TItem> : ObservableCollection<TItem>, IModel, INotifyObjectChanged {
        /// <summary>
        /// Counts marked items. Object is changed when count > 0;
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        private sealed class ItemsCounter<TModel> : Model {
            private readonly HashSet<TModel> items = new HashSet<TModel>();
            private int count = 0;
            public int Count { get { return count; } private set { Property.Set<int>(ref count, value); } }
            #region -=Public methods=-
            public IEnumerable<TModel> GetAll() {
                foreach (TModel item in items)
                    yield return item;
            }
            public bool Contains(TModel item) {
                return items.Contains(item);
            }
            public void Add(TModel item) {
                items.Add(item);
                this.Count = items.Count();
            }
            public void Remove(TModel item) {
                items.Remove(item);
                this.Count = items.Count();
            }
            public void Reset() {
                items.Clear();
                this.Count = 0;
            }
            /// <summary>
            /// Accepts chages by reseting the counter.
            /// </summary>
            public override void AcceptChanges() {
                this.Reset();
            }
            #endregion
            #region -=Base class methods=-
            protected override ChangeManager CreateChangeManager() { return new ChangeManager(); }
            protected override void OnSetInitialValues() {
                base.OnSetInitialValues();
                Property.Init<int>(ref count, count, "Count");
            }
            #endregion
        }

        private readonly ItemsCounter<TItem> addedItems = new ItemsCounter<TItem>();
        private readonly ItemsCounter<TItem> removedItems = new ItemsCounter<TItem>();
        /// <summary>
        /// ChangeManager object instance.
        /// </summary>
        protected ChangeManager ChangeManager { get; set; }
        /// <summary>
        /// Holds last item that was added to the collection.
        /// </summary>
        public TItem LastAdded { get; private set; }
        /// <summary>
        /// Creates new object instance.
        /// </summary>
        public Items() {
            this.ChangeManager = new Uaaa.ChangeManager();
            this.ChangeManager.Track(addedItems);
            this.ChangeManager.Track(removedItems);
            this.ChangeManager.ObjectChanged += ChangeManager_ObjectChanged;
        }
        #region -=Public methods=-
        /// <summary>
        /// Returns all items that were added to the initial items collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TItem> GetAddedItems() {
            foreach (TItem item in addedItems.GetAll())
                yield return item;
        }
        /// <summary>
        /// Returns all items that were removed from the initial items collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TItem> GetRemovedItems() {
            foreach (TItem item in removedItems.GetAll())
                yield return item;
        }
        #endregion
        #region -=Base class methods=-
        /// <summary>
        /// Inserts item to the collection at specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, TItem item) {
            base.InsertItem(index, item);
            this.ChangeManager.Track(item as INotifyObjectChanged);
            this.LastAdded = item;
            if (removedItems.Contains(item))
                removedItems.Remove(item);
            else
                addedItems.Add(item);
        }

        /// <summary>
        /// Clears items collection.
        /// </summary>
        protected override void ClearItems() {
            base.ClearItems();
            this.LastAdded = default(TItem);
            AcceptChanges();
        }
        /// <summary>
        /// Removes item from collection.
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index) {
            TItem item = this[index];
            base.RemoveItem(index);
            if (item.Equals(this.LastAdded))
                this.LastAdded = default(TItem);
            this.ChangeManager.Remove(item as INotifyObjectChanged);
            if (addedItems.Contains(item))
                addedItems.Remove(item);
            else
                removedItems.Add(item);
        }
        /// <summary>
        /// Sets the item at specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, TItem item) {
            this.ChangeManager.Remove(this[index] as INotifyObjectChanged);
            if (addedItems.Contains(item))
                addedItems.Remove(item);
            base.SetItem(index, item);
            this.ChangeManager.Track(item as INotifyObjectChanged);
        }
        #endregion
        private void ChangeManager_ObjectChanged(object sender, EventArgs args) {
            this.ObjectChanged?.Invoke(this, new EventArgs());
            ((IModel)this).RaisePropertyChanged("IsChanged");
        }
        #region -=INotifyObjectChanged members=-
        /// <see cref="Uaaa.INotifyObjectChanged.ObjectChanged"/>
        public event EventHandler ObjectChanged;
        /// <see cref="Uaaa.INotifyObjectChanged.IsChanged"/>
        public bool IsChanged { get { return this.ChangeManager.IsChanged; } }
        /// <summary>
        /// Accepts all changes made to the collection.
        /// Property IsChanged gets value False after method is finished.
        /// </summary>
        public void AcceptChanges() {
            this.ChangeManager.AcceptChanges();
        }
        #endregion
        #region -=IModel members=-
        void IModel.RaisePropertyChanged(string propertyName) {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
