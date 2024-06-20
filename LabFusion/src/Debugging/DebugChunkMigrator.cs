#if DEBUG
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Zones;
using Il2CppSLZ.Rig;

using LabFusion.Data;
using LabFusion.Marrow.Zones;
using LabFusion.Representation;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Debugging;

public static class DebugChunkMigrator
{
    private static MarrowEntity _migratorEntity = null;

    public static void SpawnMigrator()
    {
        var rigManager = RigData.RigReferences.RigManager;
        var physicsRig = rigManager.physicsRig;

        PlayerRepUtilities.CreateNewRig((rig) =>
        {
            rig.transform.position = physicsRig.rightHand.transform.position;
            _migratorEntity = rig.marrowEntity;
        });
    }

    public static void MigrateToZone()
    {
        if (_migratorEntity == null)
        {
            return;
        }

        var rigManager = RigData.RigReferences.RigManager;
        var physicsRig = rigManager.physicsRig;
        var rightHand = physicsRig.rightHand;

        var overlap = Physics.OverlapSphere(rightHand.transform.position, 0.02f, ~0, QueryTriggerInteraction.Collide);

        ZoneCuller foundCuller = null;

        foreach (var collider in overlap)
        {
            var zoneCuller = collider.GetComponent<ZoneCuller>();

            if (zoneCuller == null)
            {
                continue;
            }

            foundCuller = zoneCuller;
            break;
        }

        if (foundCuller == null)
        {
            FusionLogger.Warn("Migration failed, no culler was found.");
            return;
        }

        FusionLogger.Log($"Closest culler was {foundCuller.name}, migrating.");

        ZoneCullHelper.MigrateEntity(foundCuller._zoneId, _migratorEntity);

        // Get rig from the migrator
        var rig = _migratorEntity.GetComponent<RigManager>();

        // Offset marrow entity to teleport
        var offset = rightHand.transform.position - rig.physicsRig.feet.transform.position;
        _migratorEntity.transform.position += offset;
    }
}
#endif