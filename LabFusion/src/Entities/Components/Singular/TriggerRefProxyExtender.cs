using LabFusion.Utilities;

using Il2CppSLZ.Marrow.AI;

namespace LabFusion.Entities;

public class TriggerRefProxyExtender : EntityComponentExtender<TriggerRefProxy>
{
    public static FusionComponentCache<TriggerRefProxy, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, TriggerRefProxy component)
    {
        Cache.Add(component, entity);
    }

    protected override void OnUnregister(NetworkEntity entity, TriggerRefProxy component)
    {
        Cache.Remove(component);
    }
}