using System.Reflection;

namespace LabFusion.Utilities;

public static class AssemblyUtilities
{
    public static bool IsValid(this Assembly assembly)
    {
        string name = assembly.FullName;

        if (name.Contains("System"))
        {
            return false;
        }

        return true;
    }

    public static bool IsValid(this Type type)
    {
        // Mono types can cause a "System.TypeLoadException: Recursive type definition detected" error from IsAssignableFrom, this bypasses it
        if (type.Name.Contains("Mono") && type.Name.Contains("Security"))
        {
            return false;
        }

        return true;
    }

    public static void LoadAllValid<T>(Assembly assembly, Action<Type> runOnValid)
    {
        if (!assembly.IsValid())
        {
            return;
        }

        foreach (Type type in assembly.GetTypes())
        {
            // Mono types can cause a "System.TypeLoadException: Recursive type definition detected" error from IsAssignableFrom, this bypasses it
            if (!type.IsValid())
            {
                continue;
            }

            if (typeof(T).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
            {
                try
                {
                    runOnValid(type);
                }
                catch (Exception e)
                {
                    FusionLogger.Error(e.Message);
                }
            }
        }
    }
}
