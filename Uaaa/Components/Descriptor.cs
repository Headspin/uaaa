using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    public interface IDescriptor {
        string Label { get; }
        string Description { get; }
    }
    /// <summary>
    /// Append description to the value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class Descriptor<TValue> : IViewModel {
        public TValue Value { get; private set; }
        public virtual string Label { get; protected set; }
        /// <summary>
        /// Item description.
        /// </summary>
        public virtual string Description { get; protected set; }
        /// <summary>
        /// Creates new instance of descriptor class.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="description"></param>
        public Descriptor(TValue value, string label, string description = null) {
            this.Value = value;
            this.Label = label;
            this.Description = description;
        }
        #region -=Public methods=-
        public override string ToString() {
            return this.Label;
        }
        #endregion
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
