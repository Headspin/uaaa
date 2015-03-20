using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Implements INotifyDataErrorInfo and handles data validation via added PropertyValidators.
    /// </summary>
    public class DataErrorInfo : INotifyDataErrorInfo {
        private Dictionary<string, Items<PropertyValidator>> _rulesByPropertyName = new Dictionary<string, Items<PropertyValidator>>();
        private Dictionary<string, Items<PropertyValidator>> _currentErrors = new Dictionary<string, Items<PropertyValidator>>();
        public DataErrorInfo() { }
        #region -=Public methods=-
        /// <summary>
        /// Adds validator to the manager.
        /// </summary>
        /// <param name="validator"></param>
        public void Add(PropertyValidator validator) {
            AddToIndex(validator);
        }
        /// <summary>
        /// Removes validator from manager.
        /// </summary>
        /// <param name="validator"></param>
        public void Remove(PropertyValidator validator) {
            RemoveFromIndex(validator);
        }
        public bool IsValid(object model, string propertyName = "") {
            bool isValid = true;
            if (string.IsNullOrEmpty(propertyName)) {
                #region -=Check all rules=-
                foreach (KeyValuePair<string, Items<PropertyValidator>> pair in _rulesByPropertyName) {
                    Items<PropertyValidator> errors = new Items<PropertyValidator>();
                    foreach (PropertyValidator rule in GetErrors(model, pair.Value)) {
                        errors.Add(rule);
                        isValid = false;
                        this.HasErrors = true;
                    }
                    if (_currentErrors.ContainsKey(pair.Key))
                        _currentErrors[pair.Key] = errors;
                    else
                        _currentErrors.Add(pair.Key, errors);

                    if (isValid)
                        this.HasErrors = false;
                    OnErrorsChanged(pair.Key);

                }
                #endregion
            } else if (_rulesByPropertyName.ContainsKey(propertyName)) {
                #region -=Check property specific rules=-
                Items<PropertyValidator> errors = new Items<PropertyValidator>();
                foreach (PropertyValidator rule in GetErrors(model, _rulesByPropertyName[propertyName])) {
                    errors.Add(rule);
                    isValid = false;
                }
                if (_currentErrors.ContainsKey(propertyName))
                    _currentErrors[propertyName] = errors;
                else
                    _currentErrors.Add(propertyName, errors);

                this.HasErrors = !isValid;
                OnErrorsChanged(propertyName);
                #endregion
            }
            return isValid;
        }
        #endregion
        #region -=Private methods=-
        private void AddToIndex(PropertyValidator item) {
            if (!_rulesByPropertyName.ContainsKey(item.PropertyName))
                _rulesByPropertyName.Add(item.PropertyName, new Items<PropertyValidator>() { item });
            else
                _rulesByPropertyName[item.PropertyName].Add(item);
        }
        private void RemoveFromIndex(PropertyValidator item) {
            if (_rulesByPropertyName.ContainsKey(item.PropertyName)) {
                _rulesByPropertyName[item.PropertyName].Remove(item);
                if (_rulesByPropertyName[item.PropertyName].Count < 1)
                    _rulesByPropertyName.Remove(item.PropertyName);
            }
        }
        private IEnumerable<PropertyValidator> GetErrors(object model, Items<PropertyValidator> rules) {
            foreach (PropertyValidator rule in rules) {
                if (rule.IsValid(model)) continue;
                yield return rule;
            }
        }
        #endregion
        #region -=INotifyDataErrorInfo members=-
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                // return all errors
                foreach (KeyValuePair<string, Items<PropertyValidator>> pair in _currentErrors) {
                    foreach (PropertyValidator validator in pair.Value)
                        yield return validator.Error;
                }
            } else if (_currentErrors.ContainsKey(propertyName)) {
                Items<PropertyValidator> errors = _currentErrors[propertyName];
                foreach (PropertyValidator validator in errors)
                    yield return validator.Error;
            }
        }
        public bool HasErrors { get; private set; }
        private void OnErrorsChanged(string propertyName) {
            EventHandler<DataErrorsChangedEventArgs> handler = this.ErrorsChanged;
            if (handler != null)
                handler(this, new DataErrorsChangedEventArgs(propertyName));
        }
        #endregion
    }
}
