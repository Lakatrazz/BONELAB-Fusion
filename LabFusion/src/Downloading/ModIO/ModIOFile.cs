namespace LabFusion.Downloading.ModIO;

public readonly struct ModIOFile
{
    public int ModID { get; }

    public int? FileID { get; }

    public ModIOFile(int modID, int? fileID = null)
    {
        ModID = modID;
        FileID = fileID;
    }
}
