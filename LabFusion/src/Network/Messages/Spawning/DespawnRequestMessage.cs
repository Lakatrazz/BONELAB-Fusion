using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class DespawnRequestData : INetSerializable
{
    public const int Size = sizeof(ushort) + sizeof(byte) * 2;

    public byte despawnerId;
    public ushort entityId;

    public bool despawnEffect;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref despawnerId);
        serializer.SerializeValue(ref entityId);

        serializer.SerializeValue(ref despawnEffect);
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
        var readData = received.ReadData<DespawnRequestData>();

        var writtenData = DespawnResponseData.Create(readData.despawnerId, readData.entityId, readData.despawnEffect);

        MessageRelay.RelayNative(writtenData, NativeMessageTag.DespawnResponse, NetworkChannel.Reliable, RelayType.ToClients);
    }
}