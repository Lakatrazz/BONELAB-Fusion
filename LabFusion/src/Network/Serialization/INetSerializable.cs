namespace LabFusion.Network.Serialization;

public interface INetSerializable
{
    int? GetSize() => null;

    void Serialize(INetSerializer serializer);
}
