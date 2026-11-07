using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.Marrow.Messages;

public class MarrowEntityCreateRequestData : INetSerializable
{
    public int? GetSize() => ComponentHashData.Size + sizeof(uint);

    public ComponentHashData HashData;

    public uint TrackerID;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref HashData);
        serializer.SerializeValue(ref TrackerID);
    }
}

[Net.DelayWhileTargetLoading]
public sealed class MarrowEntityCreateRequest : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MarrowEntityCreateRequestData>();

        var hashData = data.HashData;

        var ownerID = received.SenderSmallID.Value;
        var entityID = NetworkEntityManager.IDManager.RegisteredEntities.AllocateUniqueID();
        var trackerID = data.TrackerID;

        var responseData = new MarrowEntityCreateResponseData()
        {
            OwnerID = ownerID,
            EntityID = entityID,
            HashData = hashData,
            TrackerID = trackerID,
        };

        ServerManager.SendToClientsModule<MarrowEntityCreateResponse, MarrowEntityCreateResponseData>(responseData, NetworkChannel.Reliable);
    }
}