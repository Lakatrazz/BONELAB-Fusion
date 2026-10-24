using LabFusion.Utilities;

using MarrowFusion.Epic.Utilities;

namespace MarrowFusion.Epic;

public static class EpicResourceManager
{
    public static bool IsResourcesAvailable()
    {
        if (PlatformHelper.IsAndroid && !EOSJNI.IsDexAvailable())
        {
            return false;
        }

        return EOSSDKLoader.IsSDKAvailable();
    }

    public static bool TryLoadResources()
    {
        if (PlatformHelper.IsAndroid && !EOSJNI.TryInitialize())
        {
            return false;
        }

        return EOSSDKLoader.TryLoadEOSSDK();
    }

    internal static void Initialize()
    {
        RedirectLibraries();
    }

    private static void RedirectLibraries()
    {
        if (PlatformHelper.IsAndroid)
        {
            ImportRedirector.SetImportResolver();
            ImportRedirector.Redirect("EOSSDK-Win64-Shipping.dll", "EOSSDK");
        }
    }
}
