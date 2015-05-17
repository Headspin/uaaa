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
    public abstract class Model : IModel, INotifyPropertyChanged, INotifyObjectChanged, INotifyDataErrorInfo {
        /// <summary>
        /// Handles property values change tracking and notifications.
        /// Use PropertySetter for setting property values if you need INotifyPropertyChanged features.
        /// </summary>
        protected readonly PropertySetter Property;
        /// <summary>
        /// ChangeManager instance for hierarchical change tracking.
        /// Instance should be set when needed by overriding CreateChangeManager method.
        /// </summary>
        protected ChangeManager ChangeManager { get; private set; }
        /// <summary>
        /// BusinessRulesChecker that checks model business rules.
        /// </summary>
        /// <value>The rules checker.</value>
        protected BusinessRulesChecker RulesChecker { get; private set; }
        /// <summary>
        /// Creates new model instance.
        /// </summary>
        protected Model() {
            this.Property = new PropertySetter(this);
            this.ChangeManager = InitChangeManager();
            this.RulesChecker = InitRulesChecker();
            SetInitialValues();
        }

        #region -=Public methods=-
        /// <summary>
        /// Accepts changes made to the model.
        /// Method is applicable to models that have ChangeManager set.
        /// </summary>
        public virtual void AcceptChanges() {
            if (this.ChangeManager != null)
                this.ChangeManager.AcceptChanges();
        }
        /// <summary>
        /// Checks model business rules and returns TRUE if all checked businessRules are valid.
        /// Property name can be provided to check rules bound to that specific property. All
        /// rules are checked if propery name not provided.
        /// </summary>
        /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
        public virtual bool IsValid(string propertyName = "") {
            if (this.RulesChecker != null)
                return this.RulesChecker.IsValid(this, propertyName);
            return false;
        }

        #endregion

        #region -=Protected methods=-

        /// <summary>
        /// Create change manager if object change notification is required.
        /// </summary>
        /// <returns></returns>
        protected virtual ChangeManager CreateChangeManager() {
            return null;
        }

        /// <summary>
        /// Creates the rules checker that handles business rules checking.
        /// </summary>
        /// <returns>The rules checker.</returns>
        protected virtual BusinessRulesChecker CreateRulesChecker() {
            return null;
        }

        /// <summary>
        /// Sets instance initial values for change tracking.
        /// </summary>
        protected virtual void OnSetInitialValues() {
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

        private ChangeManager InitChangeManager() {
            ChangeManager manager = CreateChangeManager();
            if (manager != null)
                manager.ObjectChanged += ChangeManager_ObjectChanged;
            return manager;
        }

        private BusinessRulesChecker InitRulesChecker() {
			BusinessRulesChecker checker = CreateRulesChecker ();
            if (checker != null) {
                checker.ErrorsChanged += RulesChecker_ErrorsChanged;
                checker.PropertyChanged += RulesChecker_PropertyChanged;
            }
			return checker;
		}

        private void SetInitialValues() {
            if (this.ChangeManager != null)
                this.ChangeManager.Track(this.Property);
            OnSetInitialValues();
        }

        private void ChangeManager_ObjectChanged(object sender, EventArgs args) {
            OnObjectChanged();
        }

        private void RulesChecker_ErrorsChanged(object sender, DataErrorsChangedEventArgs args) {
            OnErrorsChanged(args);
        }

        private void RulesChecker_PropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (string.Compare(args.PropertyName, "HasErrors", StringComparison.Ordinal) == 0) {
                this.RaisePropertyChanged(args.PropertyName);
            }
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
        #region INotifyDataErrorInfo implementation
        /// <see cref="System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged"/>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        /// <see cref="System.ComponentModel.INotifyDataErrorInfo.GetErrors"/>
        public System.Collections.IEnumerable GetErrors(string propertyName) {
            if (this.RulesChecker != null)
                foreach (var error in this.RulesChecker.GetErrors(propertyName))
                    yield return error;
        }
        /// <see cref="System.ComponentModel.INotifyDataErrorInfo.HasErrors"/>
        public bool HasErrors {
            get {
                if (this.RulesChecker != null)
                    return this.RulesChecker.HasErrors;
                return false;
            }
        }
        /// <summary>
        /// Raises ErrorsChanged event.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs args) {
            var handler = this.ErrorsChanged;
            if (handler != null)
                handler(this, args);
        }

        #endregion
    }
}
