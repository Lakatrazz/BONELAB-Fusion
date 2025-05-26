using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;

namespace LabFusion.Network;

public class ConnectionResponseData : INetSerializable
{
    public PlayerID playerId = null;
    public string avatarBarcode = null;
    public SerializedAvatarStats avatarStats = null;
    public bool isInitialJoin = false;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref avatarBarcode);
        serializer.SerializeValue(ref avatarStats);
        serializer.SerializeValue(ref isInitialJoin);
    }

    public static ConnectionResponseData Create(PlayerID id, string avatarBarcode, SerializedAvatarStats stats, bool isInitialJoin)
    {
        return new ConnectionResponseData()
        {
            playerId = id,
            avatarBarcode = avatarBarcode,
            avatarStats = stats,
            isInitialJoin = isInitialJoin,
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
        data.playerId.Insert();

        // Check the id to see if its our own
        // If it is, just update our self reference
        if (data.playerId.PlatformID == PlayerIDManager.LocalPlatformID)
        {
            PlayerIDManager.ApplyLocalID();

            NetworkPlayerManager.CreateLocalPlayer();

            InternalServerHelpers.OnJoinServer();
        }
        // Otherwise, create a network player
        else
        {
            InternalServerHelpers.OnPlayerJoined(data.playerId, data.isInitialJoin);

            var networkPlayer = NetworkPlayerManager.CreateNetworkPlayer(data.playerId);
            networkPlayer.AvatarSetter.SwapAvatar(data.avatarStats, data.avatarBarcode);
        }

        // Update our vitals to everyone
        if (RigData.HasPlayer)
        {
            RigData.OnSendVitals();
        }

        // Send catchup messages now that the user is registered
        if (NetworkInfo.IsHost)
        {
            CatchupPlayer(data.playerId);
        }
    }

    private static void CatchupPlayer(PlayerID player)
    {
        // SERVER CATCHUP
        // Catchup hooked events
        CatchupManager.InvokePlayerServerCatchup(player);
    }
}