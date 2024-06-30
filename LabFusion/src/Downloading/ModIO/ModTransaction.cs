using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Downloading.ModIO;

public class ModTransaction
{
    public ModIOFile modFile;
    public DownloadCallback callback;

    public void HookDownload(DownloadCallback callback)
    {
        this.callback += callback;
    }
}
