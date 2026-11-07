using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class DespawnRequestData : INetSerializable
{
    public static readonly int Size = NetworkEntityReference.Size + sizeof(bool);

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

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DespawnRequestData>();

        var response = new DespawnResponseData()
        {
            Despawner = new(received.SenderSmallID.Value),
            Entity = data.Entity,
            DespawnEffect = data.DespawnEffect,
        };

        ServerManager.SendToClientsNative(response, NativeMessageTag.DespawnResponse, NetworkChannel.Reliable);
    }
}