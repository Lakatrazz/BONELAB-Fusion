namespace LabFusion.SDK.Points;

public static class BitEconomy
{
    public const double BitsToPennyRatio = 1;

    public const int PricelessValue = 1000000;

    public static int ConvertPrice(int pennies)
    {
        return (int)(BitsToPennyRatio * (double)pennies);
    }
}