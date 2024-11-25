namespace LabFusion.Data;

public static class DataConversions
{
    public const long MEGABYTES_TO_BYTES = 1000000;

    public static long ConvertMegabytesToBytes(long megabytes)
    {
        // Make sure these are both longs, otherwise we can overflow an int
        // Which can cause max file size limits to essentially prevent downloads
        return megabytes * MEGABYTES_TO_BYTES;
    }
}
