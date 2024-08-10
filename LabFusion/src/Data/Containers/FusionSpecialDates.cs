using LabFusion.Utilities;

namespace LabFusion.Data
{
    public static class FusionSpecialDates
    {
        public enum FusionDate
        {
            NONE = 0,
            FUSION_BIRTHDAY = 1,
            BONELAB_BIRTHDAY = 2,
            HALLOWEEN = 3,
            CHRISTMAS = 4,
        }

        private enum Month
        {
            JANUARY = 1,
            FEBRUARY = 2,
            MARCH = 3,
            APRIL = 4,
            MAY = 5,
            JUNE = 6,
            JULY = 7,
            AUGUST = 8,
            SEPTEMBER = 9,
            OCTOBER = 10,
            NOVEMBER = 11,
            DECEMBER = 12,
        }

        public static FusionDate GetCurrentDate()
        {
            var time = DateTime.Now;

#if DEBUG
            FusionLogger.Log($"Retrieving date. Month is {(Month)time.Month}. Day is {time.Day}. Year is {time.Year}.");
#endif

            switch ((Month)time.Month)
            {
                case Month.MARCH:
                    // Fusion birthday is March 14th
                    // 4 days of leeway
                    if (Math.Abs(time.Day - 14) <= 4)
                    {
                        return FusionDate.FUSION_BIRTHDAY;
                    }
                    break;
                case Month.SEPTEMBER:
                    // Bonelab birthday is September 29th
                    // 4 days of leeway
                    if (Math.Abs(time.Day - 29) <= 4)
                    {
                        return FusionDate.BONELAB_BIRTHDAY;
                    }
                    break;
                case Month.OCTOBER:
                    if (time.Day <= 31 && time.Day >= 20)
                    {
                        return FusionDate.HALLOWEEN;
                    }
                    break;
                case Month.DECEMBER:
                    if (time.Day <= 25 && time.Day >= 12)
                    {
                        return FusionDate.CHRISTMAS;
                    }
                    break;
            }

            return FusionDate.NONE;
        }
    }
}
