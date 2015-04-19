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
    public class BusinessRulesChecker : INotifyDataErrorInfo {
		private Dictionary<string, Items<BusinessRule>> _rulesByPropertyName = new Dictionary<string, Items<BusinessRule>>();
		private Dictionary<string, Items<BusinessRule>> _currentErrors = new Dictionary<string, Items<BusinessRule>>();
        public BusinessRulesChecker() { }
        #region -=Public methods=-
        /// <summary>
        /// Adds validator to the manager.
        /// </summary>
        /// <param name="rule"></param>
		/// <param name = "propertyName"></param>
		public void Add(BusinessRule rule, string propertyName = "") {
            AddToIndex(rule, propertyName);
        }
        /// <summary>
        /// Removes validator from manager.
        /// </summary>
        /// <param name="rule"></param>
		public void Remove(BusinessRule rule) {
            RemoveFromIndex(rule);
        }
        public bool IsValid(object model, string propertyName = "") {
            bool isValid = true;
            if (string.IsNullOrEmpty(propertyName)) {
                #region -=Check all rules=-
				foreach (KeyValuePair<string, Items<BusinessRule>> pair in _rulesByPropertyName) {
					Items<BusinessRule> errors = new Items<BusinessRule>();
					foreach (BusinessRule rule in GetErrors(model, pair.Value)) {
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
				Items<BusinessRule> errors = new Items<BusinessRule>();
				foreach (BusinessRule rule in GetErrors(model, _rulesByPropertyName[propertyName])) {
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
		private void AddToIndex(BusinessRule item, string propertyName = "") {
			if (!_rulesByPropertyName.ContainsKey(propertyName))
				_rulesByPropertyName.Add(propertyName, new Items<BusinessRule>() { item });
            else 
				_rulesByPropertyName[propertyName].Add(item);
        }
		private void RemoveFromIndex(BusinessRule item, string propertyName = "") {
			if (_rulesByPropertyName.ContainsKey(propertyName)) {
                _rulesByPropertyName[propertyName].Remove(item);
				if (_rulesByPropertyName[propertyName].Count < 1)
					_rulesByPropertyName.Remove(propertyName);
            }
        }
		private IEnumerable<BusinessRule> GetErrors(object model, Items<BusinessRule> rules) {
			foreach (BusinessRule rule in rules) {
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
				foreach (KeyValuePair<string, Items<BusinessRule>> pair in _currentErrors) {
					foreach (BusinessRule validator in pair.Value)
                        yield return validator.Error;
                }
            } else if (_currentErrors.ContainsKey(propertyName)) {
				Items<BusinessRule> errors = _currentErrors[propertyName];
				foreach (BusinessRule validator in errors)
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
