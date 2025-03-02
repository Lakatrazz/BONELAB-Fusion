using LabFusion.Network.Serialization;

namespace LabFusion.Downloading.ModIO;

public class SerializedModIOFile : INetSerializable
{
    public static readonly SerializedModIOFile Default = new(null);

    public ModIOFile File;

    public bool HasFile;

    public void Serialize(INetSerializer serializer)
    {
        int modId = File.ModId;

        serializer.SerializeValue(ref modId);
        serializer.SerializeValue(ref HasFile);

        if (serializer.IsReader)
        {
            File = new ModIOFile(modId, null);
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
    }
}