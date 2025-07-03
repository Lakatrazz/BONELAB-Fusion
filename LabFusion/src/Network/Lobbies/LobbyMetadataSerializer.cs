namespace LabFusion.Network;

public static class LobbyMetadataSerializer
{
    public static void WriteInfo(INetworkLobby lobby)
    {
        LobbyMetadataInfo.Create().Write(lobby);
    }

    public static LobbyMetadataInfo ReadInfo(INetworkLobby lobby)
    {
        try
        {
            return LobbyMetadataInfo.Read(lobby);
        }
        catch (Exception e)
        {
#if DEBUG
            FusionLogger.LogException("reading lobby info", e);
#endif

            return new LobbyMetadataInfo() { HasServerOpen = false };
        }
    }
}