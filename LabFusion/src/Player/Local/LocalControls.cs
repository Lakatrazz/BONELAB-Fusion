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
}