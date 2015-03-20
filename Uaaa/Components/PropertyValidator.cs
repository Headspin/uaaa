using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Property data validator.
    /// </summary>
    public abstract class PropertyValidator {
        public readonly string PropertyName = null;
        public readonly string Error = null;
        public PropertyValidator(string propertyName, string error) {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");
            if (string.IsNullOrEmpty(error))
                throw new ArgumentNullException("error");
            this.PropertyName = propertyName;
            this.Error = error;
        }
        public abstract bool IsValid(object model);
    }
}
