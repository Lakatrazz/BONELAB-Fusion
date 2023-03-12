using LabFusion.Utilities;
using System;
using System.Reflection;

namespace LabFusion.Utilities
{
    public static class AssemblyUtilities
    {
        public static void LoadAllValid<T>(Assembly assembly, Action<Type> runOnValid)
        {
            string asmName = assembly.FullName;
            if (asmName.Contains("System"))
                return;

            foreach (Type type in assembly.GetTypes())
            {
                // Mono types can cause a "System.TypeLoadException: Recursive type definition detected" error from IsAssignableFrom, this bypasses it
                if (type.Name.Contains("Mono") && type.Name.Contains("Security"))
                    continue;

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
}
