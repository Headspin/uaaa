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
    public sealed class PropertyHandler<TModel> : IDisposable where TModel : INotifyPropertyChanged {
        private INotifyPropertyChanged _model = null;
        private ConcurrentDictionary<string, Items<Trigger<TModel>>> _triggersByProperty = new ConcurrentDictionary<string, Items<Trigger<TModel>>>();
        public PropertyHandler(TModel model) {
            _model.PropertyChanged += Model_PropertyChanged;
        }
        public void AddTrigger(Trigger<TModel> trigger, string propertyName) {
            Items<Trigger<TModel>> triggers = _triggersByProperty.AddOrUpdate(propertyName, new Items<Trigger<TModel>>(), (key, value) => value);
            triggers.Add(trigger);
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs args) {
            try {
                Items<Trigger<TModel>> triggers = null;
                TModel model = (TModel)sender;
                if (_triggersByProperty.TryGetValue(args.PropertyName, out triggers)) {
                    foreach (Trigger<TModel> trigger in triggers)
                        trigger.Invoke(model);
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
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
