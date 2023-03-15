using LabFusion.MonoBehaviours;
using LabFusion.Data;
using LabFusion.Extensions;

using SLZ.Combat;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using LabFusion.Network;

using SLZ.Interaction;

using UnhollowerBaseLib;

using SLZ.Marrow.Data;
using SLZ.UI;
using LabFusion.Preferences;

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
        }

        public static void OnDeinitializeMelon() {
            // Undo layer changes
            Physics.IgnoreLayerCollision(_feetLayer, _playerLayer, true);
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
            // Insert quick mute button
            var popUpMenu = manager.uiRig.popUpMenu;
            var homePage = popUpMenu.radialPageView.m_HomePage;
            var mutedPref = FusionPreferences.ClientSettings.Muted;

            string name = mutedPref.GetValue() ? "Quick Unmute" : "Quick Mute";

            var mutePage = new PageItem(name, PageItem.Directions.SOUTHEAST, (Action)(() => {
                mutedPref.SetValue(!mutedPref.GetValue());
                popUpMenu.Deactivate();
            }));

            mutedPref.OnValueChanged += (v) => {
                if (mutePage != null) {
                    mutePage.name = mutedPref.GetValue() ? "Quick Unmute" : "Quick Mute";
                }
            };

            homePage.items.Add(mutePage);
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
