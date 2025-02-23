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
}