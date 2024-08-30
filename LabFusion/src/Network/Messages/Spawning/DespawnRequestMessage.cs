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
public class DespawnRequestMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.DespawnRequest;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // If we aren't the server, throw an error
        if (!isServerHandled)
        {
            throw new ExpectedServerException();
        }

        using var reader = FusionReader.Create(bytes);
        var readData = reader.ReadFusionSerializable<DespawnRequestData>();

        using var writer = FusionWriter.Create(DespawnResponseData.Size);
        var data = DespawnResponseData.Create(readData.despawnerId, readData.entityId, readData.despawnEffect);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.DespawnResponse, writer);
        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
    }
}