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
