using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.MonoBehaviours;

using UnityEngine;

namespace LabFusion.Marrow.Rig;

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
    /// <param name="onSpawnCallback"></param>
    public static void SpawnNetRig(Action<RigManager> onSpawnCallback)
    {
        var marrowSettings = MarrowSettings.RuntimeInstance;

        if (marrowSettings == null)
        {
            return;
        }

        var defaultPlayerRig = marrowSettings.DefaultPlayerRig.Crate;

        if (defaultPlayerRig == null)
        {
            return;
        }

        defaultPlayerRig.LoadAsset((Action<GameObject>)((go) => OnDefaultRigLoaded(go, onSpawnCallback)));
    }

    private static void OnDefaultRigLoaded(GameObject asset, Action<RigManager> onSpawnCallback)
    {
        // Create a disabled parent GameObject for the rig
        // This is so that the rig can be stripped without any of the duplicated components executing
        GameObject disabledContainer = new();
        disabledContainer.SetActive(false);

        var rigManagerAsset = asset.GetComponentInChildren<RigManager>().gameObject;

        var newRigGameObject = GameObject.Instantiate(rigManagerAsset, disabledContainer.transform);
        newRigGameObject.name = NetRigName;
        newRigGameObject.SetActive(false);

        // Give it a known valid spawn position to prevent any weird collision issues at 0, 0, 0
        if (RigData.Refs.RigManager)
        {
            newRigGameObject.transform.position = RigData.RigSpawn;
            newRigGameObject.transform.rotation = RigData.RigSpawnRot;
        }

        var newRigManager = newRigGameObject.GetComponent<RigManager>();
        ConvertToNetRig(newRigManager);

        newRigGameObject.transform.parent = null;
        GameObject.Destroy(disabledContainer);

        newRigGameObject.SetActive(true);

        onSpawnCallback?.Invoke(newRigManager);
    }

    private static void ConvertToNetRig(RigManager rigManager)
    {
        // Since the net rig is not part of the pool, an AntiHasher needs to be added
        // This prevents Fusion from hashing its MarrowEntity, which could cause syncing issues
        rigManager.gameObject.AddComponent<AntiHasher>();

        // Strip all components from the rig that are unnecessary/interfere with the local player
        RigStripper.StripRigManager(rigManager);

        // Add the FusionPlayer BoneTag for identifying net rigs
        rigManager.physicsRig.marrowEntity.Tags.Tags.Add(FusionBoneTagReferences.FusionPlayerReference);

        // Apply any additional components or changes to the net rig
        RigAdditions.ApplyNetRigAdditions(rigManager);
    }
}
