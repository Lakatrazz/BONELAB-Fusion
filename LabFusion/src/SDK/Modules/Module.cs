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
    /// Called when the module is initially registered. Use this to hook into Fusion functions, register Module Messages, etc.
    /// </summary>
    public virtual void OnModuleRegistered() { }

    /// <summary>
    /// Called when the module is unregistered. Use this to clean up anything affected by the module.
    /// </summary>
    public virtual void OnModuleUnregistered() { }
}