using BoneLib;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using SLZ;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    public static class FusionPlayer {
        public static byte? LastAttacker { get; internal set; }
        public static readonly List<Transform> SpawnPoints = new List<Transform>();

        internal static void OnMainSceneInitialized() {
            LastAttacker = null;
        }

        /// <summary>
        /// Tries to get the player that we were last attacked by.
        /// </summary>
        /// <returns></returns>
        public static bool TryGetLastAttacker(out PlayerId id) {
            id = null;

            if (!LastAttacker.HasValue)
                return false;

            id = PlayerIdManager.GetPlayerId(LastAttacker.Value);
            return id != null;
        }

        /// <summary>
        /// Checks if the rigmanager is ourselves.
        /// </summary>
        /// <param name="rigManager"></param>
        /// <returns></returns>
        public static bool IsSelf(this RigManager rigManager) {
            return rigManager == RigData.RigReferences.RigManager;
        }

        /// <summary>
        /// Sets the ammo count of the local player for all types.
        /// </summary>
        /// <param name="count"></param>
        public static void SetAmmo(int count) {
            var rm = RigData.RigReferences.RigManager;

            if (!rm.IsNOC()) {
                var ammo = rm.AmmoInventory;
                ammo.ClearAmmo();

                ammo.AddCartridge(ammo.lightAmmoGroup, count);
                ammo.AddCartridge(ammo.heavyAmmoGroup, count);
                ammo.AddCartridge(ammo.mediumAmmoGroup, count);
            }
        }

        /// <summary>
        /// Checks if we are allowed to unragdoll.
        /// </summary>
        /// <returns></returns>
        public static bool CanUnragdoll() {
            // Check gamemode
            if (Gamemode.ActiveGamemode != null)
            {
                var gamemode = Gamemode.ActiveGamemode;

                if (gamemode.DisableManualUnragdoll)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the mortality of the player.
        /// </summary>
        /// <param name="isMortal"></param>
        public static void SetMortality(bool isMortal) {
            var rm = RigData.RigReferences.RigManager;

            if (!rm.IsNOC()) {
                var playerHealth = rm.health.TryCast<Player_Health>();

                if (isMortal) {
                    playerHealth.healthMode = Health.HealthMode.Mortal;
                }
                else {
                    playerHealth.healthMode = Health.HealthMode.Invincible;
                }
            }
        }

        /// <summary>
        /// Resets the mortality to the server settings.
        /// </summary>
        public static void ResetMortality() {
            if (!NetworkInfo.HasServer)
                return;

            SetMortality(FusionPreferences.IsMortal);
        }

        /// <summary>
        /// Teleports the player to a point.
        /// </summary>
        /// <param name="position"></param>
        /// <param name=""></param>
        public static void Teleport(Vector3 position, Vector3 fwdSnap, bool zeroVelocity = true) {
            var rm = RigData.RigReferences.RigManager;

            if (!rm.IsNOC()) {
                rm.Teleport(position, fwdSnap, zeroVelocity);
                rm.physicsRig.ResetHands(Handedness.BOTH);
            }
        }

        /// <summary>
        /// Sets the custom spawn points for the player.
        /// </summary>
        /// <param name="points"></param>
        public static void SetSpawnPoints(params Transform[] points) {
            SpawnPoints.Clear();
            SpawnPoints.AddRange(points);
        }

        /// <summary>
        /// Clears all spawn points.
        /// </summary>
        public static void ResetSpawnPoints() {
            SpawnPoints.Clear();
        }

        /// <summary>
        /// Gets a random spawn point from the list.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool TryGetSpawnPoint(out Transform point) {
            point = null;

            SpawnPoints.RemoveAll((t) => t == null);
            
            if (SpawnPoints.Count > 0) {
                point = SpawnPoints.GetRandom();
                return true;
            }

            return false;
        }
    }
}
