using LabFusion.Data;
using LabFusion.Exceptions;

namespace LabFusion.Network;

public class DespawnRequestData : IFusionSerializable
{
    public const int Size = sizeof(ushort) + sizeof(byte) * 2;

    public byte despawnerId;
    public ushort entityId;

    public bool despawnEffect;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(despawnerId);
        writer.Write(entityId);

        writer.Write(despawnEffect);
    }

    public void Deserialize(FusionReader reader)
    {
        despawnerId = reader.ReadByte();
        entityId = reader.ReadUInt16();

        despawnEffect = reader.ReadBoolean();
    }

    public static DespawnRequestData Create(byte despawnerId, ushort entityId, bool despawnEffect)
    {
        return new DespawnRequestData()
        {
            despawnerId = despawnerId,
            entityId = entityId,

            despawnEffect = despawnEffect,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class DespawnRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.DespawnRequest;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        using var reader = FusionReader.Create(received.Bytes);
        var readData = reader.ReadFusionSerializable<DespawnRequestData>();

        var writtenData = DespawnResponseData.Create(readData.despawnerId, readData.entityId, readData.despawnEffect);

        MessageRelay.RelayNative(writtenData, NativeMessageTag.DespawnResponse, NetworkChannel.Reliable, RelayType.ToClients);
    }
}