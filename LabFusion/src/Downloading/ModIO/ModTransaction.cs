namespace LabFusion.Downloading.ModIO;

public class ModTransaction
{
    public ModIOFile modFile = default;

    public bool temporary = false;

    public DownloadCallback callback = null;

    private float _progress = 0f;
    public float Progress
    {
        get
        {
            return _progress;
        }
        set
        {
            _progress = value;
        }
    }

    public void HookDownload(DownloadCallback callback)
    {
        this.callback += callback;
    }
}
