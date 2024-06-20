#if DEBUG
using BoneLib.BoneMenu.Elements;

using LabFusion.Data;
using LabFusion.Debugging;
using LabFusion.Extensions;
using LabFusion.Representation;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    public static partial class BoneMenuCreator
    {
        public static void CreateDebugMenu(MenuCategory category)
        {
            var debugCategory = category.CreateCategory("DEBUG", Color.red);
            debugCategory.CreateFunctionElement("Spawn Player Rep", Color.white, () =>
            {
                PlayerRepUtilities.CreateNewRig((rig) =>
                {
                    rig.transform.position = RigData.RigReferences.RigManager.physicsRig.feet.transform.position;
                });
            });

            debugCategory.CreateFunctionElement("Send To Floating Point", Color.red, () =>
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

            var zoneMigration = debugCategory.CreateCategory("Zone Migration", Color.yellow);
            zoneMigration.CreateFunctionElement("Spawn Zone Migration Tester", Color.yellow, () =>
            {
                DebugChunkMigrator.SpawnMigrator();
            });

            zoneMigration.CreateFunctionElement("Migrate To Zone", Color.yellow, () =>
            {
                DebugChunkMigrator.MigrateToZone();
            });
        }
    }
}
#endif