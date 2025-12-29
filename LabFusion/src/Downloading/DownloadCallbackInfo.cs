using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Downloading;

public delegate void DownloadCallback(DownloadCallbackInfo info);

public struct DownloadCallbackInfo
{
    public static readonly DownloadCallbackInfo FailedCallback = new()
    {
        Pallet = null,
        Result = ModResult.FAILED,
    };

    public static readonly DownloadCallbackInfo CanceledCallback = new()
    {
        Pallet = null,
        Result = ModResult.CANCELED,
    };

    public Pallet Pallet;
    public ModResult Result;
}