using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Defines common Model intefrace
    /// </summary>
    public interface IModel : INotifyPropertyChanged {
        /// <summary>
        /// Raises PropertyChanged event for provided property name.
        /// </summary>
        /// <param name="propertyName"></param>
        void RaisePropertyChanged(string propertyName);
    }
}
