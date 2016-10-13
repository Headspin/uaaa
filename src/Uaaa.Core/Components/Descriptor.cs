namespace Uaaa {
    /// <summary>
    /// Defines descriptor object.
    /// </summary>
    public interface IDescriptor {
        /// <summary>
        /// Object label.
        /// </summary>
        string Label { get; }
        /// <summary>
        /// Object description.
        /// </summary>
        string Description { get; }
    }
    /// <summary>
    /// Append description to the value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class Descriptor<TValue> : IViewModel {
        /// <summary>
        /// Value object which is described by the descriptor.
        /// </summary>
        public TValue Value { get; private set; }
        /// <see cref="Uaaa.IDescriptor.Label"/>
        public string Label { get; protected set; }
        /// <see cref="Uaaa.IDescriptor.Description"/>
        public string Description { get; protected set; }
        /// <summary>
        /// Creates new instance of descriptor class.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="label"><see cref="Uaaa.IDescriptor.Label"/></param>
        /// <param name="description"><see cref="Uaaa.IDescriptor.Description"/></param>
        public Descriptor(TValue value, string label, string description = null) {
            this.Value = value;
            this.Label = label;
            this.Description = description;
        }
        #region -=Public methods=-
        /// <summary>
        /// Returns descriptors label.
        /// </summary>
        /// <returns></returns>
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
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static implicit operator TValue(Descriptor<TValue> descriptor) {
            return descriptor.Value;
        }
        #endregion
    }
}
