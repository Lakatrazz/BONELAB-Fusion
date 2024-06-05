namespace LabFusion.Extensions
{
    public static class Int32Extensions
    {
        public static bool IsNaN(this int value)
        {
            return value >= int.MaxValue || value <= int.MinValue;
        }
    }
}
