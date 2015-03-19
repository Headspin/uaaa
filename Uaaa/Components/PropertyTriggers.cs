using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Handles TModel property changes and invokes appropriate triggers for given property.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public sealed class PropertyTriggers<TModel> : IDisposable where TModel : class, INotifyPropertyChanged {
        #region -=Support types=-
        /// <summary>
        /// Defines condition and action that should be executed if condition is met.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        private sealed class Trigger {
            /// <summary>
            /// Trigger condition.
            /// </summary>
            private readonly Predicate<TModel> _condition = null;
            /// <summary>
            /// Trigger action.
            /// </summary>
            private readonly Action<TModel> _action = null;
            /// <summary>
            /// Creates new unconditional trigger.
            /// </summary>
            /// <param name="action"></param>
            public Trigger(Action<TModel> action, Predicate<TModel> condition = null) {
                _action = action;
                _condition = condition;
            }
            /// <summary>
            /// Invokes the trigger.
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            public bool Invoke(TModel model) {
                if (_condition == null || _condition(model)) {
                    _action(model);
                    return true;
                }
                return false;
            }
        }
        #endregion
        private TModel _model = null;
        public TModel Model {
            get { return _model; }
            set {
                bool modelSwitched = _model != null;
                if (_model != null) 
                    _model.PropertyChanged -= Model_PropertyChanged;
                _model = value;
                if (_model != null) 
                    _model.PropertyChanged += Model_PropertyChanged;
                if (modelSwitched)
                    TriggerAll(_model);
            }
        }
        private ConcurrentDictionary<string, Items<Trigger>> _triggersByProperty = new ConcurrentDictionary<string, Items<Trigger>>();
        public PropertyTriggers() { }
        /// <summary>
        /// Adds trigger for property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="action">Trigger action to be invoked.</param>
        /// <param name="condition">Trigger condition.</param>
        public void Add(string propertyName, Action<TModel> action, Predicate<TModel> condition = null){
            Items<Trigger> triggers = _triggersByProperty.AddOrUpdate(propertyName, new Items<Trigger>(), (key, value) => value);
            triggers.Add(new Trigger(action, condition));
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs args) {
            try {
                Items<Trigger> triggers = null;
                TModel model = (TModel)sender;
                if (_triggersByProperty.TryGetValue(args.PropertyName, out triggers)) {
                    foreach (Trigger trigger in triggers)
                        trigger.Invoke(model);
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Invokes all registered triggers.
        /// </summary>
        /// <param name="model"></param>
        private void TriggerAll(TModel model) {
            foreach (Items<Trigger> triggers in _triggersByProperty.Values) {
                foreach (Trigger trigger in triggers)
                    trigger.Invoke(model);
            }
        }

        #region -=IDisposable members=-
        private bool _isDisposed = false;
        public void Dispose() {
            Dispose(true);
        }
        private void Dispose(bool disposing) {
            if (!_isDisposed) {
                if (disposing) {
                    if (_model != null) {
                        _model.PropertyChanged -= Model_PropertyChanged;
                        _model = null;
                    }
                }
                _isDisposed = true;
            }
        }
        #endregion
    }
}
