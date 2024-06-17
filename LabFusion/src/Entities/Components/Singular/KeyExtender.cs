using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

namespace LabFusion.Entities;

public class KeyExtender : EntityComponentExtender<Key>
{
    public static FusionComponentCache<Key, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, Key component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Key component)
    {
        Cache.Remove(component);
    }
}