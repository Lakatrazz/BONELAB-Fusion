using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;

namespace LabFusion.Network;

public class ConnectionResponseData : INetSerializable
{
    public PlayerID PlayerID = null;

    public bool IsInitialJoin = false;

    public int? GetSize() => PlayerID.GetSize() + sizeof(bool);

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);

        serializer.SerializeValue(ref IsInitialJoin);
    }

    public static ConnectionResponseData Create(PlayerID id, bool isInitialJoin)
    {
        return new ConnectionResponseData()
        {
            PlayerID = id,
            IsInitialJoin = isInitialJoin,
        };
    }
}

public class ConnectionResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ConnectionResponse;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConnectionResponseData>();

        // Insert the id into our list
        data.PlayerID.Insert();

        // Check the id to see if its our own
        // If it is, just update our self reference
        if (data.PlayerID.PlatformID == PlayerIDManager.LocalPlatformID)
        {
            PlayerIDManager.ApplyLocalID();

            NetworkPlayerManager.CreateLocalPlayer();

            InternalServerHelpers.OnJoinServer();
        }
        // Otherwise, create a network player
        else
        {
            InternalServerHelpers.OnPlayerJoined(data.PlayerID, data.IsInitialJoin);

            NetworkPlayerManager.CreateNetworkPlayer(data.PlayerID);
        }

        // Send catchup messages now that the user is registered
        if (NetworkInfo.IsHost)
        {
            CatchupPlayer(data.PlayerID);
        }
    }

    private static void CatchupPlayer(PlayerID player)
    {
        // SERVER CATCHUP
        // Catchup hooked events
        CatchupManager.InvokePlayerServerCatchup(player);
    }
}