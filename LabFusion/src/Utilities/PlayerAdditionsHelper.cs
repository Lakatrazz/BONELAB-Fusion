using LabFusion.MonoBehaviours;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;

using Il2CppSLZ.Marrow;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

using UnityEngine;

namespace LabFusion.Utilities;

public static class PlayerAdditionsHelper
{
    public static void OnInitializeMelon()
    {
        // Hook multiplayer events
        MultiplayerHooking.OnJoinedServer += () => { OnEnterServer(RigData.Refs.RigManager); };
        MultiplayerHooking.OnStartedServer += () => { OnEnterServer(RigData.Refs.RigManager); };
        MultiplayerHooking.OnDisconnected += () => { OnExitServer(RigData.Refs.RigManager); };
        LocalPlayer.OnLocalRigCreated += (rig) =>
        {
            if (NetworkInfo.HasServer)
            {
                OnEnterServer(rig);
            }
        };

        // Invoke extras
        MuteUIHelper.OnInitializeMelon();
    }

    public static void OnDeinitializeMelon()
    {
        // Invoke extras
        MuteUIHelper.OnDeinitializeMelon();
    }

    public static void OnEnterServer(RigManager manager)
    {
        if (manager == null)
        {
            return;
        }

        // Create mute icon
        MuteUIHelper.OnCreateMuteUI(manager);

        // Setup impact properties
        PersistentAssetCreator.SetupImpactProperties(manager);

        // Enable unused experimental features
        manager.health._testVisualDamage = true;

        // Remove level reloading on death
        var playerHealth = manager.health.TryCast<Player_Health>();
        playerHealth.reloadLevelOnDeath = false;
        playerHealth.slowMoOnDeath = false;

        // Add syncers for player collision
        var physRig = manager.physicsRig;

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

    public static void OnExitServer(RigManager manager)
    {
        if (manager == null)
        {
            return;
        }

        // Disable mute icons
        MuteUIHelper.OnDestroyMuteUI();

        // Remove impact properties
        manager.physicsRig._impactProperties = new Il2CppReferenceArray<ImpactProperties>(0);

        var impactProperties = manager.GetComponentsInChildren<ImpactProperties>(true);

        foreach (var properties in impactProperties)
        {
            GameObject.Destroy(properties);
        }

        // Remove collision syncers
        var collisionSyncers = manager.GetComponentsInChildren<CollisionSyncer>(true);

        foreach (var syncer in collisionSyncers)
        {
            GameObject.Destroy(syncer);
        }

        // Remove experimental features
        manager.health._testVisualDamage = false;

        // Add back slowmo on death
        var playerHealth = manager.health.TryCast<Player_Health>();
        playerHealth.slowMoOnDeath = true;
    }
}