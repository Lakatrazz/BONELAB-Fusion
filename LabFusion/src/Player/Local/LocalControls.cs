using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Player;

public static class LocalControls
{
    public static bool LockedMovement { get; set; } = false;

    private static bool? _doubleJumpOverride = null;
    public static bool? DoubleJumpOverride
    {
        get
        {
            return _doubleJumpOverride;
        }
        set
        {
            _doubleJumpOverride = value;

            OnOverrideControls();
        }
    }

    internal static void OnInitializeMelon()
    {
        MultiplayerHooking.OnMainSceneInitialized += OnMainSceneInitialized;
    }

    private static void OnMainSceneInitialized()
    {
        OnOverrideControls();
    }

    private static void OnOverrideControls()
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!RigData.HasPlayer)
        {
            return;
        }

        var rigManager = RigData.Refs.RigManager;

        var remapRig = rigManager.remapHeptaRig;

        remapRig.doubleJump = DoubleJumpOverride.GetValueOrDefault();
    }
}