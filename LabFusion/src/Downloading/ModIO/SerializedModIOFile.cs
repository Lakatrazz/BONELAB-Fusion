using LabFusion.Network.Serialization;

namespace LabFusion.Downloading.ModIO;

public class SerializedModIOFile : INetSerializable
{
    public static readonly SerializedModIOFile Default = new(null);

    public int? GetSize() => sizeof(int) * 2 + sizeof(bool) + Platform.GetSize();

    public ModIOFile File;

    public bool HasFile;

    public string Platform;

    public void Serialize(INetSerializer serializer)
    {
        int modID = File.ModID;
        int? fileID = File.FileID;

        serializer.SerializeValue(ref modID);
        serializer.SerializeValue(ref fileID);

        serializer.SerializeValue(ref HasFile);
        serializer.SerializeValue(ref Platform);

        if (serializer.IsReader)
        {
            if (Platform != ModIOManager.GetActivePlatform())
            {
                fileID = null;
            }

            File = new ModIOFile(modID, fileID);
        }
    }

    public SerializedModIOFile() { }

    public SerializedModIOFile(ModIOFile? file)
    {
        HasFile = file.HasValue;

        if (HasFile)
        {
            this.File = file.Value;
        }

        Platform = ModIOManager.GetActivePlatform();
    }
}