namespace LabFusion.Player;

public static class LocalControls
{
    private static bool _lockedMovement = false;
    public static bool LockedMovement => _lockedMovement;

    public static void LockMovement()
    {
        _lockedMovement = true;
    }

    public static void UnlockMovement()
    {
        _lockedMovement = false;
    }
}