using LabFusion.Utilities;

namespace LabFusion.Downloading.ModIO;

public class ModTransaction : IProgress<float>
{
    public ModIOFile ModFile { get; set; } = default;

    public bool Temporary { get; set; } = false;

    public DownloadCallback Callback { get; set; } = null;

    public long? MaxBytes { get; set; } = null;

    public float Progress { get; private set; } = 0f;

    public IReadOnlyList<IProgress<float>> Reporters => _reporters;

    public bool HasReporters => Reporters.Count > 0;

    private readonly List<IProgress<float>> _reporters = new();

    public void HookTransaction(ModTransaction otherTransaction)
    {
        if (otherTransaction.HasReporters)
        {
            AddReporters(otherTransaction.Reporters);
        }

        HookDownload(otherTransaction.Callback);
    }

    public void HookDownload(DownloadCallback callback)
    {
        this.Callback += callback;
    }

    public void AddReporter(IProgress<float> reporter)
    {
        _reporters.Add(reporter);
    }

    public void AddReporters(IEnumerable<IProgress<float>> reporters)
    {
        _reporters.AddRange(reporters);
    }

    public void RemoveReporter(IProgress<float> reporter)
    {
        _reporters.Remove(reporter);
    }

    public void ClearReporters()
    {
        _reporters.Clear();
    }

    public void Report(float value)
    {
        Progress = value;

        // If we have progress reporters, then try to report the download progress
        if (HasReporters)
        {
            foreach (var reporter in Reporters)
            {
                try
                {
                    reporter?.Report(value);
                }
                catch (Exception e)
                {
                    FusionLogger.LogException("reporting progress of mod transaction", e);
                }
            }
        }
    }
}
