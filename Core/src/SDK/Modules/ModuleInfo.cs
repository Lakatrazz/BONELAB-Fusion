using System;

namespace LabFusion.SDK.Modules
{
    /// <summary>
    /// An assembly attribute pointing towards the Fusion module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleInfo : Attribute {
        public readonly Type moduleType;
        public readonly string name, author, version, abbreviation;
        public readonly bool autoRegister;
        public readonly ConsoleColor color;

        public ModuleInfo(Type moduleType, string name, string version = null, string author = null, string abbreviation = null, bool autoRegister = true)
            : this(moduleType, name, version, author, abbreviation, autoRegister, ConsoleColor.Magenta) { }

        public ModuleInfo(Type moduleType, string name, string version = null, string author = null, string abbreviation = null, bool autoRegister = true, ConsoleColor color = ConsoleColor.Magenta) {
            this.moduleType = moduleType;
            this.name = name;
            this.author = author;
            this.version = version;

            if (version == null)
                this.version = "0.0.0";

            if (author == null)
                this.author = "Unknown";

            this.abbreviation = abbreviation;
            this.autoRegister = autoRegister;
            this.color = color;
        }  
    }
}
