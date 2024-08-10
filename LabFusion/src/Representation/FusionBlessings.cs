namespace LabFusion.Representation
{
    public static class FusionBlessings
    {
        public enum BlessingLevel
        {
            NONE = 1 << 0,
            BLESSED = 1 << 1,
            GOD = 1 << 2,
        }

        public static bool IsBlessed(ulong id)
        {
            return FusionMasterList.VerifyPlayer(id, string.Empty) == FusionMasterResult.MASTER;
        }

        public static BlessingLevel GetBlessingLevel(ulong id)
        {
            if (IsBlessed(id))
                return BlessingLevel.BLESSED;
            else
                return BlessingLevel.NONE;
        }
    }
}
