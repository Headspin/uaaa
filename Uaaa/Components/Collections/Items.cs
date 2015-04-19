using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Observable collection of TItem.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class Items<TItem> : ObservableCollection<TItem>, IModel, INotifyObjectChanged {
        /// <summary>
        /// Counts marked items. Object is changed when count > 0;
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        private sealed class ItemsCounter<TModel> : Model {
            private readonly HashSet<TModel> _items = new HashSet<TModel>();
            private int _count = 0;
            public int Count { get { return _count; } private set { Property.Set<int>(ref _count, value); } }
            #region -=Public methods=-
            public IEnumerable<TModel> GetAll() {
                foreach (TModel item in _items)
                    yield return item;
            }
            public bool Contains(TModel item) {
                return _items.Contains(item);
            }
            public void Add(TModel item) {
                _items.Add(item);
                this.Count = _items.Count();
            }
            public void Remove(TModel item) {
                _items.Remove(item);
                this.Count = _items.Count();
            }
            public void Reset() {
                _items.Clear();
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
                Property.Init<int>(ref _count, _count, "Count");
            }
            #endregion
        }

        private readonly ItemsCounter<TItem> _addedItems = new ItemsCounter<TItem>();
        private readonly ItemsCounter<TItem> _removedItems = new ItemsCounter<TItem>();

        protected ChangeManager ChangeManager { get; set; }
        public TItem LastAdded { get; private set; }
        public Items() {
            this.ChangeManager = new Uaaa.ChangeManager();
            this.ChangeManager.Track(_addedItems);
            this.ChangeManager.Track(_removedItems);
            this.ChangeManager.ObjectChanged += ChangeManager_ObjectChanged;
        }
        #region -=Public methods=-
        /// <summary>
        /// Returns all items that were added to the initial items collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TItem> GetAddedItems() {
            foreach (TItem item in _addedItems.GetAll())
                yield return item;
        }
        /// <summary>
        /// Returns all items that were removed from the initial items collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TItem> GetRemovedItems() {
            foreach (TItem item in _removedItems.GetAll())
                yield return item;
        }
        #endregion
        #region -=Base class methods=-
        protected override void InsertItem(int index, TItem item) {
            base.InsertItem(index, item);
            this.ChangeManager.Track(item as INotifyObjectChanged);
            this.LastAdded = item;
            if (_removedItems.Contains(item))
                _removedItems.Remove(item);
            else
                _addedItems.Add(item);
        }
        protected override void ClearItems() {
            base.ClearItems();
            this.LastAdded = default(TItem);
            AcceptChanges();
        }
        protected override void RemoveItem(int index) {
            TItem item = this[index];
            base.RemoveItem(index);
            if (item.Equals(this.LastAdded))
                this.LastAdded = default(TItem);
            this.ChangeManager.Remove(item as INotifyObjectChanged);
            if (_addedItems.Contains(item))
                _addedItems.Remove(item);
            else
                _removedItems.Add(item);
        }
        protected override void SetItem(int index, TItem item) {
            this.ChangeManager.Remove(this[index] as INotifyObjectChanged);
            if (_addedItems.Contains(item))
                _addedItems.Remove(item);
            base.SetItem(index, item);
            this.ChangeManager.Track(item as INotifyObjectChanged);
        }
        #endregion
        private void ChangeManager_ObjectChanged(object sender, EventArgs args) {
            if (this.ObjectChanged != null)
                this.ObjectChanged(this, new EventArgs());
            ((IModel)this).RaisePropertyChanged("IsChanged");
        }
        #region -=INotifyObjectChanged members=-
        public event EventHandler ObjectChanged;
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
