using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class EntityPlayerData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte PlayerId;
    public NetworkEntityReference Entity;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerId);
        serializer.SerializeValue(ref Entity);
    }
}