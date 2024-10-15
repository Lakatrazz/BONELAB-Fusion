namespace LabFusion.SDK.Modules;

/// <summary>
/// The base class for a Fusion module.
/// </summary>
public abstract class Module
{
    /// <summary>
    /// The logger that the module can use to log information.
    /// </summary>
    public ModuleLogger LoggerInstance { get; internal set; }

    internal void Register(ModuleData moduleData)
    {
        string name = moduleData.Name;

        LoggerInstance = new ModuleLogger(name);

        OnModuleRegistered();
    }

    /// <summary>
    /// Called when the module is initially registered.
    /// </summary>
    public virtual void OnModuleRegistered() { }
}