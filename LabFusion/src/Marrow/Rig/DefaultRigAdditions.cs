using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.MonoBehaviours;
using LabFusion.Player;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Marrow.Rig;

/// <summary>
/// Applies default modifications to the local and net RigManagers.
/// To implement your own modifications, use the events in <see cref="RigAdditions"/>.
/// </summary>
public static class DefaultRigAdditions
{
    internal static void Initialize()
    {
        RigAdditions.OnApplyRigAdditions += OnApplyRigAdditions;
        RigAdditions.OnRemoveRigAdditions += OnRemoveRigAdditions;

        RigAdditions.OnApplyLocalRigAdditions += OnApplyLocalRigAdditions;
        RigAdditions.OnRemoveLocalRigAdditions += OnRemoveLocalRigAdditions;

        NetworkAvatarManager.OnNetworkPlayerAvatarChanged += OnNetworkPlayerAvatarChanged;
    }

    private static void OnNetworkPlayerAvatarChanged(NetworkPlayer networkPlayer, Avatar avatar, string barcode)
    {
        // Reset blood hits so that they're properly cleared
        networkPlayer.RigRefs.Health.ResetHits();
    }

    private static void OnApplyRigAdditions(RigManager rigManager)
    {
        AddImpactProperties(rigManager);

        // Allows for blood decals
        rigManager.health._testVisualDamage = true;
    }

    private static void OnRemoveRigAdditions(RigManager rigManager)
    {
        RemoveImpactProperties(rigManager);

        // Remove the enabled blood decals
        rigManager.health._testVisualDamage = false;
    }

    private static void OnApplyLocalRigAdditions(RigManager rigManager)
    {
        AddCollisionSyncers(rigManager);

        // Remove level reloading on death
        var playerHealth = rigManager.health.TryCast<Player_Health>();
        playerHealth.reloadLevelOnDeath = false;
        playerHealth.slowMoOnDeath = false;
    }

    private static void OnRemoveLocalRigAdditions(RigManager rigManager)
    {
        RemoveCollisionSyncers(rigManager);

        // Restore slowmo on death
        var playerHealth = rigManager.health.TryCast<Player_Health>();
        playerHealth.slowMoOnDeath = true;
    }

    private static void AddImpactProperties(RigManager rigManager)
    {
        var physRig = rigManager.physicsRig;
        var rigidbodies = physRig.GetComponentsInChildren<Rigidbody>(true);

        var surfaceData = physRig._surfaceDataDefault;

        var avatar = rigManager.avatar;

        if (avatar != null && avatar.surfaceData)
        {
            surfaceData = avatar.surfaceData;
        }

        var impactProperties = new List<ImpactProperties>();

        for (var i = 0; i < rigidbodies.Length; i++)
        {
            var rb = rigidbodies[i];
            var go = rb.gameObject;

            // Check if it already has impact properties
            if (rb.GetComponent<ImpactProperties>())
            {
                continue;
            }

            // Ignore specific rigidbodies
            if (go == physRig.knee || go == physRig.feet)
            {
                continue;
            }

            var properties = go.AddComponent<ImpactProperties>();
            properties.surfaceData = surfaceData;
            properties.decalType = ImpactProperties.DecalType.None;

            impactProperties.Add(properties);
        }

        physRig._impactProperties = new Il2CppReferenceArray<ImpactProperties>(impactProperties.ToArray());
    }

    private static void RemoveImpactProperties(RigManager rigManager)
    {
        rigManager.physicsRig._impactProperties = new Il2CppReferenceArray<ImpactProperties>(0);

        var impactProperties = rigManager.GetComponentsInChildren<ImpactProperties>(true);

        foreach (var properties in impactProperties)
        {
            GameObject.Destroy(properties);
        }
    }

    private static void AddCollisionSyncers(RigManager rigManager)
    {
        var physRig = rigManager.physicsRig;

        // Left arm
        physRig.m_handLf.gameObject.AddComponent<CollisionSyncer>();
        physRig.m_elbowLf.gameObject.AddComponent<CollisionSyncer>();
        physRig.m_shoulderLf.gameObject.AddComponent<CollisionSyncer>();

        // Right arm
        physRig.m_handRt.gameObject.AddComponent<CollisionSyncer>();
        physRig.m_elbowRt.gameObject.AddComponent<CollisionSyncer>();
        physRig.m_shoulderRt.gameObject.AddComponent<CollisionSyncer>();

        // Head and feet
        physRig.feet.gameObject.AddComponent<CollisionSyncer>();
        physRig.m_head.gameObject.AddComponent<CollisionSyncer>();
    }

    private static void RemoveCollisionSyncers(RigManager rigManager)
    {
        var collisionSyncers = rigManager.GetComponentsInChildren<CollisionSyncer>(true);

        foreach (var syncer in collisionSyncers)
        {
            GameObject.Destroy(syncer);
        }
    }
}
