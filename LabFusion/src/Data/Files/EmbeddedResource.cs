using System.Reflection;

using LabFusion.Utilities;

namespace LabFusion.Data;

public static class EmbeddedResource
{
    public static byte[] LoadBytesFromAssembly(Assembly assembly, string name)
    {
        string[] manifestResources = assembly.GetManifestResourceNames();

        if (!manifestResources.Contains(name))
        {
            return null;
        }

        FusionLogger.Log($"Loading embedded resource data {name}...", ConsoleColor.DarkCyan);

        using Stream str = assembly.GetManifestResourceStream(name);
        using MemoryStream memoryStream = new();

        str.CopyTo(memoryStream);

        FusionLogger.Log("Done!", ConsoleColor.DarkCyan);

        return memoryStream.ToArray();
    }

    public static Assembly LoadAssemblyFromAssembly(Assembly assembly, string name)
    {
        var rawAssembly = LoadBytesFromAssembly(assembly, name);

        if (rawAssembly == null)
        {
            return null;
        }

        return Assembly.Load(rawAssembly);
    }
}
