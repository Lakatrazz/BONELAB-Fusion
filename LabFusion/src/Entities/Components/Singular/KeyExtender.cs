using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

namespace LabFusion.Entities;

public class KeyExtender : EntityComponentExtender<Key>
{
    public static FusionComponentCache<Key, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, Key component)
    {
        Cache.Add(component, entity);
    }

    protected override void OnUnregister(NetworkEntity entity, Key component)
    {
        Cache.Remove(component);
    }
}