using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using LabFusion.Network;

namespace MarrowFusion.Epic;

internal class EpicLobby : NetworkLobby
{
    internal EOSRuntime Runtime;
    internal LobbyDetails LobbyDetails;
    internal ProductUserId Owner;
    
    internal EpicLobby(EOSRuntime runtime, LobbyDetails lobbyDetails, ProductUserId owner)
    {
        Runtime = runtime;
        LobbyDetails = lobbyDetails;
        Owner = owner;
    }

    protected override ServerID OnGetServerID() => new(Owner.ToString());

    protected override void OnSetMetadata(string key, string value)
    {
        value ??= string.Empty;

        Runtime.Lobby.SetAttribute(LobbyDetails, key, value);
    }

    protected override string OnGetMetadata(string key) => Runtime.Lobby.GetAttribute(LobbyDetails, key);

    protected override void OnDisposed(bool disposing)
    {
        LobbyDetails?.Release();
        LobbyDetails = null;
    }
}