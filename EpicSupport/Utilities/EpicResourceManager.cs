using LabFusion.Utilities;

using MarrowFusion.Epic.Utilities;

namespace MarrowFusion.Epic;

public static class EpicResourceManager
{
    public static bool IsResourcesAvailable()
    {
        if (!EOSSDKLoader.IsSDKAvailable())
        {
            return false;
        }

        if (PlatformHelper.IsAndroid && !EOSJNI.IsDexAvailable())
        {
            return false;
        }

        return true;
    }

    public static bool TryLoadResources()
    {
        if (!EOSSDKLoader.TryLoadEOSSDK())
        {
            return false;
        }

        if (PlatformHelper.IsAndroid && !EOSJNI.TryInitialize())
        {
            return false;
        }

        return true;
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
