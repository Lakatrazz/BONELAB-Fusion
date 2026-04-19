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

        using Stream stream = assembly.GetManifestResourceStream(name);
        using MemoryStream memoryStream = new();

        stream.CopyTo(memoryStream);

        FusionLogger.Log("Done!", ConsoleColor.DarkCyan);

        return memoryStream.ToArray();
    }

    public static string LoadTextFromAssembly(Assembly assembly, string name)
    {
        string[] manifestResources = assembly.GetManifestResourceNames();

        if (!manifestResources.Contains(name))
        {
            return null;
        }

        FusionLogger.Log($"Loading embedded resource text {name}...", ConsoleColor.DarkCyan);

        using Stream stream = assembly.GetManifestResourceStream(name);
        using StreamReader reader = new(stream);

        var text = reader.ReadToEnd();

        FusionLogger.Log("Done!", ConsoleColor.DarkCyan);

        return text;
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
