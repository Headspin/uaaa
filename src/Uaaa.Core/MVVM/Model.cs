using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Uaaa
{
    /// <summary>
    /// Defines common Model intefrace
    /// </summary>
    public interface IModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Raises PropertyChanged event for provided property name.
        /// </summary>
        /// <param name="propertyName"></param>
        void RaisePropertyChanged(string propertyName);
    }

    /// <summary>
    /// Model base class that supports property change notification and change tracking.
    /// </summary>
    public abstract class Model : IModel, INotifyObjectChanged, INotifyDataErrorInfo
    {
        /// <summary>
        /// Handles property values change tracking and notifications.
        /// Use PropertySetter for setting property values if you need INotifyPropertyChanged features.
        /// </summary>
        protected readonly PropertySetter Property;
        /// <summary>
        /// ChangeManager instance for hierarchical change tracking.
        /// Instance should be set when needed by overriding CreateChangeManager method.
        /// </summary>
        protected ChangeManager ChangeManager { get; }
        /// <summary>
        /// BusinessRulesChecker that checks model business rules.
        /// </summary>
        /// <value>The rules checker.</value>
        protected BusinessRulesChecker RulesChecker { get; }
        /// <summary>
        /// Creates new model instance.
        /// </summary>
        protected Model()
        {
            Property = new PropertySetter(this);
            ChangeManager = InitChangeManager();
            RulesChecker = InitRulesChecker();
            if (RulesChecker != null)
                Property.Use(RulesChecker);
            SetInitialValues();
        }

        #region -=Public methods=-
        /// <summary>
        /// Accepts changes made to the model.
        /// Method is applicable to models that have ChangeManager set.
        /// </summary>
        public virtual void AcceptChanges()
        {
            ChangeManager?.AcceptChanges();
        }
        /// <summary>
        /// Checks model business rules and returns TRUE if all checked businessRules are valid.
        /// Property name can be provided to check rules bound to that specific property. All
        /// rules are checked if propery name not provided.
        /// </summary>
        /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
        public virtual bool IsValid(string propertyName = "")
            => RulesChecker?.IsValid(this, propertyName) == true;

        #endregion
        #region -=Protected methods=-

        /// <summary>
        /// Create change manager if object change notification is required.
        /// </summary>
        /// <returns></returns>
        protected virtual ChangeManager CreateChangeManager()
        {
            return null;
        }

        /// <summary>
        /// Creates the rules checker that handles business rules checking.
        /// </summary>
        /// <returns>The rules checker.</returns>
        protected virtual BusinessRulesChecker CreateRulesChecker()
        {
            return null;
        }

        /// <summary>
        /// Sets instance initial values for change tracking.
        /// </summary>
        protected virtual void OnSetInitialValues()
        {
        }

        /// <summary>
        /// Initializes object instance data and resets change tracking to its initial state.
        /// Use this method when implementing initializers.
        /// </summary>
        /// <param name="initializeObject"></param>
        protected void InitializeCore(Action initializeObject)
        {
            initializeObject();
            this.ChangeManager?.Reset();
            SetInitialValues();
            this.Property.AcceptChanges();
        }

        #endregion

        #region -=Private methods=-

        private ChangeManager InitChangeManager()
        {
            ChangeManager manager = CreateChangeManager();
            if (manager != null)
                manager.ObjectChanged += ChangeManager_ObjectChanged;
            return manager;
        }

        private BusinessRulesChecker InitRulesChecker()
        {
            BusinessRulesChecker checker = CreateRulesChecker();
            if (checker != null)
            {
                checker.ErrorsChanged += RulesChecker_ErrorsChanged;
                checker.PropertyChanged += RulesChecker_PropertyChanged;
            }
            return checker;
        }

        private void SetInitialValues()
        {
            this.ChangeManager?.Track(this.Property);
            OnSetInitialValues();
        }

        private void ChangeManager_ObjectChanged(object sender, EventArgs args)
        {
            OnObjectChanged();
        }

        private void RulesChecker_ErrorsChanged(object sender, DataErrorsChangedEventArgs args)
        {
            OnErrorsChanged(args);
        }

        private void RulesChecker_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (string.Compare(args.PropertyName, nameof(HasErrors), StringComparison.Ordinal) == 0)
            {
                this.RaisePropertyChanged(args.PropertyName);
            }
        }

        #endregion

        #region -=IModel members=-

        /// <summary>
        /// INotifyPropertyChanged.PropertyChanged implementation.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        void IModel.RaisePropertyChanged(string propertyName)
        {
            this.RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Triggers PropertyChanged event.
        /// </summary>
        /// <param name="propertyName"></param> 
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                if (ChangeManager != null)
                    return ChangeManager.IsChanged;
                return Property.IsChanged;
            }
        }

        /// <summary>
        /// Raises ObjectChanged event.
        /// </summary>
        protected virtual void OnObjectChanged()
        {
            this.ObjectChanged?.Invoke(this, new EventArgs());
        }

        #endregion
        #region INotifyDataErrorInfo implementation
        /// <see cref="System.ComponentModel.INotifyDataErrorInfo.ErrorsChanged"/>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        /// <see cref="System.ComponentModel.INotifyDataErrorInfo.GetErrors"/>
        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (this.RulesChecker != null)
                foreach (object error in RulesChecker.GetErrors(propertyName))
                    yield return error;
        }

        /// <see cref="System.ComponentModel.INotifyDataErrorInfo.HasErrors"/>
        public bool HasErrors => RulesChecker?.HasErrors ?? true;
        /// <summary>
        /// Raises ErrorsChanged event.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs args)
            => ErrorsChanged?.Invoke(this, args);

        #endregion

        #region -=Nested classes=-
        /// <summary>
        /// Exposes internal infrastructure objects of a model to other components in the library.
        /// </summary>
        internal sealed class Inspector
        {
            private readonly Model model;
            public bool IsChanged(string propertyName) => model?.Property.IsPropertyChanged(propertyName) == true;
            public Inspector(Model model)
            {
                this.model = model;
            }
        }
        #endregion
    }
}
