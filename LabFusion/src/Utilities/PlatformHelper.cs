using MelonLoader;
using MelonLoader.Utils;

namespace LabFusion.Utilities;

public static class PlatformHelper
{
    public enum Platform
    {
        Steam,
        Rift,
        Quest,
        Unknown
    }
    
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
    
    public static Platform GetPlatform()
    {
        if (IsAndroid)
            return Platform.Quest;

        if (MelonEnvironment.GameExecutableName.Contains("Steam"))
            return Platform.Steam;
        if (MelonEnvironment.GameExecutableName.Contains("Oculus"))
            return Platform.Rift;
        
        return Platform.Unknown;
    }
}