using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class DespawnRequestData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + sizeof(bool);

    public NetworkEntityReference Entity;

    public bool DespawnEffect;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Entity);

        serializer.SerializeValue(ref DespawnEffect);
    }
}

[Net.DelayWhileTargetLoading]
public class DespawnRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.DespawnRequest;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DespawnRequestData>();

        var response = new DespawnResponseData()
        {
            Despawner = new(received.Sender.Value),
            Entity = data.Entity,
            DespawnEffect = data.DespawnEffect,
        };

        MessageRelay.RelayNative(response, NativeMessageTag.DespawnResponse, CommonMessageRoutes.ReliableToClients);
    }
}