using LabFusion.Marrow.Integration;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Entities;

namespace LabFusion.SDK.Extenders;

public class OwnershipEventsExtender : EntityComponentArrayExtender<OwnershipEvents>
{
    protected override void OnRegister(NetworkEntity entity, OwnershipEvents[] components)
    {
        foreach (var component in components)
        {
            component.Entity = entity;
        }

        entity.OnEntityOwnershipTransfer += OnEntityOwnershipTransfer;

        // Invoke the event if the owner has already been set
        if (entity.HasOwner)
        {
            OnEntityOwnershipTransfer(entity, entity.OwnerID);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, OwnershipEvents[] components)
    {
        foreach (var component in components)
        {
            component.Entity = null;
        }

        entity.OnEntityOwnershipTransfer -= OnEntityOwnershipTransfer;
    }

    private void OnEntityOwnershipTransfer(NetworkEntity entity, PlayerID playerId)
    {
        bool owner = playerId.IsMe;

        foreach (var component in Components)
        {
            try
            {
                component.OnOwnerChanged(owner);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running OwnershipEvents", e);
            }
        }
    }
}