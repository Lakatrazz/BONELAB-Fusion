using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Scene;
using LabFusion.Marrow;
using LabFusion.Extensions;

using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Utilities;

public static class FusionPlayer
{
    public static byte? LastAttacker { get; internal set; }
    public static readonly List<Transform> SpawnPoints = new();

    public static string AvatarOverride { get; internal set; } = null;

    private static bool _brokeBounds = false;

    internal static void OnInitializeMelon()
    {
        LocalAvatar.OnAvatarChanged += OnAvatarChanged;
    }

    internal static void OnMainSceneInitialized()
    {
        LastAttacker = null;

        if (_brokeBounds)
        {
            Physics.autoSimulation = true;
            _brokeBounds = false;
        }
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

        if (NetworkTransformManager.IsInBounds(position))
        {
            return;
        }

#if DEBUG
        FusionLogger.Warn("Player was sent out of bounds, reloading scene.");
#endif

        // Incase we hit NaN, don't simulate physics!
        Physics.autoSimulation = false;
        _brokeBounds = true;

        if (NetworkInfo.HasServer && !NetworkInfo.IsServer)
        {
            NetworkHelper.Disconnect("Left Bounds");
        }

        SceneStreamer.Reload();

        FusionNotifier.Send(new FusionNotification()
        {
            ShowPopup = true,
            Title = "Whoops! Sorry about that!",
            Type = NotificationType.WARNING,
            Message = "The scene was reloaded due to being sent far out of bounds.",
            PopupLength = 6f,
        });
    }

    private static void OnAvatarChanged(Avatar avatar, string barcode)
    {
        var rigManager = RigData.Refs.RigManager;

        // Save the stats
        RigData.RigAvatarStats = new SerializedAvatarStats(avatar);
        RigData.RigAvatarId = barcode;

        // Send avatar change
        PlayerSender.SendPlayerAvatar(RigData.RigAvatarStats, barcode);

        // Check player avatar
        var crateReference = new AvatarCrateReference(barcode);

        var crate = crateReference.Crate;

        if (crate != null)
        {
            // Apply metadata
            LocalPlayer.Metadata.TrySetMetadata(MetadataHelper.AvatarTitleKey, crate.Title);
            LocalPlayer.Metadata.TrySetMetadata(MetadataHelper.AvatarModIdKey, CrateFilterer.GetModId(crate.Pallet).ToString());
        }

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
                    rigManager.SwapAvatarCrate(new Barcode(BONELABAvatarReferences.PolyBlankBarcode), true);
                }
            }
        }
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
    /// Checks if the RigManager is the local player.
    /// </summary>
    /// <param name="rigManager"></param>
    /// <returns></returns>
    public static bool IsLocalPlayer(this RigManager rigManager)
    {
        if (!RigData.HasPlayer)
        {
            return true;
        }

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
                    rm.SwapAvatarCrate(new Barcode(BONELABAvatarReferences.PolyBlankBarcode), true);
                }
            }));
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