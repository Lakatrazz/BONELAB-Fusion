using LabFusion.Utilities;
using LabFusion.Player;

using Il2CppSLZ.Marrow.PuppetMasta;

namespace LabFusion.Entities;

public class PuppetMasterExtender : EntityComponentExtender<PuppetMaster>
{
    public static FusionComponentCache<PuppetMaster, NetworkEntity> Cache = new();

    public static NetworkEntity LastKilled = null;

    protected override void OnRegister(NetworkEntity entity, PuppetMaster component)
    {
        Cache.Add(component, entity);

        entity.OnEntityOwnershipTransfer += OnPuppetOwnershipTransfer;

        // Update puppet drives if theres already an owner
        if (NetworkEntity.HasOwner)
        {
            OnPuppetOwnershipTransfer(entity, entity.OwnerId);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, PuppetMaster component)
    {
        Cache.Remove(component);

        entity.OnEntityOwnershipTransfer -= OnPuppetOwnershipTransfer;
    }

    private void OnPuppetOwnershipTransfer(NetworkEntity entity, PlayerId playerId)
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
}