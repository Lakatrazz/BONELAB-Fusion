namespace LabFusion.Debugging
{
    public static class FusionDevMode
    {
#if DEBUG
        public const bool UnlockEverything = false;
#else
        public const bool UnlockEverything = false;
#endif
    }
}
