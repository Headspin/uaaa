using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Implements INotifyDataErrorInfo and handles data validation by checking added business rules.
    /// </summary>
    public class BusinessRulesChecker : INotifyDataErrorInfo, INotifyPropertyChanged {
        private Dictionary<string, Items<BusinessRule>> rulesByPropertyName = new Dictionary<string, Items<BusinessRule>>();
        private Dictionary<string, Items<BusinessRule>> currentErrors = new Dictionary<string, Items<BusinessRule>>();
        /// <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Creates new object instance.
        /// </summary>
        public BusinessRulesChecker() { }

        #region -=Public methods=-

        /// <summary>
        /// Adds business rules to checker instance.
        /// Provide property name to bind business rule to specific model property.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name = "propertyName">Name of property which is validated by provided business rule. Use "" when business rule is not speficic to any model property.</param>
        public void Add(BusinessRule rule, string propertyName) {
            AddToIndex(rule, propertyName);
        }

        /// <summary>
        /// Removes business rule from checker instance.
        /// </summary>
        /// <param name="rule"></param>
        public void Remove(BusinessRule rule) {
            RemoveFromIndex(rule);
        }

        /// <summary>
        /// Checks business rules with provided model as context.
        /// Property name can be provided to check rules bound to that specific property.
        /// All rules are checked if property name not provided.
        /// </summary>
        /// <returns><c>true</c> if this instance is valid the specified model propertyName; otherwise, <c>false</c>.</returns>
        /// <param name="model">Model.</param>
        /// <param name="propertyName">Property name.</param>
        public bool IsValid(object model, string propertyName = "") {
            bool isValid = true;
            Items<string> errorsChangedProperties = new Items<string>();
            if (string.IsNullOrEmpty(propertyName)) {
                #region -=Check all rules=-
                foreach (KeyValuePair<string, Items<BusinessRule>> pair in rulesByPropertyName) {
                    Items<BusinessRule> errors = new Items<BusinessRule>();
                    foreach (BusinessRule rule in GetInvalidRules(model, pair.Value)) {
                        errors.Add(rule);
                        isValid = false;
                    }
                    bool errorsChanged = false;
                    if (errors.Count > 0) {
                        if (currentErrors.ContainsKey(pair.Key))
                            currentErrors[pair.Key] = errors;
                        else
                            currentErrors.Add(pair.Key, errors);
                        errorsChanged = true;
                    } else {
                        if (currentErrors.ContainsKey(pair.Key)) {
                            currentErrors.Remove(pair.Key);
                            errorsChanged = true;
                        }
                    }
                    if (errorsChanged)
                        errorsChangedProperties.Add(pair.Key);
                }
                #endregion
            } else if (rulesByPropertyName.ContainsKey(propertyName)) {
                #region -=Check property specific rules=-
                Items<BusinessRule> errors = new Items<BusinessRule>();
                foreach (BusinessRule rule in GetInvalidRules(model, rulesByPropertyName[propertyName])) {
                    errors.Add(rule);
                    isValid = false;
                }
                bool errorsChanged = false;
                if (errors.Count > 0) {
                    if (currentErrors.ContainsKey(propertyName))
                        currentErrors[propertyName] = errors;
                    else
                        currentErrors.Add(propertyName, errors);
                    errorsChanged = true;
                } else {
                    if (currentErrors.ContainsKey(propertyName)) {
                        currentErrors.Remove(propertyName);
                        errorsChanged = true;
                    }
                }
                if (errorsChanged)
                    errorsChangedProperties.Add(propertyName);
                #endregion
            }
            this.HasErrors = currentErrors.Count > 0;
            foreach (string property in errorsChangedProperties)
                OnErrorsChanged(property);
            return isValid;
        }

        #endregion

        #region -=Private methods=-
        private void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddToIndex(BusinessRule rule, string propertyName = "") {
            if (!rulesByPropertyName.ContainsKey(propertyName))
                rulesByPropertyName.Add(propertyName, new Items<BusinessRule>() { rule });
            else
                rulesByPropertyName[propertyName].Add(rule);
        }

        private void RemoveFromIndex(BusinessRule rule, string propertyName = "") {
            if (rulesByPropertyName.ContainsKey(propertyName)) {
                rulesByPropertyName[propertyName].Remove(rule);
                if (rulesByPropertyName[propertyName].Count < 1)
                    rulesByPropertyName.Remove(propertyName);
            }
        }

        private IEnumerable<BusinessRule> GetInvalidRules(object model, Items<BusinessRule> rules) {
            foreach (BusinessRule rule in rules) {
                if (rule.IsValid(model))
                    continue;
                yield return rule;
            }
        }

        #endregion

        #region -=INotifyDataErrorInfo members=-

        /// <summary>
        /// INotifyDataErrorInfo.ErrorsChanged event.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// INotifyDataErrorInfo.GetErrors(propertyName) implementation.
        /// </summary>
        /// <returns>The errors.</returns>
        /// <param name="propertyName">Property name.</param>
        public System.Collections.IEnumerable GetErrors(string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                // return all errors
                foreach (KeyValuePair<string, Items<BusinessRule>> pair in currentErrors) {
                    foreach (BusinessRule rule in pair.Value)
                        yield return rule.Error;
                }
            } else if (currentErrors.ContainsKey(propertyName)) {
                Items<BusinessRule> errors = currentErrors[propertyName];
                foreach (BusinessRule rule in errors)
                    yield return rule.Error;
            }
        }

        private bool hasErrors = false;
        /// <summary>
        /// INotifyDataErrorInfo.HasErrors property implementation.
        /// </summary>
        /// <value><c>true</c> if this instance has errors; otherwise, <c>false</c>.</value>
        public bool HasErrors {
            get { return hasErrors; }
            private set {
                if (hasErrors != value) {
                    hasErrors = value;
                    OnPropertyChanged(nameof(HasErrors));
                }
            }
        }

        private void OnErrorsChanged(string propertyName) {
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// INotifyDataErrorInfo extension methods.
    /// </summary>
    public static class NotifyDataErrorInfoExtensions {
        /// <summary>
        /// Returns errors items collection.
        /// </summary>
        /// <returns>The errors list.</returns>
        /// <param name="errorInfo">Error info.</param>
        /// <param name="propertyName">Propery name.</param>
        public static Items<string> GetErrorsCollection(this INotifyDataErrorInfo errorInfo, string propertyName) {
            Items<string> errors = new Items<string>();
            foreach (var error in errorInfo.GetErrors(propertyName))
                errors.Add(error.ToString());
            return errors;
        }
    }
}
