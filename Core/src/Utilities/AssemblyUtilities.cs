using LabFusion.Utilities;
using System;
using System.Reflection;

namespace LabFusion.Utilities
{
    public static class AssemblyUtilities
    {
        public static void LoadAllValid<T>(Assembly assembly, Action<Type> runOnValid)
        {
            foreach (Type type in assembly.GetTypes())
            {
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
