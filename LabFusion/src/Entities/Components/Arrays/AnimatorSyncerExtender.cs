using LabFusion.Marrow.Integration;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public class AnimatorSyncerExtender : EntityComponentArrayExtender<AnimatorSyncer>
{
    public static readonly FusionComponentCache<AnimatorSyncer, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, AnimatorSyncer[] components)
    {
        foreach (var grip in components)
        {
            Cache.Add(grip, entity);
        }

        entity.OnEntityOwnershipTransfer += OnEntityOwnershipTransfer;

        // Invoke the event if the owner has already been set
        if (entity.HasOwner)
        {
            OnEntityOwnershipTransfer(entity, entity.OwnerId);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, AnimatorSyncer[] components)
    {
        foreach (var grip in components)
        {
            Cache.Remove(grip);
        }

        entity.OnEntityOwnershipTransfer -= OnEntityOwnershipTransfer;
    }

    private void OnEntityOwnershipTransfer(NetworkEntity entity, PlayerId playerId)
    {
        bool owner = playerId != null && playerId.IsMe;

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