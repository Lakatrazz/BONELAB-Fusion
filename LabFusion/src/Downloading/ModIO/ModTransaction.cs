namespace LabFusion.Downloading.ModIO;

public class ModTransaction
{
    public ModIOFile modFile = default;

    public bool temporary = false;

    public DownloadCallback callback = null;

    public void HookDownload(DownloadCallback callback)
    {
        this.callback += callback;
    }
}
