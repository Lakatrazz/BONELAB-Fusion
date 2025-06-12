using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class EntityPlayerData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte PlayerID;
    public NetworkEntityReference Entity;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);
        serializer.SerializeValue(ref Entity);
    }
}