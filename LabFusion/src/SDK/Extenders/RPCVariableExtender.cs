using LabFusion.Marrow.Integration;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Entities;

namespace LabFusion.SDK.Extenders;

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

        entity.OnEntityDataCatchup += OnEntityDataCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, RPCVariable[] components)
    {
        foreach (var component in components)
        {
            component.HasNetworkEntity = false;

            Cache.Remove(component);
        }

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        foreach (var component in Components)
        {
            component.CatchupPlayer(player);
        }
    }
}