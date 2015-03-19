using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Defines common Model intefrace
    /// </summary>
    public interface IModel : INotifyPropertyChanged {
        /// <summary>
        /// Raises PropertyChanged event for provided property name.
        /// </summary>
        /// <param name="propertyName"></param>
        void RaisePropertyChanged(string propertyName);
    }
    /// <summary>
    /// Model base class that supports property change notification and change tracking.
    /// </summary>
    public abstract class Model : IModel, INotifyPropertyChanged, INotifyObjectChanged {
        /// <summary>
        /// Model propertySetter instance.
        /// </summary>
        protected readonly PropertySetter Property;
        /// <summary>
        /// ChangeManager instance for hierarchical change tracking.
        /// Instance should be set when needed by overriding CreateChangeManager method.
        /// </summary>
        protected ChangeManager ChangeManager { get; private set; }
        /// <summary>
        /// Creates new model instance.
        /// </summary>
        public Model() {
            this.Property = new PropertySetter(this);
            this.ChangeManager = CreateChangeManager();
            if (this.ChangeManager != null) {
                this.ChangeManager.ObjectChanged += ChangeManager_ObjectChanged;
            }
            SetInitialValues();
        }
        #region -=Public methods=-
        public void AcceptChanges() {
            this.Property.AcceptChanges();
        }
        #endregion
        #region -=Protected methods=-
        /// <summary>
        /// Create change manager if object change notification is required.
        /// </summary>
        /// <returns></returns>
        protected virtual ChangeManager CreateChangeManager() { return null; }
        /// <summary>
        /// Sets instance initial values for change tracking.
        /// Also initializes change manager.
        /// </summary>
        protected virtual void SetInitialValues() {
            if (this.ChangeManager != null)
                this.ChangeManager.Track(this.Property);
        }
        /// <summary>
        /// Initializes object instance data and resets change tracking to its initial state.
        /// Use this method when implementing initializers.
        /// </summary>
        /// <param name="initializeObject"></param>
        protected void InitializeCore(Action initializeObject) {
            initializeObject();
            if (this.ChangeManager != null)
                this.ChangeManager.Reset();
            SetInitialValues();
            this.Property.AcceptChanges();
        }
        #endregion
        #region -=Private methods=-
        private void ChangeManager_ObjectChanged(object sender, EventArgs args) {
            OnObjectChanged();
        }
        #endregion
        #region -=IModel members=-
        /// <summary>
        /// INotifyPropertyChanged.PropertyChanged implementation.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        void IModel.RaisePropertyChanged(string propertyName) {
            this.RaisePropertyChanged(propertyName);
        }
        /// <summary>
        /// Triggers PropertyChanged event.
        /// </summary>
        /// <param name="propertyName"></param> 
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null && !string.IsNullOrEmpty(propertyName))
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region -=INotifyObjectChanged members=-
        /// <summary>
        /// INotifyObjectChanged.ObjectChanged implementation
        /// </summary>
        public event EventHandler ObjectChanged;
        /// <summary>
        /// TRUE if object changed, false otherwise.
        /// </summary>
        public bool IsChanged {
            get {
                if (this.ChangeManager != null)
                    return this.ChangeManager.IsChanged;
                return this.Property.IsChanged;
            }
        }
        /// <summary>
        /// Raises ObjectChanged event.
        /// </summary>
        protected virtual void OnObjectChanged() {
            EventHandler handler = this.ObjectChanged;
            if (handler != null)
                handler(this, new EventArgs());
        }
        #endregion
    }
}
