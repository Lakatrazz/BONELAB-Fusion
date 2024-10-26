using BoneLib;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Preferences.Server;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Scene;

using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;
using LabFusion.Marrow;

namespace LabFusion.Utilities;

public static class FusionPlayer
{
    public static byte? LastAttacker { get; internal set; }
    public static readonly List<Transform> SpawnPoints = new();

    public static float? VitalityOverride { get; internal set; } = null;
    public static string AvatarOverride { get; internal set; } = null;

    internal static void OnMainSceneInitialized()
    {
        LastAttacker = null;
    }

    internal static void OnUpdate()
    {
        if (FusionSceneManager.IsLoading())
        {
            return;
        }

        if (!RigData.HasPlayer)
        {
            return;
        }

        CheckFloatingPoint();
    }

    private static void CheckFloatingPoint()
    {
        var rm = RigData.Refs.RigManager;
        var position = rm.physicsRig.feet.transform.position;

        if (!position.IsNanOrInf())
        {
            return;
        }

#if DEBUG
        FusionLogger.Warn("Player was sent out of floating point, reloading scene.");
#endif

        if (NetworkInfo.HasServer && !NetworkInfo.IsServer)
        {
            NetworkHelper.Disconnect("Game Crash");
        }

        SceneStreamer.Reload();

        FusionNotifier.Send(new FusionNotification()
        {
            isPopup = true,
            showTitleOnPopup = true,
            title = "Whoops! Sorry about that!",
            type = NotificationType.WARNING,
            message = "The scene was reloaded due to a physics crash.",
            popupLength = 6f,
        });
    }

    internal static void Internal_OnAvatarChanged(RigManager rigManager, Avatar avatar, string barcode)
    {
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
        var crate = rigManager.AvatarCrate.Crate;

        if (AvatarOverride != null && !FusionAvatar.IsMatchingAvatar(barcode, AvatarOverride))
        {
            Internal_ChangeAvatar();
        }
        // If we don't have an avatar override set, check if we are allowed to use custom avatars
        else if (crate != null && !crate.Pallet.IsInMarrowGame())
        {
            if (PlayerIdManager.LocalId != null && PlayerIdManager.LocalId.TryGetPermissionLevel(out var level))
            {
                var requirement = LobbyInfoManager.LobbyInfo.CustomAvatars;

                if (!FusionPermissions.HasSufficientPermissions(level, requirement))
                {
                    // Change to polyblank, we don't have permission
                    rigManager.SwapAvatarCrate(new Barcode(FusionAvatar.POLY_BLANK_BARCODE), true);
                }
            }
        }

        // Invoke hooks and other events
        PlayerAdditionsHelper.OnAvatarChanged(rigManager);
    }

    /// <summary>
    /// Tries to get the player that we were last attacked by.
    /// </summary>
    /// <returns></returns>
    public static bool TryGetLastAttacker(out PlayerId id)
    {
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
    public static bool IsSelf(this RigManager rigManager)
    {
        if (!RigData.HasPlayer)
            return true;

        return rigManager == RigData.Refs.RigManager;
    }

    /// <summary>
    /// Sets the ammo count of the local player for all types.
    /// </summary>
    /// <param name="count"></param>
    public static void SetAmmo(int count)
    {
        var ammoInventory = AmmoInventory.Instance;

        if (ammoInventory == null)
        {
            return;
        }

        ammoInventory.ClearAmmo();

        ammoInventory.AddCartridge(ammoInventory.lightAmmoGroup, count);
        ammoInventory.AddCartridge(ammoInventory.heavyAmmoGroup, count);
        ammoInventory.AddCartridge(ammoInventory.mediumAmmoGroup, count);
    }

    /// <summary>
    /// Checks if we are allowed to unragdoll.
    /// </summary>
    /// <returns></returns>
    public static bool CanUnragdoll()
    {
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
    public static void SetMortality(bool isMortal)
    {
        var rm = RigData.Refs.RigManager;

        if (!rm.IsNOC())
        {
            var playerHealth = rm.health.TryCast<Player_Health>();

            if (isMortal)
            {
                playerHealth.healthMode = Health.HealthMode.Mortal;
            }
            else
            {
                playerHealth.healthMode = Health.HealthMode.Invincible;
            }
        }
    }

    /// <summary>
    /// Resets the mortality to the server settings.
    /// </summary>
    public static void ResetMortality()
    {
        if (!NetworkInfo.HasServer)
            return;

        SetMortality(CommonPreferences.Mortality);
    }

    /// <summary>
    /// Teleports the player to a point.
    /// </summary>
    /// <param name="position"></param>
    /// <param name=""></param>
    public static void Teleport(Vector3 position, Vector3 fwdSnap, bool zeroVelocity = true)
    {
        if (!RigData.HasPlayer)
            return;

        var rm = RigData.Refs.RigManager;

        rm.Teleport(position, fwdSnap, zeroVelocity);
        rm.physicsRig.ResetHands(Handedness.BOTH);
    }

    /// <summary>
    /// Sets the custom spawn points for the player.
    /// </summary>
    /// <param name="points"></param>
    public static void SetSpawnPoints(params Transform[] points)
    {
        SpawnPoints.Clear();
        SpawnPoints.AddRange(points);
    }

    /// <summary>
    /// Clears all spawn points.
    /// </summary>
    public static void ResetSpawnPoints()
    {
        SpawnPoints.Clear();
    }

    public static void SetAvatarOverride(string barcode)
    {
        AvatarOverride = barcode;
        Internal_ChangeAvatar();
    }

    public static void ClearAvatarOverride()
    {
        AvatarOverride = null;
    }

    public static void SetPlayerVitality(float vitality)
    {
        VitalityOverride = vitality;
        Internal_ChangePlayerHealth();
    }

    public static void ClearPlayerVitality()
    {
        VitalityOverride = null;
        Internal_ChangePlayerHealth();
    }

    private static void Internal_ChangeAvatar()
    {
        // Check avatar override
        if (RigData.HasPlayer && AssetWarehouse.ready && AvatarOverride != null)
        {
            var avatarCrate = CrateFilterer.GetCrate<AvatarCrate>(new Barcode(AvatarOverride));

            if (avatarCrate == null)
            {
                return;
            }

            var rm = RigData.Refs.RigManager;
            rm.SwapAvatarCrate(new Barcode(AvatarOverride), true, (Action<bool>)((success) =>
            {
                // If the avatar forcing doesn't work, change into polyblank
                if (!success)
                {
                    rm.SwapAvatarCrate(new Barcode(FusionAvatar.POLY_BLANK_BARCODE), true);
                }
            }));
        }
    }

    private static void Internal_ChangePlayerHealth()
    {
        if (RigData.HasPlayer)
        {
            var rm = RigData.Refs.RigManager;
            var avatar = rm._avatar;

            if (VitalityOverride.HasValue)
            {
                avatar._vitality = VitalityOverride.Value;
                rm.health.SetAvatar(avatar);
            }
            else
            {
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
    public static bool TryGetSpawnPoint(out Transform point)
    {
        point = null;

        SpawnPoints.RemoveAll((t) => t == null);

        if (SpawnPoints.Count > 0)
        {
            point = SpawnPoints.GetRandom();
            return true;
        }

        return false;
    }
}