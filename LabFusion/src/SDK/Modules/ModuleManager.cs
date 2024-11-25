using LabFusion.Utilities;

namespace LabFusion.SDK.Modules;

public static class ModuleManager
{
    private static readonly List<Module> _modules = new();

    public static List<Module> Modules => _modules;

    /// <summary>
    /// Registers a module of the given type.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    public static void RegisterModule<TModule>() where TModule : Module
    {
        var module = Activator.CreateInstance<TModule>();

        LogDescription(module);

        _modules.Add(module);
        module.Register();
    }

    private static void LogDescription(Module module)
    {
        FusionLogger.Log("--==== Loaded Fusion Module ====--", module.Color);

        FusionLogger.Log($"{module.Name} - v{module.Version}");

        FusionLogger.Log($"by {module.Author}");

        FusionLogger.Log("--=============================--", module.Color);
    }
}