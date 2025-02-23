namespace LabFusion.Player;

public static class LocalControls
{
    public static bool LockedMovement { get; set; } = false;

    private static bool _disableGrabbing = false;
    public static bool DisableGrabbing
    { 
        get
        {
            return _disableGrabbing;
        }
        set
        {
            _disableGrabbing = value;

            if (value)
            {
                LocalPlayer.ReleaseGrips();
            }
        }
    }
}