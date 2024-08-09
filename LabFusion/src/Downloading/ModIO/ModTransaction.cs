namespace LabFusion.Downloading.ModIO;

public class ModTransaction
{
    public ModIOFile ModFile { get; set; } = default;

    public bool Temporary { get; set; } = false;

    public DownloadCallback Callback { get; set; } = null;

    public long? MaxBytes { get; set; } = null;

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
        this.Callback += callback;
    }
}
