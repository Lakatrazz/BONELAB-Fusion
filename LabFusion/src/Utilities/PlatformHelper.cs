using MelonLoader;

namespace LabFusion.Utilities;

public static class PlatformHelper
{
    private static readonly bool _isAndroidCached = MelonUtils.CurrentPlatform == (MelonPlatformAttribute.CompatiblePlatforms)3;
    public static bool IsAndroid => _isAndroidCached;

    public const string AndroidName = "QUEST";
    public const string WindowsName = "PC";

    public static string GetPlatformName()
    {
        if (IsAndroid)
        {
            return AndroidName;
        }

        return WindowsName;
    }
}