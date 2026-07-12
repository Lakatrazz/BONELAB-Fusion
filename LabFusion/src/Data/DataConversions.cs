namespace LabFusion.Data;

public static class DataConversions
{
    public const long MegabytesToBytes = 1000000;

    public const long GigabytesToBytes = 1000000000;

    public static long ConvertMegabytesToBytes(long megabytes)
    {
        // Make sure these are both longs, otherwise we can overflow an int
        // Which can cause max file size limits to essentially prevent downloads
        return megabytes * MegabytesToBytes;
    }

    public static long ConvertGigabytesToBytes(long gigabytes)
    {
        return gigabytes * GigabytesToBytes;
    }
}
