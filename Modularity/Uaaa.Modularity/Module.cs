using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa.Modularity {
    /// <summary>
    /// Module definition.
    /// </summary>
    internal interface IModule {
        /// <summary>
        /// Performs module initialization.
        /// </summary>
        /// <returns></returns>
        Task Initialize();
    }

    /// <summary>
    /// Module definition.
    /// </summary>
    public abstract class Module : IModule {
        public enum ModuleState {
            /// <summary>
            /// Module is loaded and ready to be initialized.
            /// </summary>
            Ready,
            /// <summary>
            /// Module initialization in progress.
            /// </summary>
            Initializing,
            /// <summary>
            /// Module initialization finished.
            /// </summary>
            Initialized
        }
        public ModuleState State { get; private set; } = ModuleState.Ready;
        #region -=IModule members=-
		private readonly object _initializeSync = new object ();
        Task IModule.Initialize() {
            lock (_initializeSync) {
                if (this.State != ModuleState.Ready) return Task.FromResult<bool>(false);
                this.State = ModuleState.Initializing;
            }
            try {
                return this.Initialize();
            } finally {
                this.State = ModuleState.Initialized;
            }
        }
        #endregion
        #region -=Protected methods=-
        /// <summary>
        /// Initializes the module.
        /// </summary>
        /// <returns></returns>
        protected abstract Task Initialize();
        #endregion
    }
}
