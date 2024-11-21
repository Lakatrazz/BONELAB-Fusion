using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Downloading;

public delegate void DownloadCallback(DownloadCallbackInfo info);

public struct DownloadCallbackInfo
{
    public static readonly DownloadCallbackInfo FailedCallback = new()
    {
        pallet = null,
        result = ModResult.FAILED,
    };

    public static readonly DownloadCallbackInfo CanceledCallback = new()
    {
        pallet = null,
        result = ModResult.CANCELED,
    };

    public Pallet pallet;
    public ModResult result;
}