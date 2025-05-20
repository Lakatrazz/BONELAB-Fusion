using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;

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
        }
    }

    private static bool _disableMagazinePouch = false;
    public static bool DisableAmmoPouch
    {
        get
        {
            return _disableMagazinePouch;
        }
        set
        {
            _disableMagazinePouch = value;
        }
    }

    internal static void OnFixedUpdate() 
    {
        UpdateSlowMo();
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
        if (DisableSlowMo || !TimeManager.slowMoEnabled)
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