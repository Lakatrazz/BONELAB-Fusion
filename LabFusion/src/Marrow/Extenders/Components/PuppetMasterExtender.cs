using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Marrow.Messages;

using Il2CppSLZ.Marrow.PuppetMasta;

namespace LabFusion.Marrow.Extenders;

public class PuppetMasterExtender : EntityComponentExtender<PuppetMaster>
{
    public static readonly FusionComponentCache<PuppetMaster, NetworkEntity> Cache = new();

    public static NetworkEntity LastKilled { get; set; } = null;

    protected override void OnRegister(NetworkEntity entity, PuppetMaster component)
    {
        Cache.Add(component, entity);

        entity.OnEntityOwnershipTransfer += OnPuppetOwnershipTransfer;

        entity.OnEntityDataCatchup += OnEntityDataCatchup;

        // Update puppet drives if theres already an owner
        if (NetworkEntity.HasOwner)
        {
            OnPuppetOwnershipTransfer(entity, entity.OwnerID);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, PuppetMaster component)
    {
        Cache.Remove(component);

        entity.OnEntityOwnershipTransfer -= OnPuppetOwnershipTransfer;

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
    }

    private void OnPuppetOwnershipTransfer(NetworkEntity entity, PlayerID playerId)
    {
        // If we aren't the owner, clear the puppet's pd drives so we can control it with forces
        bool isOwner = entity.IsOwner;

        float muscleWeightMaster = Component.muscleWeight;
        float muscleSpring = Component.muscleSpring;
        float muscleDamper = Component.muscleDamper;

        foreach (var muscle in Component.muscles)
        {
            if (isOwner)
            {
                muscle.MusclePdDrive(muscleWeightMaster, muscleSpring, muscleDamper);
            }
            else
            {
                muscle.MusclePdDrive(0f, 0f, 0f);
            }
        }
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        if (Component.isDead)
        {
            var data = new NetworkEntityReference(entity);

            MessageRelay.RelayModule<PuppetMasterKillMessage, NetworkEntityReference>(data, new MessageRoute(player.SmallID, NetworkChannel.Reliable));
        }
    }
}