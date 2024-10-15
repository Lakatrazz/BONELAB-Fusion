using LabFusion.Utilities;

namespace LabFusion.SDK.Modules;

public static class ModuleManager
{
    private static readonly List<Module> _modules = new();

    public static List<Module> Modules => _modules;

    /// <summary>
    /// Registers a module using the given ModuleData.
    /// </summary>
    /// <param name="moduleData"></param>
    public static void RegisterModule(ModuleData moduleData)
    {
        if (Activator.CreateInstance(moduleData.ModuleType) is not Module module)
        {
            FusionLogger.Error("Failed to create a Module as the ModuleType was not valid!");

            return;
        }

        LogDescription(moduleData);

        _modules.Add(module);
        module.Register(moduleData);
    }

    private static void LogDescription(ModuleData moduleData)
    {
        FusionLogger.Log("--==== Loaded Fusion Module ====--", moduleData.Color);

        FusionLogger.Log($"{moduleData.Name} - v{moduleData.Version}");

        FusionLogger.Log($"by {moduleData.Author}");

        FusionLogger.Log("--=============================--", moduleData.Color);
    }
}