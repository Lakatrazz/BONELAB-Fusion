using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class EntityPlayerData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte playerId;
    public ushort entityId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref entityId);
    }

    public static EntityPlayerData Create(byte playerId, ushort entityId)
    {
        return new EntityPlayerData()
        {
            playerId = playerId,
            entityId = entityId,
        };
    }
}