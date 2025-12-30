using LabFusion.Data;
using LabFusion.SDK.Modules;

using System.Reflection;

namespace LabFusion.Support;

public static class SupportManager
{
    public static readonly Dictionary<string, string> GameToModule = new()
    {
        { SupportGameNames.BonelabName, SupportResourcePaths.BonelabSupportPath },
    };

    public static void LoadGameModule(Assembly assembly)
    {
        if (!GameToModule.TryGetValue(GameInfo.GameName, out var modulePath))
        {
            return;
        }

        var moduleAssembly = EmbeddedResource.LoadAssemblyFromAssembly(assembly, modulePath);

        if (moduleAssembly == null)
        {
            return;
        }

        ModuleManager.LoadModules(moduleAssembly);
    }
}
