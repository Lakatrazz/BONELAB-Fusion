using LabFusion.Data;
using LabFusion.Network.Serialization;

#if DEBUG
using LabFusion.Utilities;
#endif

using System.Text.Json;

namespace LabFusion.Network;

public class ServerSettingsData : INetSerializable
{
    public LobbyInfo lobbyInfo;

    public void Serialize(INetSerializer serializer)
    {
        if (serializer is NetWriter writer)
        {
            Serialize(writer);
        }
        else if (serializer is NetReader reader)
        {
            Deserialize(reader);
        }
    }

    public void Serialize(NetWriter writer)
    {
        writer.Write(JsonSerializer.Serialize(lobbyInfo));
    }

    public void Deserialize(NetReader reader)
    {
        lobbyInfo = JsonSerializer.Deserialize<LobbyInfo>(reader.ReadString());
    }

    public static ServerSettingsData Create()
    {
        return new ServerSettingsData()
        {
            lobbyInfo = LobbyInfoManager.LobbyInfo,
        };
    }
}

public class ServerSettingsMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ServerSettings;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ServerSettingsData>();

        LobbyInfoManager.LobbyInfo = data.lobbyInfo;

#if DEBUG
        FusionLogger.Log("Server LobbyInfo received!");
#endif
    }
}