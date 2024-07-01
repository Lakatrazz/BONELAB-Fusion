using Il2CppSLZ.Marrow.Warehouse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Downloading;

public delegate void DownloadCallback(DownloadCallbackInfo info);

public struct DownloadCallbackInfo
{
    public static readonly DownloadCallbackInfo FailedCallback = new()
    {
        pallet = null,
        result = DownloadResult.FAILED,
    };

    public enum DownloadResult
    {
        NONE,
        FAILED,
        SUCCEEDED,
    }

    public Pallet pallet;
    public DownloadResult result;
}