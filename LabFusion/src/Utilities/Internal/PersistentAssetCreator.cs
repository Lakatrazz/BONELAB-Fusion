using LabFusion.Data;

using Il2CppTMPro;

using UnityEngine;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LabFusion.Utilities;

internal static class PersistentAssetCreator
{
    // ALL FONTS AT THE START:
    // - arlon-medium SDF
    // - nasalization-rg SDF
    private const string _targetFont = "arlon-medium";

    internal static TMP_FontAsset Font { get; private set; }
    internal static HandPose SoftGrabPose { get; private set; }

    private static Action<HandPose> _onSoftGrabLoaded = null;

    internal static void OnLateInitializeMelon()
    {
        CreateTextFont();
    }

    internal static void OnMainSceneInitialized()
    {
        GetHandPose();
    }

    private static void GetHandPose()
    {
        if (RigData.HasPlayer)
        {
            SoftGrabPose = RigData.Refs.RigManager.worldGripHandPose;
        }

        if (SoftGrabPose != null)
        {
            _onSoftGrabLoaded?.Invoke(SoftGrabPose);
        }

        _onSoftGrabLoaded = null;
    }

    public static void HookOnSoftGrabLoaded(Action<HandPose> action)
    {
        if (SoftGrabPose != null)
        {
            action?.Invoke(SoftGrabPose);
        }
        else
        {
            _onSoftGrabLoaded += action;
        }
    }

    private static void CreateTextFont()
    {
        // This font is paid but included in the game, so I'm going to messily loop for it
        // Could probably do something to load it with addressables but this works
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var font in fonts)
        {
            if (font.name.ToLower().Contains(_targetFont))
            {
                Font = font;
                break;
            }
        }

        // Make sure we at least have a font
        if (Font == null)
        {
#if DEBUG
            FusionLogger.Error($"Failed finding the {_targetFont} font! Defaulting to the first font in the game!");
#endif

            Font = fonts[0];
        }
    }
}
