using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa.Modularity {

    public sealed class ModuleInfo {
        /// <summary>
        /// Defines module loading modes.
        /// </summary>
        public enum ModuleLoadingMode {
            /// <summary>
            /// Module is loaded immediatly.
            /// </summary>
            Immediate,
            /// <summary>
            /// Module loading is deferred to later time.
            /// </summary>
            Deferred
        }
        #region -=Instance members=-
        private readonly int _hashCode = 0;
        public readonly string Name = null;
        public readonly string AssemblyName = null;
        public readonly string TypeName = null;
        /// <summary>
        /// Returns selected module loading mode.
        /// </summary>
        public ModuleLoadingMode LoadingMode { get; private set; } = ModuleLoadingMode.Immediate;
        /// <summary>
        /// Module already loaded.
        /// </summary>
        public bool IsLoaded { get; private set; } = false;

        private readonly HashSet<ModuleInfo> _dependencies = new HashSet<ModuleInfo>();

        private ModuleInfo(string name, string assemblyName, string typeName) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(assemblyName))
                throw new ArgumentNullException(nameof(assemblyName));
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException(nameof(typeName));
            this.AssemblyName = assemblyName;
            this.TypeName = typeName;
            _hashCode = this.AssemblyName.GetHashCode() ^ this.TypeName.GetHashCode();
        }
        /// <summary>
        /// Defines module dependencies.
        /// </summary>
        /// <param name="modules"></param>
        /// <returns></returns>
        public ModuleInfo DependsOn(params ModuleInfo[] modules) {
            if (modules == null)
                throw new ArgumentNullException(nameof(modules));
            foreach (var module in modules) {
                if (module.Equals(this)) continue;
                if (_dependencies.Contains(module)) continue;
                _dependencies.Add(module);
            }
            return this;
        }
        /// <summary>
        /// Returns module dependencies list.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ModuleInfo> GetDependencies() {
            return _dependencies.AsEnumerable<ModuleInfo>();
        }
        /// <summary>
        /// Set deferred initialization mode.
        /// </summary>
        /// <returns></returns>
        public ModuleInfo LoadDefered() {
            this.LoadingMode = ModuleLoadingMode.Deferred;
            return this;
        }

        private readonly object _loadSync = new object();
        public async Task Load() {
            lock (_loadSync) {
                if (this.IsLoaded) return;
                this.IsLoaded = true;
            }
            // load dependencies.
            foreach (var module in _dependencies) {
                if (module.IsLoaded) continue;
                await module.Load();
            }
            // load module and initialize.
            _loadedModuleAssemblies.GetOrAdd(this.AssemblyName,
                assemblyName => Assembly.LoadFrom(assemblyName)); // no need to load if already loaded.
            Type moduleType = Type.GetType(this.TypeName);
            var loadedModule = ServiceLocator.Current.GetInstance(moduleType) as IModule;
            if (loadedModule != null)
                await loadedModule.Initialize(); // initialize if module class instance of type IModule.
            // signal module loaded.
            ModuleInfo.Loaded?.Invoke(this, this.Name);
        }


        public override bool Equals(object obj) {
            ModuleInfo source = obj as ModuleInfo;
            if (source != null)
                return string.Compare(this.TypeName, source.TypeName) == 0
                    && string.Compare(this.AssemblyName, source.AssemblyName) == 0;
            return false;
        }

        public override int GetHashCode() {
            return _hashCode;
        }
        #endregion
        #region -=Static members=-
        private static ConcurrentDictionary<string, Assembly> _loadedModuleAssemblies = new ConcurrentDictionary<string, Assembly>();
        /// <summary>
        /// Event is raised after module loading completed.
        /// </summary>
        public static event EventHandler<string> Loaded;
        /// <summary>
        /// Registers module information.
        /// </summary>
        /// <param name="name">Name that uniquely identifies the module.</param>
        /// <param name="assemblyName">Module assembly name</param>
        /// <param name="typeName">Module typename (including containing namespace).</param>
        /// <returns></returns>
        public static ModuleInfo Register(string name, string assemblyName, string typeName) {
            return new ModuleInfo(name, assemblyName, typeName);
        }
        #endregion
    }
}
