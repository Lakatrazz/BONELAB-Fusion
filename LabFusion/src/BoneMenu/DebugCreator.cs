#if DEBUG
using BoneLib.BoneMenu;

using LabFusion.Data;
using LabFusion.Debugging;
using LabFusion.Extensions;
using LabFusion.Representation;

using UnityEngine;

namespace LabFusion.BoneMenu;

public static partial class BoneMenuCreator
{
    public static void CreateDebugMenu(Page page)
    {
        var debugCategory = page.CreatePage("DEBUG", Color.red);
        debugCategory.CreateFunction("Spawn Player Rep", Color.white, () =>
        {
            PlayerRepUtilities.CreateNewRig((rig) =>
            {
                rig.transform.position = RigData.RigReferences.RigManager.physicsRig.feet.transform.position;
            });
        });

        debugCategory.CreateFunction("Send To Floating Point", Color.red, () =>
        {
            var physRig = RigData.RigReferences.RigManager.physicsRig;

            float force = 100000000000000f;

            for (var i = 0; i < 10; i++)
            {
                physRig.rbFeet.AddForce(Vector3Extensions.left * force, ForceMode.VelocityChange);
                physRig.rbKnee.AddForce(Vector3Extensions.right * force, ForceMode.VelocityChange);
                physRig.rightHand.rb.AddForce(Vector3Extensions.up * force, ForceMode.VelocityChange);
                physRig.leftHand.rb.AddForce(Vector3Extensions.down * force, ForceMode.VelocityChange);
            }
        });

        var zoneMigration = debugCategory.CreatePage("Zone Migration", Color.yellow);
        zoneMigration.CreateFunction("Spawn Zone Migration Tester", Color.yellow, () =>
        {
            DebugZoneMigrator.SpawnMigrator();
        });

        zoneMigration.CreateFunction("Migrate To Zone", Color.yellow, () =>
        {
            DebugZoneMigrator.MigrateToZone();
        });
    }
}
#endif