using LiteNetLib.Utils;

namespace LabFusion.Network.Proxy;

public struct ProxyLobbyRequestParameters
{
    public MatchmakerFilters Filters;

    public int VersionMajor;

    public int VersionMinor;

    public string LobbyCode;

    public readonly void Put(NetDataWriter writer)
    {
        writer.Put(Filters.FilterFull);
        writer.Put(Filters.FilterMismatchingVersions);

        writer.Put(VersionMajor);
        writer.Put(VersionMinor);

        bool hasCode = LobbyCode != null;

        writer.Put(hasCode);

        if (hasCode)
        {
            writer.Put(LobbyCode);
        }
    }
}