using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Player;

public static class LocalControls
{
    public static bool LockedMovement { get; set; } = false;

    private static bool _disableInteraction = false;
    public static bool DisableInteraction
    { 
        get
        {
            return _disableInteraction;
        }
        set
        {
            _disableInteraction = value;

            if (value)
            {
                LocalPlayer.ReleaseGrips();
            }
        }
    }

    private static bool _disableSlowMo = false;
    public static bool DisableSlowMo
    {
        get
        {
            return _disableSlowMo;
        }
        set
        {
            _disableSlowMo = value;

            OnOverrideSlowMo();
        }
    }

    public static bool SlowMoEnabled
    {
        get
        {
            if (!NetworkInfo.HasServer)
            {
                return true;
            }

            if (DisableSlowMo)
            {
                return false;
            }

            return CommonPreferences.SlowMoMode switch
            {
                TimeScaleMode.DISABLED => false,
                _ => true,
            };
        }
    }

    public static bool DisableAmmoPouch { get; set; } = false;

    public static bool DisableInventory { get; set; } = false;

    internal static void OnInitializeMelon()
    {
        MultiplayerHooking.OnMainSceneInitialized += OnMainSceneInitialized;
        LobbyInfoManager.OnLobbyInfoChanged += OnLobbyInfoChanged;
    }

    private static void OnMainSceneInitialized()
    {
        OnOverrideSlowMo();
    }

    private static void OnLobbyInfoChanged()
    {
        OnOverrideSlowMo();
    }

    internal static void OnFixedUpdate() 
    {
        UpdateSlowMo();
    }

    private static void OnOverrideSlowMo()
    {
        TimeManager.slowMoEnabled = SlowMoEnabled;
    }

    private static void UpdateSlowMo()
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!RigData.HasPlayer)
        {
            return;
        }

        var mode = CommonPreferences.SlowMoMode;

        switch (mode)
        {
            case TimeScaleMode.LOW_GRAVITY:
                ApplyLowGravity();
                break;
        }
    }

    private static void ApplyLowGravity()
    {
        if (!TimeManager.slowMoEnabled)
        {
            return;
        }

        var references = RigData.Refs;
        var rm = references.RigManager;

        float intensity = TimeManager.cur_intensity;

        if (intensity <= 0f)
        {
            return;
        }

        float mult = 1f - (1f / TimeManager.cur_intensity);

        Vector3 force = -Physics.gravity * mult;

        foreach (var rb in rm.physicsRig.selfRbs)
        {
            if (rb.useGravity)
            {
                rb.AddForce(force, ForceMode.Acceleration);
            }
        }
    }
}