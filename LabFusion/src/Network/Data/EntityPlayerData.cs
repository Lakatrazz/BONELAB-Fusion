using LabFusion.Data;

namespace LabFusion.Network
{
    public class EntityPlayerData : IFusionSerializable
    {
        public const int Size = sizeof(byte) + sizeof(ushort);

        public byte playerId;
        public ushort entityId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(playerId);
            writer.Write(entityId);
        }

        public void Deserialize(FusionReader reader)
        {
            playerId = reader.ReadByte();
            entityId = reader.ReadUInt16();
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
}