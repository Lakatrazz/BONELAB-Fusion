using LabFusion.Marrow.Integration;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Entities;

namespace LabFusion.SDK.Extenders;

public class AnimatorSyncerExtender : EntityComponentArrayExtender<AnimatorSyncer>
{
    public static readonly FusionComponentCache<AnimatorSyncer, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, AnimatorSyncer[] components)
    {
        foreach (var component in components)
        {
            Cache.Add(component, entity);
        }

        entity.OnEntityOwnershipTransfer += OnEntityOwnershipTransfer;

        // Invoke the event if the owner has already been set
        if (entity.HasOwner)
        {
            OnEntityOwnershipTransfer(entity, entity.OwnerID);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, AnimatorSyncer[] components)
    {
        foreach (var component in components)
        {
            Cache.Remove(component);
        }

        entity.OnEntityOwnershipTransfer -= OnEntityOwnershipTransfer;
    }

    private void OnEntityOwnershipTransfer(NetworkEntity entity, PlayerID playerId)
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