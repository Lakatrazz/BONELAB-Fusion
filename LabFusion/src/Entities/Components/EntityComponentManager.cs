using System.Reflection;

using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public static class EntityComponentManager
{
    public static void RegisterComponentsFromAssembly(Assembly targetAssembly)
    {
        if (targetAssembly == null)
        {
            throw new NullReferenceException("Can't register from a null assembly!");
        }

#if DEBUG
        FusionLogger.Log($"Populating EntityComponentExtender list from {targetAssembly.GetName().Name}!");
#endif

        AssemblyUtilities.LoadAllValid<IEntityComponentExtender>(targetAssembly, RegisterComponent);
    }

    public static void RegisterComponent<T>() where T : IEntityComponentExtender => RegisterComponent(typeof(T));

    private static void RegisterComponent(Type type)
    {
        if (ExtenderTypes.Contains(type))
        {
            throw new ArgumentException($"Extender type {type.Name} was already registered.");
        }

        ExtenderTypes[_lastExtenderIndex++] = type;

#if DEBUG
        FusionLogger.Log($"Registered {type.Name}");
#endif
    }

    public static HashSet<IEntityComponentExtender> ApplyComponents(NetworkEntity networkEntity, GameObject[] parents)
    {
        var set = new HashSet<IEntityComponentExtender>();

        for (var i = 0; i < _lastExtenderIndex; i++)
        {
            var type = ExtenderTypes[i];

            var instance = Activator.CreateInstance(type) as IEntityComponentExtender;

            if (instance.TryRegister(networkEntity, parents))
            {
                set.Add(instance);
            }
        }

        return set;
    }

    private static int _lastExtenderIndex = 0;

    public static readonly Type[] ExtenderTypes = new Type[1024];
}