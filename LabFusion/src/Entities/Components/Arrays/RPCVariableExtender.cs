using LabFusion.Marrow.Integration;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public class RPCVariableExtender : EntityComponentArrayExtender<RPCVariable>
{
    public static readonly FusionComponentCache<RPCVariable, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, RPCVariable[] components)
    {
        foreach (var component in components)
        {
            component.HasNetworkEntity = true;

            Cache.Add(component, entity);
        }

        entity.OnEntityCatchup += OnEntityCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, RPCVariable[] components)
    {
        foreach (var component in components)
        {
            component.HasNetworkEntity = false;

            Cache.Remove(component);
        }

        entity.OnEntityCatchup -= OnEntityCatchup;
    }

    private void OnEntityCatchup(NetworkEntity entity, PlayerId player)
    {
        foreach (var component in Components)
        {
            component.CatchupPlayer(player);
        }
    }
}