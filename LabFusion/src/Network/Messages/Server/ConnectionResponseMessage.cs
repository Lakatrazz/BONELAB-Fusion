using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;

namespace LabFusion.Network;

public class ConnectionResponseData : INetSerializable
{
    public ClientPlatformID PlatformID;

    public ClientSmallID SmallID;

    public Dictionary<string, string> InitialMetadata;

    public bool IsInitialJoin = false;

    public int? GetSize() => PlatformID.GetSize() + SmallID.GetSize() + InitialMetadata.GetSize() + sizeof(bool);

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlatformID);
        serializer.SerializeValue(ref SmallID);
        serializer.SerializeValue(ref InitialMetadata);

        serializer.SerializeValue(ref IsInitialJoin);
    }
}

public class ConnectionResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ConnectionResponse;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConnectionResponseData>();

        PlayerID playerID = PlayerIDManager.GetPlayerID(data.PlatformID);

        if (playerID == null)
        {
            playerID = new PlayerID(data.PlatformID, data.SmallID, data.InitialMetadata);
            playerID.Insert();
        }

        // Check the id to see if its our own
        // If it is, just update our self reference
        if (playerID.PlatformID == PlayerIDManager.LocalPlatformID)
        {
            PlayerIDManager.ApplyLocalID();

            NetworkPlayerManager.CreateLocalPlayer();

            InternalServerHelpers.OnJoinServer();
        }
        // Otherwise, create a network player
        else
        {
            InternalServerHelpers.OnPlayerJoined(playerID, data.IsInitialJoin);

            NetworkPlayerManager.CreateNetworkPlayer(playerID);
        }

        // Send catchup messages now that the user is registered
        if (ServerManager.IsServerRunning)
        {
            CatchupPlayer(playerID);
        }
    }

    private static void CatchupPlayer(PlayerID player)
    {
        // SERVER CATCHUP
        // Catchup hooked events
        CatchupManager.InvokePlayerServerCatchup(player);
    }
}