using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.Entities;

public class PooleeExtender : EntityComponentExtender<Poolee>
{
    public static FusionComponentCache<Poolee, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, Poolee component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Poolee component)
    {
        Cache.Remove(component);
    }
}