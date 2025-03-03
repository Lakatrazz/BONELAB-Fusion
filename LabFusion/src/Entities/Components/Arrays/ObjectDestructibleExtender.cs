using LabFusion.Utilities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

public class ObjectDestructibleExtender : EntityComponentArrayExtender<ObjectDestructible>
{
    public static FusionComponentCache<ObjectDestructible, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, ObjectDestructible[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, entity);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, ObjectDestructible[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }
    }
}