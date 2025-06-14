using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.Entities;

public class PooleeExtender : EntityComponentParentExtender<Poolee>
{
    public static readonly FusionComponentCache<Poolee, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, Poolee component)
    {
        Cache.Add(component, entity);
    }

    protected override void OnUnregister(NetworkEntity entity, Poolee component)
    {
        Cache.Remove(component);
    }
}