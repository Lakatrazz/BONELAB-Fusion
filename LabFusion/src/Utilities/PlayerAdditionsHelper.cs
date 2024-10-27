using LabFusion.MonoBehaviours;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;

using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Utilities;

public static class PlayerAdditionsHelper
{
    public static void OnInitializeMelon()
    {
        // Hook multiplayer events
        MultiplayerHooking.OnJoinServer += () => { OnEnterServer(RigData.Refs.RigManager); };
        MultiplayerHooking.OnStartServer += () => { OnEnterServer(RigData.Refs.RigManager); };
        MultiplayerHooking.OnDisconnect += () => { OnExitServer(RigData.Refs.RigManager); };
        LocalPlayer.OnLocalRigCreated += (rig) =>
        {
            OnCreatedLocalPlayer(rig);

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

    public static void OnAvatarChanged(RigManager manager)
    {
    }

    public static void OnCreatedLocalPlayer(RigManager manager)
    {
        // Forward to the regular method
        OnCreatedRig(manager);
    }

    public static void OnCreatedRig(RigManager manager)
    {
        OnAvatarChanged(manager);
    }

    public static void OnEnterServer(RigManager manager)
    {
        if (manager.IsNOC())
            return;

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

        // Apply mortality
        FusionPlayer.ResetMortality();
    }

    public static void OnExitServer(RigManager manager)
    {
        if (manager.IsNOC())
            return;

        // Disable mute icons
        MuteUIHelper.OnDestroyMuteUI();

        // Remove impact properties
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