using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Defines objects ability to notify its IsChanged status change.
    /// </summary>
    public interface INotifyObjectChanged {
        /// <summary>
        /// Triggered when objects IsChanged property changes.
        /// </summary>
        event EventHandler ObjectChanged;
        /// <summary>
        /// True if object state is changed, false otherwise.
        /// </summary>
        bool IsChanged { get; }
        /// <summary>
        /// Accepts changed data so that object enters not changed state (IsChanged = false).
        /// </summary>
        void AcceptChanges();
    }
}
