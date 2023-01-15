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

namespace LabFusion.Utilities {
    public static class PlayerAdditionsHelper {
        public static void OnInitializeMelon() {
            // Hook multiplayer events
            MultiplayerHooking.OnJoinServer += () => { OnEnterServer(RigData.RigReferences.RigManager); };
            MultiplayerHooking.OnStartServer += () => { OnEnterServer(RigData.RigReferences.RigManager); };
            MultiplayerHooking.OnDisconnect += () => { OnExitServer(RigData.RigReferences.RigManager); };
        }

        public static void OnEnterServer(RigManager rig) {
            if (rig.IsNOC())
                return;

            // Setup impact properties
            PersistentAssetCreator.SetupImpactProperties(rig);

            // Setup ragdoll on death
            rig.health._testRagdollOnDeath = true;

            // Remove level reloading on death
            var playerHealth = rig.health.TryCast<Player_Health>();
            playerHealth.reloadLevelOnDeath = false;
            playerHealth.slowMoOnDeath = false;
        }

        public static void OnExitServer(RigManager rig) {
            if (rig.IsNOC())
                return;

            // Remove impact properties
            var impactProperties = rig.GetComponentsInChildren<ImpactProperties>(true);
            foreach (var properties in impactProperties)
                GameObject.Destroy(properties);

            var impactManager = rig.GetComponentInChildren<ImpactPropertiesManager>(true);
            GameObject.Destroy(impactManager);

            // Remove ragdoll on death
            rig.health._testRagdollOnDeath = false;

            // Add back slowmo on death
            var playerHealth = rig.health.TryCast<Player_Health>();
            playerHealth.slowMoOnDeath = true;
        }
    }
}
