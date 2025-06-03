#nullable enable

using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Bonelab;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Math;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Player;

public delegate void PlayerAvatarDelegate(Avatar avatar, string barcode);

public static class LocalAvatar
{
    private static float? _heightOverride = null;

    /// <summary>
    /// An override for the Local Player's height in meters. Setting to null will apply the default avatar height.
    /// </summary>
    public static float? HeightOverride
    {
        get
        {
            return _heightOverride;
        }
        set
        {
            _heightOverride = value;

            RefreshAvatar();
        }
    }

    private static string? _avatarOverride = null;
    private static string? _storedAvatar = null;

    public static string? AvatarOverride
    {
        get
        {
            return _avatarOverride;
        }
        set
        {
            bool overriding = value != null;
            bool wasOverriding = _avatarOverride != null;

            if (overriding && !wasOverriding)
            {
                _storedAvatar = AvatarBarcode;
            }

            _avatarOverride = value;

            if (!overriding && wasOverriding && !string.IsNullOrWhiteSpace(_storedAvatar))
            {
                SwapAvatarCrate(_storedAvatar);
                _storedAvatar = null;
            }
            else
            {
                RefreshAvatar();
            }
        }
    }

    public static string? AvatarBarcode { get; private set; } = null;

    public static float AvatarHeight { get; private set; } = 1.76f;

    public static float AvatarMass { get; private set; } = 84f;

    public static event PlayerAvatarDelegate? OnAvatarChanged;

    internal static void OnInitializeMelon()
    {
        OnAvatarChanged += OnCheckAvatar;
    }

    internal static void InvokeAvatarChanged(Avatar avatar, string barcode)
    {
        AvatarBarcode = barcode;
        AvatarHeight = avatar.height;
        AvatarMass = avatar.massTotal;

        OnAvatarChanged?.InvokeSafe(avatar, barcode, "executing LocalPlayer.OnAvatarChanged");
    }

    private static void OnCheckAvatar(Avatar avatar, string barcode)
    {
        // Save the stats
        RigData.RigAvatarStats = new SerializedAvatarStats(avatar);
        RigData.RigAvatarId = barcode;

        // Send avatar change
        PlayerSender.SendPlayerAvatar(RigData.RigAvatarStats, barcode);

        var crateReference = new AvatarCrateReference(barcode);

        var crate = crateReference.Crate;

        if (crate != null)
        {
            // Apply metadata
            LocalPlayer.Metadata.AvatarTitle.SetValue(crate.Title);
            LocalPlayer.Metadata.AvatarModID.SetValue(CrateFilterer.GetModID(crate.Pallet));
        }

        OnOverrideAvatar(avatar, barcode, crate);
    }

    private static bool CheckAvatarPrivileges(AvatarCrate? crate)
    {
        if (crate == null || crate.Pallet.IsInMarrowGame())
        {
            return true;
        }

        if (PlayerIDManager.LocalID == null || !PlayerIDManager.LocalID.TryGetPermissionLevel(out var level))
        {
            return true;
        }

        var requirement = LobbyInfoManager.LobbyInfo.CustomAvatars;

        if (!FusionPermissions.HasSufficientPermissions(level, requirement))
        {
            return false;
        }

        return true;
    }

    private static void OnOverrideAvatar(Avatar avatar, string barcode, AvatarCrate? crate)
    {
        if (!string.IsNullOrWhiteSpace(AvatarOverride) && !IsMatchingAvatar(barcode, AvatarOverride))
        {
            SwapAvatarCrate(AvatarOverride);
            return;
        }

        if (!CheckAvatarPrivileges(crate))
        {
            SwapAvatarCrate(BonelabAvatarReferences.PolyBlankBarcode);
            return;
        }

        OnOverrideHeight(avatar, barcode);
    }

    private static float? ProcessHeightOverride(float avatarHeight)
    {
        float? heightOverride = HeightOverride;

        if (heightOverride.HasValue)
        {
            return heightOverride;
        }

        float maxAvatarHeight = ManagedMathf.Clamp(LobbyInfoManager.LobbyInfo.MaxAvatarHeight, 2f, 30f);

        if (NetworkInfo.HasServer && avatarHeight > maxAvatarHeight)
        {
            return maxAvatarHeight;
        }

        return null;
    }

    private static bool _overridingHeight = false;
    private static void OnOverrideHeight(Avatar avatar, string barcode)
    {
        if (_overridingHeight)
        {
            _overridingHeight = false;
            return;
        }

        float? heightOverride = ProcessHeightOverride(avatar.height);

        if (!heightOverride.HasValue)
        {
            return;
        }

        float originalHeight = avatar.height;
        float newHeight = heightOverride.Value;

        Vector3 newScale = (newHeight / originalHeight) * avatar.transform.localScale;

        _overridingHeight = true;

        RigData.Refs.SwapAvatarCrate(barcode, null, (_, newAvatar) =>
        {
            newAvatar.transform.localScale = newScale;
        });
    }
    
    /// <summary>
    /// Refreshes the avatar that the Local Player is currently using.
    /// </summary>
    public static void RefreshAvatar()
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        var rigManager = RigData.Refs.RigManager;
        rigManager.SwapAvatarCrate(rigManager.AvatarCrate.Barcode, true);
    }

    /// <summary>
    /// Swaps the avatar that the Local Player is currently using.
    /// </summary>
    /// <param name="barcode">The avatar barcode to change to.</param>
    public static void SwapAvatarCrate(string barcode)
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        var rigManager = RigData.Refs.RigManager;
        rigManager.SwapAvatarCrate(new(barcode), true, (Action<bool>)(success =>
        {
            // Swap to PolyBlank in case of failure
            if (!success)
            {
                rigManager.SwapAvatarCrate(new(BonelabAvatarReferences.PolyBlankBarcode), true);
            }
        }));
    }

    /// <summary>
    /// Checks if an avatar barcode matches a target avatar. If the barcode is PolyBlank, it will also return true in case the avatar is not available.
    /// </summary>
    /// <param name="barcode">The barcode to check.</param>
    /// <param name="target">The target avatar.</param>
    /// <returns>If the avatars match.</returns>
    public static bool IsMatchingAvatar(string barcode, string target)
    {
        return barcode == target || barcode == BonelabAvatarReferences.PolyBlankBarcode;
    }
}
