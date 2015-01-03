using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Handles property change tracking.
    /// </summary>
    public sealed class PropertySetter : INotifyObjectChanged {
        #region -=Properties/Fields=-
        private readonly IModel _model;
        private bool _isTrackingChanges = false;
        private Dictionary<string, object> _initialValues = null;
        private Dictionary<string, object> _changedValues = null;
        /// <summary>
        /// TRUE when change tracking is enabled by setting inital value of at least one property.
        /// </summary>
        public bool IsTrackingChanges { get { return _isTrackingChanges; } }
        #endregion
        #region -=Constructors=-
        /// <summary>
        /// Creates new instance of PropertySetter class for the provided model.
        /// </summary>
        /// <param name="model"></param>
        public PropertySetter(IModel model) {
            _model = model;
        }
        #endregion
        #region -=Public methods=-
        /// <summary>
        /// Sets new property value and stores it to a backing store variable.
        /// </summary>
        /// <typeparam name="T">Property store type.</typeparam>
        /// <param name="store">Property backing store field.</param>
        /// <param name="value">New property value.</param>
        /// <param name="propertyName">Name of property being changed.</param>
        /// <param name="comparer">Comparer used to compare current and new property value.</param>
        /// <param name="allowChange">Return true if property is allowed to change, false otherwise.</param>
        /// <returns>TRUE if property value changed.</returns>
        public bool SetValue<T>(ref T store, T value, [CallerMemberName] string propertyName = null, IEqualityComparer<T> comparer = null, Func<bool> allowChange = null) {
            IEqualityComparer<T> selectedComparer = comparer ?? EqualityComparer<T>.Default;
            if (selectedComparer.Equals(store, value)) return false;
            if (allowChange != null && !allowChange()) return false;
            store = value;
            if ((!string.IsNullOrEmpty(propertyName))) {
                _model.RaisePropertyChanged(propertyName);
                if (_isTrackingChanges && _initialValues.ContainsKey(propertyName)) {
                    #region -=Handle change tracking notifications=-
                    bool isInitialValue = selectedComparer.Equals((T)_initialValues[propertyName], value);
                    if (isInitialValue && _changedValues.ContainsKey(propertyName)) {
                        _changedValues.Remove(propertyName); // remove from current values -> property holds initial value.
                        this.IsChanged = _changedValues.Count > 0;
                    } else if (!isInitialValue) {
                        if (_changedValues.ContainsKey(propertyName))
                            _changedValues[propertyName] = value; // update changed value.
                        else {
                            _changedValues.Add(propertyName, value); // add to changed values dictionary.
                            this.IsChanged = true;
                        }
                    }
                    #endregion
                }
            }
            return true;
        }
        //NOTE: Should use [CallerMemberName] for propertyName in future WP8.1+ profiles.
        /// <summary>
        /// Set initial property value for change tracked property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="store"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        public void InitValue<T>(ref T store, T value, string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) return;
            if (!_isTrackingChanges) {
                _initialValues = new Dictionary<string, object>();
                _changedValues = new Dictionary<string, object>();
                _isTrackingChanges = true;
            }
            if (!_initialValues.ContainsKey(propertyName))
                _initialValues.Add(propertyName, value);
            else
                _initialValues[propertyName] = value;
            store = value;
        }
        /// <summary>
        /// Accept changes by seting changed property values to initial property values.
        /// </summary>
        public void AcceptChanges() {
            if (!_isTrackingChanges) return;
            foreach (KeyValuePair<string, object> pair in _changedValues) {
                if (!_initialValues.ContainsKey(pair.Key)) continue;
                _initialValues[pair.Key] = pair.Value;
            }
            _changedValues.Clear();
            this.IsChanged = false;
        }
        #endregion
        #region -=INotifyObjectChanged members=-
        /// <summary>
        /// INotifyObjectChanged.ObjectChanged implementation.
        /// </summary>
        public event EventHandler ObjectChanged;
        private bool _isChanged = false;
        /// <summary>
        /// Returns TRUE if object is changed, FALSE otherwise.
        /// </summary>
        public bool IsChanged {
            get { return _isChanged; }
            private set {
                if (_isChanged == value) return;
                _isChanged = value;
                RaiseObjectChanged();
            }
        }
        private void RaiseObjectChanged() {
            if (this.ObjectChanged != null)
                this.ObjectChanged(this, new EventArgs());
        }
        #endregion
    }
}
