namespace LabFusion.Network.Proxy;

internal enum MessageTypes
{
    SteamID = 0,
    OnDisconnected = 1,
    OnMessage = 2,
    GetUsername = 3,
    UnreliableBroadcastToClients = 4,
    ReliableBroadcastToClients = 5,
    UnreliableBroadcastToServer = 6,
    ReliableBroadcastToServer = 7,
    OnConnectionDisconnected = 8,
    OnConnectionMessage = 9,
    JoinServer = 10,
    Disconnect = 11,
    Ping = 12,
    StartServer = 13,
    UnreliableSendFromServer = 14,
    ReliableSendFromServer = 15,
    LobbyIDs = 16,
    LobbyMetadata = 17,

    SetLobbyMetadata = 20,
    SteamFriends = 21,

    DisconnectUser = 22,
}