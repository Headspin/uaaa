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
        protected ChangeManager ChangeManager { get; set; }
        public TItem LastAdded { get; private set; }
        public Items() {
            this.ChangeManager = new Uaaa.ChangeManager();
            this.ChangeManager.ObjectChanged += ChangeManager_ObjectChanged;
        }
        #region -=Base class members=-
        protected override void InsertItem(int index, TItem item) {
            base.InsertItem(index, item);
            this.ChangeManager.Track(item as INotifyObjectChanged);
            this.LastAdded = item;
        }
        protected override void ClearItems() {
            base.ClearItems();
            this.ChangeManager.Reset();
            this.LastAdded = default(TItem);
        }
        protected override void RemoveItem(int index) {
            TItem item = this[index];
            base.RemoveItem(index);
            if (item.Equals(this.LastAdded))
                this.LastAdded = default(TItem);
        }
        protected override void SetItem(int index, TItem item) {
            this.ChangeManager.Remove(this[index] as INotifyObjectChanged);
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
        #endregion
        #region -=IModel members=-
        void IModel.RaisePropertyChanged(string propertyName) {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
