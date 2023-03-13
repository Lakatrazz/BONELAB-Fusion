using BoneLib;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using SLZ;
using SLZ.Marrow.Warehouse;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Avatar = SLZ.VRMK.Avatar;

namespace LabFusion.Utilities {
    public static class FusionPlayer {
        public static byte? LastAttacker { get; internal set; }
        public static readonly List<Transform> SpawnPoints = new List<Transform>();

        public static float? VitalityOverride { get; internal set; } = null;
        public static string AvatarOverride { get; internal set; } = null;

        internal static void OnMainSceneInitialized() {
            LastAttacker = null;
        }

        internal static void Internal_OnAvatarChanged(RigManager rigManager, Avatar avatar, string barcode) {
            // Save the stats
            RigData.RigAvatarStats = new SerializedAvatarStats(avatar);
            RigData.RigAvatarId = barcode;

            // Send avatar change
            PlayerSender.SendPlayerAvatar(RigData.RigAvatarStats, barcode);

            // Update player values
            // Check player health
            if (VitalityOverride.HasValue)
                Internal_ChangePlayerHealth();

            // Check player avatar
            if (AvatarOverride != null && barcode != AvatarOverride)
                Internal_ChangeAvatar();

            // Invoke hooks and other events
            PlayerAdditionsHelper.OnAvatarChanged(rigManager);
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
            if (!RigData.HasPlayer)
                return;

            var rm = RigData.RigReferences.RigManager;

            rm.Teleport(position, fwdSnap, zeroVelocity);
            rm.physicsRig.ResetHands(Handedness.BOTH);
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

        public static void SetAvatarOverride(string barcode) {
            AvatarOverride = barcode;
            Internal_ChangeAvatar();
        }

        public static void ClearAvatarOverride() {
            AvatarOverride = null;
        }

        public static void SetPlayerVitality(float vitality) {
            VitalityOverride = vitality;
            Internal_ChangePlayerHealth();
        }

        public static void ClearPlayerVitality() {
            VitalityOverride = null;
            Internal_ChangePlayerHealth();
        }

        private static void Internal_ChangeAvatar() {
            // Check avatar override
            if (RigData.HasPlayer && AssetWarehouse.ready && AvatarOverride != null) {
                var avatarCrate = AssetWarehouse.Instance.GetCrate<AvatarCrate>(AvatarOverride);

                if (avatarCrate != null) {
                    var rm = RigData.RigReferences.RigManager;
                    rm.SwapAvatarCrate(AvatarOverride, true, (Action<bool>)((success) => {
                        // If the avatar forcing doesn't work, change into polyblank
                        if (!success) {
                            rm.SwapAvatarCrate(PlayerRepUtilities.PolyBlankBarcode, true);
                        }
                    }));
                }
            }
        }

        private static void Internal_ChangePlayerHealth() {
            if (RigData.HasPlayer) {
                var rm = RigData.RigReferences.RigManager;
                var avatar = rm._avatar;

                if (VitalityOverride.HasValue) {
                    avatar._vitality = VitalityOverride.Value;
                    rm.health.SetAvatar(avatar);
                }
                else {
                    avatar.RefreshBodyMeasurements();
                    rm.health.SetAvatar(avatar);
                }
            }
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
