namespace MarrowFusion.Epic;

internal class EOSRuntime
{
    internal EOSPlatform Platform { get; private set; }
    internal EOSAuth Auth { get; private set; }
    internal EOSUserInfo UserInfo { get; private set; }
    internal EOSConnect Connect { get; private set; }
    internal EOSP2P P2P { get; private set; }
    internal EOSLobby Lobby { get; private set; }
    internal EOSSanctions Sanctions { get; private set; }
    internal EOSReports Reports { get; private set; }

    internal async Task<bool> InitializeAsync()
    {
        Platform = new EOSPlatform();

        if (!await Platform.InitializeAsync())
            return false;
        
        Auth = new EOSAuth(this, Platform.PlatformInterface.GetAuthInterface());
        
        if (!await Auth.InitializeAsync())
            return false;
        
        UserInfo = new EOSUserInfo(this, Platform.PlatformInterface.GetUserInfoInterface(), Auth.LocalAccountId);
        
        if (!await UserInfo.InitializeAsync())
            return false;

        Connect = new EOSConnect(this, Platform.PlatformInterface.GetConnectInterface());

        if (!await Connect.InitializeAsync())
            return false;

        P2P = new EOSP2P(this, Platform.PlatformInterface.GetP2PInterface(), Connect.LocalUserId);

        if (!await P2P.InitializeAsync())
            return false;

        Lobby = new EOSLobby(this, Platform.PlatformInterface.GetLobbyInterface(), Connect.LocalUserId);

        if (!await Lobby.InitializeAsync())
            return false;
        
        Sanctions = new EOSSanctions(this, Platform.PlatformInterface.GetSanctionsInterface(), Connect.LocalUserId);
        
        if (!await Sanctions.InitializeAsync())
            return false;
        
        Reports = new EOSReports(this, Platform.PlatformInterface.GetReportsInterface(), Connect.LocalUserId);
        if (!await Reports.InitializeAsync())
            return false;

        return true;
    }
    
    internal void Tick()
    {
        Platform?.Tick();
        Auth?.Tick();
        UserInfo?.Tick();
        Connect?.Tick();
        P2P?.Tick();
        Lobby?.Tick();
        Sanctions?.Tick();
        Reports?.Tick();
    }

    internal void Shutdown()
    {
        Reports?.Shutdown();
        Sanctions?.Shutdown();
        Lobby?.Shutdown();
        P2P?.Shutdown();
        Connect?.Shutdown();
        UserInfo?.Shutdown();
        Auth?.Shutdown();
        Platform?.Shutdown();
    }
}