namespace LabFusion.SDK.Points
{
    public static class BitEconomy
    {
        public const double BitsToPennyRatio = 1;

        public const string BaBaAuthor = "BaBaCorp";
        public const string RiggleAuthor = "Riggle";

        public const int PricelessValue = 99999999;

        public static int ConvertPrice(int pennies)
        {
            return (int)(BitsToPennyRatio * (double)pennies);
        }
    }
}
