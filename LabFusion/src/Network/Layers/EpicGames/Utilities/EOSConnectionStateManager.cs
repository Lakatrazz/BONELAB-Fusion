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

    internal void SetConnectionState(ConnectionState newConnectionState) => _connectionState = newConnectionState;

    internal bool CanStartServer()
    {
        if (_connectionState != ConnectionState.Disconnected)
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