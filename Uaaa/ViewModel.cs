using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// ViewModel base class that support property change notifications.
    /// </summary>
    public class ViewModel<TModel> : IViewModel, IModel, INotifyPropertyChanged where TModel : Model {
        #region -=Properties/Fields=-
        protected readonly PropertySetter Property = null;
        private TModel _model = default(TModel);
        public virtual TModel Model {
            get { return _model; }
            set {
                if (Property.Set<TModel>(ref _model, value, canChange: () => !this.IsReadonly || _model == default(TModel)))
                    OnModelChanged();
            }
        }
        /// <summary>
        /// ViewModel is flagged as ReadOnly (model can only be set once).
        /// </summary>
        protected bool IsReadonly { get; set; }
        #endregion
        #region -=Constructors=-
        public ViewModel() {
            this.Property = new PropertySetter(this);
        }
        public ViewModel(TModel model)
            : this() {
            this.Model = model;
        }
        #endregion
        #region -=Protected methods=-
        /// <summary>
        /// Triggers optional actions after model value changed.
        /// </summary>
        protected virtual void OnModelChanged() { }
        #endregion
        #region -=IModel members=-
        public event PropertyChangedEventHandler PropertyChanged;
        void IModel.RaisePropertyChanged(string propertyName) {
            RaisePropertyChanged(propertyName);
        }
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            if (string.IsNullOrEmpty(propertyName)) return;
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region -=IViewModel members=-
        object IViewModel.GetModel() {
            return _model;
        }
        void IViewModel.SetModel(object model) {
            this.Model = model as TModel;
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
