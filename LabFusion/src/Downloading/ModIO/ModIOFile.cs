namespace LabFusion.Downloading.ModIO;

public readonly struct ModIOFile
{
    public int ModId { get; }

    public int? FileId { get; }

    public ModIOFile(int modId, int? fileId = null)
    {
        ModId = modId;
        FileId = fileId;
    }
}
