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
        private sealed class Trigger {
            /// <summary>
            /// Trigger condition.
            /// </summary>
            private readonly Predicate<TModel> condition = null;
            /// <summary>
            /// Trigger action.
            /// </summary>
            private readonly Action<TModel> action = null;
            /// <summary>
            /// Creates new unconditional trigger.
            /// </summary>
            /// <param name="action"></param>
            /// <param name="condition">Condition that is evaluated before trigger action is invoked.</param>
            public Trigger(Action<TModel> action, Predicate<TModel> condition = null) {
                this.action = action;
                this.condition = condition;
            }
            /// <summary>
            /// Invokes the trigger.
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            public bool Invoke(TModel model) {
                if (condition == null || condition(model)) {
                    action(model);
                    return true;
                }
                return false;
            }
        }
        #endregion
        private TModel model = null;
        /// <summary>
        /// Model object being observed.
        /// </summary>
        public TModel Model {
            get { return this.model; }
            set {
                bool modelSwitched = model != null;
                if (model != null) 
                    model.PropertyChanged -= Model_PropertyChanged;
                model = value;
                if (model != null) 
                    model.PropertyChanged += Model_PropertyChanged;
                if (modelSwitched)
                    TriggerAll(model);
            }
        }
        private ConcurrentDictionary<string, Items<Trigger>> triggersByProperty = new ConcurrentDictionary<string, Items<Trigger>>();
        /// <summary>
        /// Creates new object instance.
        /// </summary>
        public PropertyTriggers() { }
        /// <summary>
        /// Adds trigger for property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="action">Trigger action to be invoked.</param>
        /// <param name="condition">Trigger condition.</param>
        public void Add(string propertyName, Action<TModel> action, Predicate<TModel> condition = null){
            Items<Trigger> triggers = triggersByProperty.AddOrUpdate(propertyName, new Items<Trigger>(), (key, value) => value);
            triggers.Add(new Trigger(action, condition));
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs args) {
            try {
                Items<Trigger> triggers = null;
                TModel model = (TModel)sender;
                if (triggersByProperty.TryGetValue(args.PropertyName, out triggers)) {
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
            foreach (Items<Trigger> triggers in triggersByProperty.Values) {
                foreach (Trigger trigger in triggers)
                    trigger.Invoke(model);
            }
        }

        #region -=IDisposable members=-
        private bool isDisposed = false;
        /// <see cref="System.IDisposable.Dispose"/>
        public void Dispose() {
            Dispose(true);
        }
        private void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    if (model != null) {
                        model.PropertyChanged -= Model_PropertyChanged;
                        model = null;
                    }
                }
                isDisposed = true;
            }
        }
        #endregion
    }
}
