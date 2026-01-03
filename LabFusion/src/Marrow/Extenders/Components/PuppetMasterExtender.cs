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

        if (component != null)
        {
            RestoreWeights(component);
        }
    }

    private void OnEntityOwnershipTransfer(NetworkEntity entity, PlayerID player)
    {
        bool isOwner = entity.IsOwner;

        if (isOwner)
        {
            RestoreWeights(Component);
        }
        else
        {
            RemoveWeights(Component);
        }
    }

    private void RestoreWeights(PuppetMaster puppetMaster)
    {
        puppetMaster.updateJointAnchors = DefaultUpdateJointAnchors;
        puppetMaster.muscleSpring = puppetMaster._defaultMuscleSpring;
        puppetMaster.muscleDamper = puppetMaster._defaultMuscleDamper;
    }

    private void RemoveWeights(PuppetMaster puppetMaster)
    {
        puppetMaster.updateJointAnchors = false;
        puppetMaster.muscleSpring = 0f;
        puppetMaster.muscleDamper = 0f;
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