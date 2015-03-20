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
        private readonly HashSet<INotifyObjectChanged> _trackedObjects = new HashSet<INotifyObjectChanged>();
        private readonly HashSet<INotifyObjectChanged> _changedObjects = new HashSet<INotifyObjectChanged>();
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
            if (trackedObject == null) return;
            if (_trackedObjects.Contains(trackedObject)) return;
            _trackedObjects.Add(trackedObject);
            trackedObject.ObjectChanged += TrackedObject_ObjectChanged;
            if (trackedObject.IsChanged) {
                _changedObjects.Add(trackedObject);
                this.IsChanged = true;
            }
        }
        /// <summary>
        /// Removes object from change manager instance.
        /// </summary>
        /// <param name="trackedObject"></param>
        public void Remove(INotifyObjectChanged trackedObject) {
            if (trackedObject == null) return;
            if (!_trackedObjects.Contains(trackedObject)) return;
            _trackedObjects.Remove(trackedObject);
            if (_changedObjects.Contains(trackedObject)) {
                _changedObjects.Remove(trackedObject);
                this.IsChanged = _changedObjects.Count > 0;
            }
            
        }
        /// <summary>
        /// Resets change manager be clearing all tracking data.
        /// </summary>
        public void Reset() {
            foreach (INotifyObjectChanged trackedObject in _trackedObjects)
                trackedObject.ObjectChanged -= TrackedObject_ObjectChanged;
            _trackedObjects.Clear();
            _changedObjects.Clear();
            this.IsChanged = false;
        }
        #endregion
        #region -=Private methods=-
        private void TrackedObject_ObjectChanged(object sender, EventArgs args) {
            INotifyObjectChanged trackedObject = sender as INotifyObjectChanged;
            if (trackedObject == null) return;
            if (trackedObject.IsChanged && !_changedObjects.Contains(trackedObject)) {
                _changedObjects.Add(trackedObject);
                this.IsChanged = true;
            } else if (!trackedObject.IsChanged && _changedObjects.Contains(trackedObject)) {
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
        /// <summary>
        /// Accepts changes on all tracked objects.
        /// </summary>
        public void AcceptChanges() {
            foreach (INotifyObjectChanged item in _trackedObjects) 
                item.AcceptChanges();
        }

        private void OnObjectChanged() {
            if (this.ObjectChanged != null)
                this.ObjectChanged(this, new EventArgs());
        }
        #endregion
    }
}
