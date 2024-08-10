using LabFusion.Marrow.Integration;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public class RPCEventExtender : EntityComponentArrayExtender<RPCEvent>
{
    public static readonly FusionComponentCache<RPCEvent, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, RPCEvent[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, RPCEvent[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }
    }
}