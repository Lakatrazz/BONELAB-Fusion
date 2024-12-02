#nullable enable

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Utilities;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Player;

public delegate void PlayerAvatarDelegate(Avatar avatar, string barcode);

public static class LocalAvatar
{
    private static float? _heightOverride = null;
    public static float? HeightOverride
    {
        get
        {
            return _heightOverride;
        }
        set
        {
            _heightOverride = value;
        }
    }

    public static event PlayerAvatarDelegate? OnAvatarChanged;

    public static void OnInitializeMelon()
    {
        OnAvatarChanged += OnOverrideHeight;
    }

    internal static void InvokeAvatarChanged(Avatar avatar, string barcode)
    {
        OnAvatarChanged?.InvokeSafe(avatar, barcode, "executing LocalPlayer.OnAvatarChanged");
    }

    private static bool _overridingHeight = false;
    private static void OnOverrideHeight(Avatar avatar, string barcode)
    {
        if (_overridingHeight)
        {
            _overridingHeight = false;
            return;
        }

        if (HeightOverride == null)
        {
            return;
        }

        float originalHeight = avatar.height;
        float newHeight = HeightOverride.Value;

        Vector3 newScale = (newHeight / originalHeight) * avatar.transform.localScale;

        _overridingHeight = true;

        RigData.Refs.SwapAvatarCrate(barcode, null, (_, newAvatar) =>
        {
            newAvatar.transform.localScale = newScale;
        });
    }
}
