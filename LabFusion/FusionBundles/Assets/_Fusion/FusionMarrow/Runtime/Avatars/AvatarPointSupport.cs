namespace LabFusion.Marrow.Integration
{
    public static class AvatarPointSupport
    {
        public static bool CheckAlignmentSupported(AvatarPoint point)
        {
            return point switch
            {
                AvatarPoint.Head or 
                AvatarPoint.Chest or 
                AvatarPoint.Hips or 
                AvatarPoint.Wrist or 
                AvatarPoint.Ankle => true,
                _ => false,
            };
        }

        public static bool CheckSideSupported(AvatarPoint point)
        {
            return point switch
            {
                AvatarPoint.Eye or
                AvatarPoint.Wrist or
                AvatarPoint.Ankle => true,
                _ => false,
            };
        }

        public static AvatarSide ValidateSideAndFallback(AvatarPoint point, AvatarSide side)
        {
            switch (point)
            {
                default:
                    return side;
                case AvatarPoint.Wrist:
                case AvatarPoint.Ankle:
                    if (side == AvatarSide.Center)
                    {
                        return AvatarSide.Left;
                    }

                    return side;
            }
        }
    }
}
