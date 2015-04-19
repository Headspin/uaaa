using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Provides model business rule validation checking.
    /// </summary>
    public abstract class BusinessRule {
        public readonly string Error = null;
        protected BusinessRule(string error) {
            if (string.IsNullOrEmpty(error))
                throw new ArgumentNullException("error");
            this.Error = error;
        }
        public abstract bool IsValid(object model);
    }
}
