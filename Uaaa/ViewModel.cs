using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Defines common ViewModel interface.
    /// </summary>
    interface IViewModel {
        /// <summary>
        /// Returns model value.
        /// </summary>
        /// <returns></returns>
        object GetModel();
        /// <summary>
        /// Sets model value.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        void SetModel(object model);
    }
    /// <summary>
    /// ViewModel base class that support property change notifications.
    /// </summary>
    public class ViewModel<TModel> : IViewModel, IModel, INotifyPropertyChanged, IDisposable where TModel : Model {
        #region -=Properties/Fields=-
        /// <summary>
        /// PropertySetter instance for setting property values.
        /// </summary>
        protected readonly PropertySetter Property = null;
        /// <summary>
        /// Model property triggers.
        /// </summary>
        protected readonly PropertyTriggers<TModel> Triggers = new PropertyTriggers<TModel>();
        private TModel model = null;
        /// <summary>
        /// Model object.
        /// </summary>
        public virtual TModel Model {
            get { return model; }
            set {
                if (Property.Set<TModel>(ref model, value, canChange: () => !this.IsReadonly || model == default(TModel)))
                    OnModelChanged();
            }
        }
        /// <summary>
        /// ViewModel is flagged as ReadOnly (model can only be set once).
        /// </summary>
        protected bool IsReadonly { get; set; }
        #endregion
        #region -=Constructors=-
        /// <summary>
        /// Creates new object instance.
        /// </summary>
        public ViewModel() {
            this.Property = new PropertySetter(this);
        }
        /// <summary>
        /// Creates new object instance.
        /// </summary>
        /// <param name="model"></param>
        public ViewModel(TModel model)
            : this() {
			SetModel (model);
        }
        #endregion
        #region -=Protected methods=-
        /// <summary>
        /// Triggers optional actions after model value changed.
        /// </summary>
        protected virtual void OnModelChanged() {
            this.Triggers.Model = model;
        }
        #endregion
		#region -=Private methods=-
		private void SetModel(TModel model){
			this.Model = model;
		}
		#endregion
        #region -=IModel members=-
        /// <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;
        void IModel.RaisePropertyChanged(string propertyName) {
            RaisePropertyChanged(propertyName);
        }
        /// <summary>
        /// Raises PropertyChanged event for specific property name.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            if (string.IsNullOrEmpty(propertyName)) return;
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region -=IViewModel members=-
        object IViewModel.GetModel() {
            return model;
        }
        void IViewModel.SetModel(object model) {
            this.Model = model as TModel;
        }
        #endregion
        #region -=IDisposable members=-
        private bool isDisposed = false;
        /// <see cref="System.IDisposable.Dispose()"/>
        public void Dispose() {
            Dispose(true);
        }
        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    this.Triggers.Dispose();
                }
                isDisposed = true;
            }
        }
        #endregion
        #region -=Static members=-
        /// <summary>
        /// Returns model.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static implicit operator TModel(ViewModel<TModel> viewModel) {
            return viewModel.Model;
        }
        #endregion


    }
}
