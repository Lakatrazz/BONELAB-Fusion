using Il2CppSLZ.Marrow;

using LabFusion.Data;

using UnityEngine;

namespace LabFusion.Marrow.Rig;

/// <summary>
/// Manages the spawning of NetRigs to be used for representing players.
/// </summary>
public static class NetRigSpawner
{
    /// <summary>
    /// This is the base name of net rigs. Rigs for actual players will have the ID appended to the end in the format "(ID #)".
    /// </summary>
    public const string NetRigName = "[RigManager (Networked)]";

    /// <summary>
    /// Gets the name for a net rig with a specific player ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetNetRigName(int id) => $"{NetRigName} (ID {id})";

    /// <summary>
    /// Spawns a RigManager to be used for network players.
    /// </summary>
    /// <param name="spawnCallback"></param>
    public static void SpawnNetRig(Action<RigManager> spawnCallback)
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        // Give it a known valid spawn position to prevent any weird collision issues at 0, 0, 0
        if (RigData.Refs.RigManager)
        {
            position = RigData.RigSpawn;
            rotation = RigData.RigSpawnRot;
        }

        DummyRigCreator.CreateDummyRig(new DummyRigCreator.DummyRigCreationInfo()
        {
            Position = position,
            Rotation = rotation,
            BeforeEnableCallback = OnBeforeNetRigEnabled,
            SpawnCallback = spawnCallback,
        });
    }

    private static void OnBeforeNetRigEnabled(RigManager rigManager)
    {
        // Rename the net rig
        rigManager.gameObject.name = NetRigName;

        // Add the FusionPlayer BoneTag for identifying net rigs
        rigManager.physicsRig.marrowEntity.Tags.Tags.Add(FusionBoneTagReferences.FusionPlayerReference);

        // Apply any additional components or changes to the net rig
        RigAdditions.ApplyNetRigAdditions(rigManager);
    }
}
