using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Manages object changes.
    /// </summary>
    public class ChangeManager : INotifyObjectChanged {
        #region -=Properties/Fields=-
        /// <summary>
        /// NOTE: Since hashset is not available in WP7 profile dictionary is used instead. Values are just booleans that do not mean anything.
        ///       HashSet should be used if it becomes available in future portable framework profiles.
        /// </summary>
        private readonly Dictionary<INotifyObjectChanged, bool> _trackedObjects = new Dictionary<INotifyObjectChanged, bool>();
        private readonly Dictionary<INotifyObjectChanged, bool> _changedObjects = new Dictionary<INotifyObjectChanged, bool>();

        #endregion
        #region -=Constructors=-
        /// <summary>
        /// Creates new instance of ChangeManager.
        /// </summary>
        public ChangeManager() { }
        #endregion
        #region -=Public methods=-
        /// <summary>
        /// Adds object to be tracked by change manager instance.
        /// </summary>
        /// <param name="trackedObject"></param>
        public void Track(INotifyObjectChanged trackedObject) {
            if (_trackedObjects.ContainsKey(trackedObject)) return;
            _trackedObjects.Add(trackedObject, false);
            trackedObject.ObjectChanged += TrackedObject_ObjectChanged;
            if (trackedObject.IsChanged) {
                _changedObjects.Add(trackedObject, true);
                this.IsChanged = true;
            }
        }
        /// <summary>
        /// Resets change manager be clearing all tracking data.
        /// </summary>
        public void Reset() {
            foreach (KeyValuePair<INotifyObjectChanged, bool> pair in _trackedObjects)
                pair.Key.ObjectChanged -= TrackedObject_ObjectChanged;
            _trackedObjects.Clear();
            _changedObjects.Clear();
            this.IsChanged = false;
        }
        #endregion
        #region -=Private methods=-
        private void TrackedObject_ObjectChanged(object sender, EventArgs args) {
            INotifyObjectChanged trackedObject = sender as INotifyObjectChanged;
            if (trackedObject == null) return;
            if (trackedObject.IsChanged && !_changedObjects.ContainsKey(trackedObject)) {
                _changedObjects.Add(trackedObject, true);
                this.IsChanged = true;
            } else if (!trackedObject.IsChanged && _changedObjects.ContainsKey(trackedObject)) {
                _changedObjects.Remove(trackedObject);
                this.IsChanged = _changedObjects.Count > 0;
            }
        }
        #endregion
        #region -=INotifyObjectChanged members=-
        /// <summary>
        /// INotifyObjectChanged.ObjectChanged implementation.
        /// </summary>
        public event EventHandler ObjectChanged;
        private bool _isChanged = false;
        /// <summary>
        /// TRUE if object changed, FALSE otherwise.
        /// </summary>
        public bool IsChanged {
            get { return _isChanged; }
            private set {
                if (_isChanged == value) return;
                _isChanged = value;
                OnObjectChanged();
            }
        }

        private void OnObjectChanged() {
            if (this.ObjectChanged != null)
                this.ObjectChanged(this, new EventArgs());
        }
        #endregion
    }
}
