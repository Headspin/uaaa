using System;
using System.Collections.Generic;

namespace Uaaa {
    /// <summary>
    /// Manages object changes.
    /// </summary>
    public class ChangeManager : INotifyObjectChanged {
        #region -=Properties/Fields=-
        private readonly HashSet<INotifyObjectChanged> trackedObjects = new HashSet<INotifyObjectChanged>();
        private readonly HashSet<INotifyObjectChanged> changedObjects = new HashSet<INotifyObjectChanged>();
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
            if (trackedObjects.Contains(trackedObject)) return;
            trackedObjects.Add(trackedObject);
            trackedObject.ObjectChanged += TrackedObject_ObjectChanged;
            if (trackedObject.IsChanged) {
                changedObjects.Add(trackedObject);
                this.IsChanged = true;
            }
        }
        /// <summary>
        /// Removes object from change manager instance.
        /// </summary>
        /// <param name="trackedObject"></param>
        public void Remove(INotifyObjectChanged trackedObject) {
            if (trackedObject == null) return;
            if (!trackedObjects.Contains(trackedObject)) return;
            trackedObjects.Remove(trackedObject);
            if (changedObjects.Contains(trackedObject)) {
                changedObjects.Remove(trackedObject);
                this.IsChanged = changedObjects.Count > 0;
            }
            
        }
        /// <summary>
        /// Resets change manager be clearing all tracking data.
        /// </summary>
        public void Reset() {
            foreach (INotifyObjectChanged trackedObject in trackedObjects)
                trackedObject.ObjectChanged -= TrackedObject_ObjectChanged;
            trackedObjects.Clear();
            changedObjects.Clear();
            this.IsChanged = false;
        }
        #endregion
        #region -=Private methods=-
        private void TrackedObject_ObjectChanged(object sender, EventArgs args) {
            INotifyObjectChanged trackedObject = sender as INotifyObjectChanged;
            if (trackedObject == null) return;
            if (trackedObject.IsChanged && !changedObjects.Contains(trackedObject)) {
                changedObjects.Add(trackedObject);
                this.IsChanged = true;
            } else if (!trackedObject.IsChanged && changedObjects.Contains(trackedObject)) {
                changedObjects.Remove(trackedObject);
                this.IsChanged = changedObjects.Count > 0;
            }
        }
        #endregion
        #region -=INotifyObjectChanged members=-
        /// <summary>
        /// INotifyObjectChanged.ObjectChanged implementation.
        /// </summary>
        public event EventHandler ObjectChanged;
        private bool isChanged = false;
        /// <summary>
        /// TRUE if object changed, FALSE otherwise.
        /// </summary>
        public bool IsChanged {
            get { return isChanged; }
            private set {
                if (isChanged == value) return;
                isChanged = value;
                OnObjectChanged();
            }
        }
        /// <summary>
        /// Accepts changes on all tracked objects.
        /// </summary>
        public void AcceptChanges() {
            foreach (INotifyObjectChanged item in trackedObjects) 
                item.AcceptChanges();
        }

        private void OnObjectChanged() {
            this.ObjectChanged?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}
