using LabFusion.Data;
using LabFusion.Exceptions;

using System.Text.Json;

namespace LabFusion.Network;

public class ServerSettingsData : IFusionSerializable
{
    public LobbyInfo lobbyInfo;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(JsonSerializer.Serialize(lobbyInfo));
    }

    public void Deserialize(FusionReader reader)
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

public class ServerSettingsMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.ServerSettings;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<ServerSettingsData>();

        // ONLY clients should receive this!
        if (NetworkInfo.IsServer)
        {
            throw new ExpectedClientException();
        }

        LobbyInfoManager.LobbyInfo = data.lobbyInfo;
    }
}