using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa.Modularity {
    /// <summary>
    /// Defines a catalog of modules.
    /// </summary>
    public class Catalog {
        private readonly HashSet<ModuleInfo> _modules = new HashSet<ModuleInfo>();
        /// <summary>
        /// Adds module to the catalog.
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public ModuleInfo Add(ModuleInfo module) {
            if (module == null)
                throw new ArgumentNullException(nameof(module));
            if (_modules.Contains(module))
                throw new ArgumentException("Module already in catalog.", nameof(module));
            _modules.Add(module);
            return module;
        }
        /// <summary>
        /// Load modules marked with immediate loading mode.
        /// </summary>
        /// <returns></returns>
        public async Task Load() {
            foreach(var module in _modules) {
                if (module.LoadingMode != ModuleInfo.ModuleLoadingMode.Immediate) continue;
                if (module.IsLoaded) continue;
                await module.Load();
            }
        }
        /// <summary>
        /// Loads module with specified name.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public async Task Load(string moduleName) {
            var module = (from item in _modules
                          where string.Compare(moduleName, item.Name) == 0
                          select item).FirstOrDefault<ModuleInfo>();
            if (module == null)
                throw new InvalidOperationException($"Module [{moduleName}] not found in catalog.");
            await module.Load();
        }
        /// <summary>
        /// Load all modules in catalog.
        /// </summary>
        /// <returns></returns>
        public async Task LoadAll() {
            foreach(var module in _modules) {
                if (module.IsLoaded) continue;
                await module.Load();
            }
        }
    }
}
