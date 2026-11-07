using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Rpc;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Modules;

namespace LabFusion.Marrow.Messages;

public class MarrowEntityCreateResponseData : INetSerializable
{
    public int? GetSize() => ClientSmallID.Size + NetworkEntityID.Size + ComponentHashData.Size + sizeof(uint);

    public ClientSmallID OwnerID;

    public NetworkEntityID EntityID;

    public ComponentHashData HashData;

    public uint TrackerID;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref OwnerID);
        serializer.SerializeValue(ref EntityID);
        serializer.SerializeValue(ref HashData);
        serializer.SerializeValue(ref TrackerID);
    }
}

[Net.DelayWhileTargetLoading]
public sealed class MarrowEntityCreateResponse : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MarrowEntityCreateResponseData>();

        var marrowEntity = MarrowEntityHelper.GetEntityFromData(data.HashData);

        // Make sure the marrow entity exists
        if (marrowEntity == null)
        {
            return;
        }

        // Check if it already has an entity attached
        if (IMarrowEntityExtender.Cache.ContainsSource(marrowEntity))
        {
            return;
        }

        // Create a new network entity and network prop
        NetworkEntity networkEntity = new()
        {
            Source = EntitySource.Scene,
        };

        NetworkProp networkProp = new(networkEntity, marrowEntity);

        // Register the entity with the sent id
        NetworkEntityManager.IDManager.RegisterEntity(data.EntityID, networkEntity);

        // Set the owner to the received owner id
        var ownerID = PlayerIDManager.GetPlayerID(data.OwnerID);

        networkEntity.SetOwner(ownerID);

        // Insert creation catchup hook for future users
        networkEntity.EntityCreationCatchingUp += (entity, player) =>
        {
            SendCatchup(networkProp, player.PlatformID);
        };

        CatchupManager.RequestEntityDataCatchup(new(networkEntity));

        NetworkMarrowEntityCreator.OnCreated(data.TrackerID, new NetworkMarrowEntityCreator.MarrowEntityCreateCallbackInfo()
        {
            MarrowEntity = marrowEntity,
            NetworkEntity = networkEntity,
        });
    }

    public static void SendCatchup(NetworkProp networkProp, ClientPlatformID client)
    {
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        var marrowEntity = networkProp.MarrowEntity;
        var hashData = MarrowEntityHelper.GetDataFromEntity(marrowEntity);

        if (hashData == null)
        {
            return;
        }

        var networkEntity = networkProp.NetworkEntity;

        var data = new MarrowEntityCreateResponseData()
        {
            OwnerID = networkEntity.OwnerID.SmallID,
            EntityID = networkEntity.ID,
            HashData = hashData,
        };

        ServerManager.SendToClientModule<MarrowEntityCreateResponse, MarrowEntityCreateResponseData>(data, NetworkChannel.Reliable, client);
    }
}