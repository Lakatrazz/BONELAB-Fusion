using LabFusion.MonoBehaviours;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;

using SLZ.Combat;
using SLZ.Rig;

using UnityEngine;

namespace LabFusion.Utilities {
    public static class PlayerAdditionsHelper {
        private static int _feetLayer;
        private static int _playerLayer;

        public static void OnInitializeMelon() {
            // Hook multiplayer events
            MultiplayerHooking.OnJoinServer += () => { OnEnterServer(RigData.RigReferences.RigManager); };
            MultiplayerHooking.OnStartServer += () => { OnEnterServer(RigData.RigReferences.RigManager); };
            MultiplayerHooking.OnDisconnect += () => { OnExitServer(RigData.RigReferences.RigManager); };
            MultiplayerHooking.OnLocalPlayerCreated += (rig) => {
                OnCreatedLocalPlayer(rig);

                if (NetworkInfo.HasServer) {
                    OnEnterServer(rig);
                }
            };

            // Setup layers
            _feetLayer = LayerMask.NameToLayer("Feet");
            _playerLayer = LayerMask.NameToLayer("Player");

            Physics.IgnoreLayerCollision(_feetLayer, _playerLayer, false);

            // Invoke extras
            MuteUIHelper.OnInitializeMelon();
        }

        public static void OnDeinitializeMelon() {
            // Undo layer changes
            Physics.IgnoreLayerCollision(_feetLayer, _playerLayer, true);

            // Invoke extras
            MuteUIHelper.OnDeinitializeMelon();
        }

        public static void OnAvatarChanged(RigManager manager) {
            // Ignore collisions between the player and its locosphere/knee due to our layer changes
            var physRig = manager.physicsRig;

            var kneeColliders = physRig.knee.GetComponentsInChildren<Collider>();
            var feetColliders = physRig.feet.GetComponentsInChildren<Collider>();

            var playerColliders = physRig.GetComponentsInChildren<Collider>();

            Internal_IgnoreCollisions(kneeColliders, playerColliders);
            Internal_IgnoreCollisions(feetColliders, playerColliders);
        }

        public static void OnCreatedLocalPlayer(RigManager manager) {
            // Forward to the regular method
            OnCreatedRig(manager);
        }

        public static void OnCreatedRig(RigManager manager) {
            OnAvatarChanged(manager);
        }

        private static void Internal_IgnoreCollisions(Collider[] first, Collider[] second) {
            foreach (var col1 in first) {
                foreach (var col2 in second) {
                    Physics.IgnoreCollision(col1, col2, true);
                }
            }
        }

        public static void OnEnterServer(RigManager manager) {
            if (manager.IsNOC())
                return;

            // Create mute icon
            MuteUIHelper.OnCreateMuteUI(manager);

            // Setup impact properties
            PersistentAssetCreator.SetupImpactProperties(manager);

            // Setup ragdoll on death
            manager.health._testRagdollOnDeath = true;

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

        public static void OnExitServer(RigManager manager) {
            if (manager.IsNOC())
                return;

            // Disable mute icons
            MuteUIHelper.OnDestroyMuteUI(manager);

            // Remove impact properties
            var impactProperties = manager.GetComponentsInChildren<ImpactProperties>(true);
            foreach (var properties in impactProperties)
                GameObject.Destroy(properties);

            var impactManager = manager.GetComponentInChildren<ImpactPropertiesManager>(true);
            GameObject.Destroy(impactManager);

            // Remove collision syncers
            var collisionSyncers = manager.GetComponentsInChildren<CollisionSyncer>(true);
            foreach (var syncer in collisionSyncers)
                GameObject.Destroy(syncer);

            // Remove ragdoll on death
            manager.health._testRagdollOnDeath = false;

            // Add back slowmo on death
            var playerHealth = manager.health.TryCast<Player_Health>();
            playerHealth.slowMoOnDeath = true;
        }
    }
}
