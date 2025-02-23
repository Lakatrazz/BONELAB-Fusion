using LabFusion.Marrow.Integration;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public class AnimatorSyncerExtender : EntityComponentArrayExtender<AnimatorSyncer>
{
    public static readonly FusionComponentCache<AnimatorSyncer, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, AnimatorSyncer[] components)
    {
        foreach (var grip in components)
        {
            Cache.Add(grip, networkEntity);
        }

        networkEntity.OnEntityOwnershipTransfer += OnEntityOwnershipTransfer;

        // Invoke the event if the owner has already been set
        if (networkEntity.HasOwner)
        {
            OnEntityOwnershipTransfer(networkEntity, networkEntity.OwnerId);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, AnimatorSyncer[] components)
    {
        foreach (var grip in components)
        {
            Cache.Remove(grip);
        }

        networkEntity.OnEntityOwnershipTransfer -= OnEntityOwnershipTransfer;
    }

    private void OnEntityOwnershipTransfer(NetworkEntity entity, PlayerId playerId)
    {
        bool owner = playerId.IsMe;

        foreach (var component in Components)
        {
            try
            {
                component.IsOwner = owner;
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running AnimatorSyncerExtender.OnEntityOwnershipTransfer", e);
            }
        }
    }
}