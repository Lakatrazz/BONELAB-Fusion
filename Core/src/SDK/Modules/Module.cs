using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Representation;

namespace LabFusion.SDK.Modules {
    /// <summary>
    /// The class to inherit from when creating a Fusion module.
    /// </summary>
    public abstract class Module {
        /// <summary>
        /// Logger for logging info from modules.
        /// </summary>
        public ModuleLogger LoggerInstance { get; internal set; }

        // Called internally when a module is setup
        internal void ModuleLoaded(ModuleInfo info) {
            string name = info.name;
            if (!string.IsNullOrWhiteSpace(info.abbreviation))
                name = info.abbreviation;

            LoggerInstance = new ModuleLogger(name);

            OnModuleLoaded();
        }

        /// <summary>
        /// Called when the module is initially loaded.
        /// </summary>
        public virtual void OnModuleLoaded() { }
    }
}
