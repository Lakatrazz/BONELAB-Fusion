using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Downloading.ModIO;

public class ModTransaction
{
    public ModIOFile modFile;
    public Action downloadCallback;

    public void HookDownload(Action downloadCallback)
    {
        this.downloadCallback += downloadCallback;
    }
}
