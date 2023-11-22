namespace LabFusion.Utilities
{
    public static class SafetyUtilities
    {
        public static bool IsValidTime => TimeUtilities.TimeScale > 0f && TimeUtilities.DeltaTime > 0f && TimeUtilities.FixedDeltaTime > 0f;
    }
}
