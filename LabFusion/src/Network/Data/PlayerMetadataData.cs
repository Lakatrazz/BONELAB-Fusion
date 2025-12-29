using LabFusion.Network.Serialization;
using LabFusion.Player;

namespace LabFusion.Network;

public class PlayerMetadataData : INetSerializable
{
    public PlayerReference Player;

    public string Key;
    public string Value;

    public int? GetSize() => PlayerReference.Size + Key.GetSize() + Value.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Player);
        serializer.SerializeValue(ref Key);
        serializer.SerializeValue(ref Value);
    }
}
