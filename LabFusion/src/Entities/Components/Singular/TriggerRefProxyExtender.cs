using LabFusion.Utilities;

using Il2CppSLZ.Marrow.AI;

namespace LabFusion.Entities;

public class TriggerRefProxyExtender : EntityComponentExtender<TriggerRefProxy>
{
    public static FusionComponentCache<TriggerRefProxy, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, TriggerRefProxy component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, TriggerRefProxy component)
    {
        Cache.Remove(component);
    }
}