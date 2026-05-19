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
    }
}
