﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Uaaa
{
    /// <summary>
    /// Handles property change tracking and notifications.
    /// </summary>
    public sealed class PropertySetter : INotifyObjectChanged
    {
        #region -=Properties/Fields=-
        private readonly IModel model;
        private bool isTrackingChanges = false;
        private Dictionary<string, object> initialValues = null;
        private Dictionary<string, object> changedValues = null;
        private BusinessRulesChecker checker = null;
        /// <summary>
        /// TRUE when change tracking is enabled by setting inital value of at least one property.
        /// </summary>
        public bool IsTrackingChanges => isTrackingChanges;

        #endregion
        #region -=Constructors=-
        /// <summary>
        /// Creates new instance of PropertySetter class for the provided model.
        /// </summary>
        /// <param name="model"></param>
        public PropertySetter(IModel model)
        {
            this.model = model;
        }
        #endregion
        #region -=Public methods=-
        /// <summary>
        /// Sets new property value and stores it to a backing store variable.
        /// Method automaticaly performs property validation if BusinessRuleChecker is used by PropertySetter and value changed.
        /// </summary>
        /// <typeparam name="T">Property store type.</typeparam>
        /// <param name="store">Property backing store field.</param>
        /// <param name="value">New property value.</param>
        /// <param name="propertyName">Name of property being changed.</param>
        /// <param name="comparer">Comparer used to compare current and new property value.</param>
        /// <param name="canChange">Return true if property is allowed to change, false otherwise.</param>
        /// <returns>TRUE if property value changed.</returns>
        public bool Set<T>(ref T store, T value, [CallerMemberName] string propertyName = null, IEqualityComparer<T> comparer = null, Func<bool> canChange = null)
        {
            IEqualityComparer<T> selectedComparer = comparer ?? EqualityComparer<T>.Default;
            if (selectedComparer.Equals(store, value)) return false;
            if (canChange != null && !canChange()) return false;
            store = value;
            if ((!string.IsNullOrEmpty(propertyName)))
            {

                model.RaisePropertyChanged(propertyName);
                if (isTrackingChanges && initialValues.ContainsKey(propertyName))
                {
                    #region -=Handle change tracking notifications=-
                    bool isInitialValue = selectedComparer.Equals((T)initialValues[propertyName], value);
                    if (isInitialValue && changedValues.ContainsKey(propertyName))
                    {
                        changedValues.Remove(propertyName); // remove from current values -> property holds initial value.
                        this.IsChanged = changedValues.Count > 0;
                    }
                    else if (!isInitialValue)
                    {
                        if (changedValues.ContainsKey(propertyName))
                            changedValues[propertyName] = value; // update changed value.
                        else
                        {
                            changedValues.Add(propertyName, value); // add to changed values dictionary.
                            this.IsChanged = true;
                        }
                    }
                    #endregion
                }
                IsValid(propertyName);
            }
            return true;
        }

        /// <summary>
        /// Resets property value to its initial value.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="propertyName"></param>
        /// <typeparam name="T"></typeparam>
        public void Reset<T>(ref T store, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            if (initialValues.ContainsKey(propertyName))
                Set(ref store, (T)initialValues[propertyName], propertyName);
        }
        /// <summary>
        /// Set initial property value for change tracked property.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        public void Init(object value, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            if (!isTrackingChanges)
            {
                initialValues = new Dictionary<string, object>();
                changedValues = new Dictionary<string, object>();
                isTrackingChanges = true;
            }
            if (!initialValues.ContainsKey(propertyName))
                initialValues.Add(propertyName, value);
            else
                initialValues[propertyName] = value;
        }
        /// <summary>
        /// Set initial property value for change tracked property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="store"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        public void Init<T>(ref T store, T value, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            Init(value, propertyName);
            store = value;
        }
        /// <summary>
        /// Perform property validation. PropertySetter must be using BusinessRulesChecker.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsValid([CallerMemberName] string propertyName = "")
            => (checker?.IsValid(model, propertyName) == true) || checker == null;
        /// <summary>
        /// Returns true if property value was changed.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal bool IsPropertyChanged(string propertyName) => isTrackingChanges && changedValues.ContainsKey(propertyName);

        /// <summary>
        /// Accept changes by seting changed property values to initial property values.
        /// </summary>
        public void AcceptChanges()
        {
            if (!isTrackingChanges) return;
            foreach (KeyValuePair<string, object> pair in changedValues)
            {
                if (!initialValues.ContainsKey(pair.Key)) continue;
                initialValues[pair.Key] = pair.Value;
            }
            changedValues.Clear();
            this.IsChanged = false;
        }
        /// <summary>
        /// Use provided rules checker for validation checking.!--
        /// </summary>
        public void Use(BusinessRulesChecker rulesChecker)
        {
            if (rulesChecker == null)
                throw new ArgumentNullException(nameof(rulesChecker));
            this.checker = rulesChecker;
        }
        #endregion
        #region -=INotifyObjectChanged members=-
        /// <summary>
        /// INotifyObjectChanged.ObjectChanged implementation.
        /// </summary>
        public event EventHandler ObjectChanged;
        private bool isChanged = false;
        /// <summary>
        /// Returns TRUE if object is changed, FALSE otherwise.
        /// </summary>
        public bool IsChanged {
            get { return isChanged; }
            private set {
                if (isChanged == value) return;
                isChanged = value;
                RaiseObjectChanged();
            }
        }
        private void RaiseObjectChanged()
        {
            this.ObjectChanged?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}
