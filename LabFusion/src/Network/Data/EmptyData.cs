using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class EmptyData : INetSerializable
{
    public const int Size = 0;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer) { }
}