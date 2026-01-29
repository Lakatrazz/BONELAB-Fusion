using LabFusion.Utilities;

namespace LabFusion.Network.EpicGames;

internal class EOSConnectionStateManager
{
    internal enum ConnectionState
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected
    }
    
    private ConnectionState _connectionState = ConnectionState.Disconnected;
    
    internal ConnectionState GetConnectionState() => _connectionState;

    internal void SetConnectionState(ConnectionState newConnectionState)
    {
        if (_connectionState != newConnectionState)
        {
            var previousState = _connectionState;
            _connectionState = newConnectionState;
#if DEBUG
            FusionLogger.Log($"Connection state changed: {previousState} -> {newConnectionState}");
#endif
        }
    }

    internal bool CanStartServer()
    {
        if (_connectionState == ConnectionState.Connecting)
            return false;
        
        return true;
    }

    internal bool CanJoinServer()
    {
        if (_connectionState == ConnectionState.Connecting || _connectionState == ConnectionState.Connected)
            return false;
        
        return true;
    }
}