namespace LabFusion.Network.Serialization;

public interface INetSerializable
{
    void Serialize(INetSerializer serializer);
}
