namespace LabFusion.SDK.Modules;

/// <summary>
/// The base class for a Fusion module.
/// </summary>
public abstract class Module
{
    public abstract string Name { get; }
    public virtual string Author { get; } = "Unknown";
    public virtual Version Version { get; } = new();

    public virtual ConsoleColor Color { get; } = ConsoleColor.Magenta;

    /// <summary>
    /// The logger that the module can use to log information.
    /// </summary>
    public ModuleLogger LoggerInstance { get; private set; }

    internal void Register()
    {
        LoggerInstance = new ModuleLogger(Name);

        OnModuleRegistered();
    }

    /// <summary>
    /// Called when the module is initially registered. Use this to hook into Fusion functions, register Module Messages, etc.
    /// </summary>
    protected virtual void OnModuleRegistered() { }

    /// <summary>
    /// Called when the module is unregistered. Use this to clean up anything affected by the module.
    /// </summary>
    protected virtual void OnModuleUnregistered() { }
}