using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Append description to the model.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class Descriptor<TModel> : ViewModel<TModel> where TModel : Model {
        /// <summary>
        /// Item description.
        /// </summary>
        public string Description { get; protected set; }
        /// <summary>
        /// Creates new instance of descriptor class.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="description"></param>
        public Descriptor(TModel item, string description)
            : base(item) {
            this.IsReadonly = true;
            this.Description = description;
        }
    }
}
