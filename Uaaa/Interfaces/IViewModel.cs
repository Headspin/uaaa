using System;
using System.Collections.Generic;
using System.Linq;
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
}
