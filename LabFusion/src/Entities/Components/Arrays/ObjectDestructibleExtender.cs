using LabFusion.Utilities;

using Il2CppSLZ.VFX;

namespace LabFusion.Entities;

public class ObjectDestructibleExtender : EntityComponentArrayExtender<ObjectDestructible>
{
    public static FusionComponentCache<ObjectDestructible, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, ObjectDestructible[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, ObjectDestructible[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }
    }
}