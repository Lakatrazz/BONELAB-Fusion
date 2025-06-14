using System.Linq.Expressions;
using System.Reflection;

using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public static class EntityComponentManager
{
    public static void LoadComponents(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new NullReferenceException("Can't register from a null assembly!");
        }

#if DEBUG
        FusionLogger.Log($"Populating EntityComponentExtender list from {assembly.GetName().Name}!");
#endif

        AssemblyUtilities.LoadAllValid<IEntityComponentExtender>(assembly, RegisterComponent);
    }

    public static void RegisterComponent<T>() where T : IEntityComponentExtender => RegisterComponent(typeof(T));

    private static void RegisterComponent(Type type)
    {
        if (ExtenderTypes.Contains(type))
        {
            throw new ArgumentException($"Extender type {type.Name} was already registered.");
        }

        var index = _lastExtenderIndex++;

        ExtenderTypes[index] = type;
        ExtenderFactories[index] = CreateExtenderFactory(type);

#if DEBUG
        FusionLogger.Log($"Registered {type.Name}");
#endif
    }

    public static HashSet<IEntityComponentExtender> ApplyComponents(NetworkEntity entity, GameObject parent)
    {
        var set = new HashSet<IEntityComponentExtender>();

        for (var i = 0; i < _lastExtenderIndex; i++)
        {
            var factory = ExtenderFactories[i];

            var instance = factory();

            if (instance.TryRegister(entity, parent))
            {
                set.Add(instance);
            }
        }

        return set;
    }

    public static HashSet<IEntityComponentExtender> ApplyDynamicComponents(NetworkEntity entity, GameObject parent)
    {
        var set = new HashSet<IEntityComponentExtender>();

        for (var i = 0; i < _lastExtenderIndex; i++)
        {
            var type = ExtenderTypes[i];

            if (entity.GetExtender(type) is IEntityComponentExtender extender)
            {
                extender.RegisterDynamics(entity, parent);
                continue;
            }

            var factory = ExtenderFactories[i];

            var instance = factory();

            if (instance.TryRegister(entity, parent))
            {
                set.Add(instance);
            }
        }

        return set;
    }

    private static Func<IEntityComponentExtender> CreateExtenderFactory(Type type)
    {
        return Expression.Lambda<Func<IEntityComponentExtender>>(Expression.New(type)).Compile();
    }

    private static int _lastExtenderIndex = 0;

    public static readonly Type[] ExtenderTypes = new Type[1024];

    public static readonly Func<IEntityComponentExtender>[] ExtenderFactories = new Func<IEntityComponentExtender>[1024];
}