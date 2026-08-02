using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using LabFusion.Utilities;

namespace LabFusion.Network;

internal class EpicLobby : NetworkLobby
{
    internal EOSRuntime Runtime;
    internal LobbyDetails LobbyDetails;
    internal Utf8String LobbyID;
    internal ProductUserId Owner;

    private Dictionary<string, string> Metadata = new Dictionary<string, string>();
    
    internal EpicLobby(EOSRuntime runtime, LobbyDetails lobbyDetails, Utf8String lobbyID, ProductUserId owner)
    {
        Runtime = runtime;
        LobbyDetails = lobbyDetails;
        LobbyID = lobbyID;
        Owner = owner;
    }
    
    ~EpicLobby()
    {
        LobbyDetails?.Release();
        LobbyDetails = null;
#if DEBUG
        FusionLogger.Log($"Lobby '{LobbyID}' was garbage collected");
#endif
    }
    
    public override void SetMetadata(string key, string value)
    {
        value ??= string.Empty;

        // EOS can get picky about how often attributes update
        // So just cache existing attributes and bail out if we are trying to write something with the same value
        if (Metadata.TryGetValue(key, out var existingValue) && existingValue == value)
            return;

        Metadata[key] = value;

        Runtime.Lobby.SetAttribute(LobbyID, key, value);
        SaveKey(key);
    }
    
    public override bool TryGetMetadata(string key, out string value)
    {
        value = Runtime.Lobby.GetAttribute(LobbyDetails, key);
        return !string.IsNullOrWhiteSpace(value);
    }
    
    public override string GetMetadata(string key)
    {
        return Runtime.Lobby.GetAttribute(LobbyDetails, key);
    }

    public override Action CreateJoinDelegate(string lobbyId)
    {
        if (NetworkLayerManager.Layer is EpicGamesNetworkLayer epicLayer)
        {
            return () =>
            {
                epicLayer.JoinServer(this);
            };
        }

        return null;
    }
}