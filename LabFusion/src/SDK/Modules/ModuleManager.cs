using LabFusion.Utilities;

using System.Reflection;

namespace LabFusion.SDK.Modules;

public static class ModuleManager
{
    private static readonly List<Module> _modules = new();

    public static List<Module> Modules => _modules;

    /// <summary>
    /// Loads all <see cref="Module"/>s from an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to load modules from.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void LoadModules(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        AssemblyUtilities.LoadAllValid<Module>(assembly, RegisterModule);
    }

    /// <summary>
    /// Registers a module of the given type.
    /// </summary>
    /// <param name="type">The type of the module.</param>
    public static void RegisterModule(Type type)
    {
        var module = (Module)Activator.CreateInstance(type);

        LogDescription(module);

        _modules.Add(module);
        module.Register();
    }

    /// <summary>
    /// Registers a module of the given type.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    public static void RegisterModule<TModule>() where TModule : Module => RegisterModule(typeof(TModule));

    private static void LogDescription(Module module)
    {
        FusionLogger.Log("--==== Loaded Fusion Module ====--", module.Color);

        FusionLogger.Log($"{module.Name} - v{module.Version}");

        FusionLogger.Log($"by {module.Author}");

        FusionLogger.Log("--=============================--", module.Color);
    }
}