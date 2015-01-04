using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Append description to the value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class Descriptor<TValue> : IViewModel {
        public TValue Value { get; private set; }
        /// <summary>
        /// Item description.
        /// </summary>
        public string Description { get; protected set; }
        /// <summary>
        /// Creates new instance of descriptor class.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="description"></param>
        public Descriptor(TValue value, string description) {
            this.Value = value;
            this.Description = description;
        }
        #region -=IViewModel members=-
        object IViewModel.GetModel() {
            return this.Value;
        }
        void IViewModel.SetModel(object model) {
            return; //NOTE: Descriptor value is readonly.
        }
        #endregion
        #region -=Static members=-
        /// <summary>
        /// Returns model.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static implicit operator TValue(Descriptor<TValue> descriptor) {
            return descriptor.Value;
        }
        #endregion
    }
}
