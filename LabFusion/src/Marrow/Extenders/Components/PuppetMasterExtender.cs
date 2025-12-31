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

    public bool DefaultUpdateJointAnchors { get; set; } = true;

    protected override void OnRegister(NetworkEntity entity, PuppetMaster component)
    {
        Cache.Add(component, entity);

        DefaultUpdateJointAnchors = component.updateJointAnchors;

        entity.OnEntityOwnershipTransfer += OnEntityOwnershipTransfer;
        entity.OnEntityDataCatchup += OnEntityDataCatchup;

        // Update puppet drives if there's already an owner
        if (entity.HasOwner)
        {
            OnEntityOwnershipTransfer(entity, entity.OwnerID);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, PuppetMaster component)
    {
        Cache.Remove(component);

        entity.OnEntityOwnershipTransfer -= OnEntityOwnershipTransfer;
        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
    }

    private void OnEntityOwnershipTransfer(NetworkEntity entity, PlayerID player)
    {
        bool isOwner = entity.IsOwner;

        // Restore defaults
        if (isOwner)
        {
            Component.updateJointAnchors = DefaultUpdateJointAnchors;
            Component.muscleSpring = Component._defaultMuscleSpring;
            Component.muscleDamper = Component._defaultMuscleDamper;
        }
        // Remove all weights
        else
        {
            Component.updateJointAnchors = false;
            Component.muscleSpring = 0f;
            Component.muscleDamper = 0f;
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