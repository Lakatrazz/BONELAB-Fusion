using LabFusion.Data;

namespace LabFusion.Network
{
    public class ComponentIndexData : IFusionSerializable
    {
        public const int Size = sizeof(byte) * 2 + sizeof(ushort);

        public byte smallId;
        public ushort syncId;
        public byte componentIndex;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
            writer.Write(componentIndex);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            componentIndex = reader.ReadByte();
        }

        public static ComponentIndexData Create(byte smallId, ushort syncId, byte componentIndex)
        {
            return new ComponentIndexData()
            {
                smallId = smallId,
                syncId = syncId,
                componentIndex = componentIndex,
            };
        }
    }
}