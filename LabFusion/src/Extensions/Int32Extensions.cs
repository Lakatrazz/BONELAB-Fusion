namespace LabFusion.Extensions;

public static class Int32Extensions
{
    /// <summary>
    /// Returns if an integer has reached the positive or negative integer limits, which can be caused by casting from a NaN float.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsNaN(this int value)
    {
        return value >= int.MaxValue || value <= int.MinValue;
    }

    /// <summary>
    /// Converts an integer to its ordinal equivalent (1st, 2nd, 3rd, 4th...)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToOrdinal(this int value)
    {
        if (value < 0)
        {
            return value.ToString();
        }

        switch (value % 100)
        {
            case 11:
            case 12:
            case 13:
                return $"{value}th";
        }

        switch (value % 10)
        {
            case 1:
                return $"{value}st";
            case 2:
                return $"{value}nd";
            case 3:
                return $"{value}rd";
            default:
                return $"{value}th";
        }
    }
}