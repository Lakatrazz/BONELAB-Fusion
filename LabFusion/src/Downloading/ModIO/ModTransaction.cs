namespace LabFusion.Downloading.ModIO;

public class ModTransaction : IProgress<float>
{
    public ModIOFile ModFile { get; set; } = default;

    public bool Temporary { get; set; } = false;

    public DownloadCallback Callback { get; set; } = null;

    public long? MaxBytes { get; set; } = null;

    private float _progress = 0f;
    public float Progress => _progress;

    public IProgress<float> Reporter { get; set; } = null;

    public void HookDownload(DownloadCallback callback)
    {
        this.Callback += callback;
    }

    public void Report(float value)
    {
        _progress = value;

        // If we have a reporter, report the progress to it
        Reporter?.Report(value);
    }
}
