using LabFusion.Marrow.Integration;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public class RPCVariableExtender : EntityComponentArrayExtender<RPCVariable>
{
    public static readonly FusionComponentCache<RPCVariable, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, RPCVariable[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, RPCVariable[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }
    }
}