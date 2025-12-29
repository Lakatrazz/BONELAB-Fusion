using LabFusion.Network;

namespace LabFusion.SDK.Lobbies;

public delegate bool GenericLobbyDelegate(INetworkLobby lobby, LobbyMetadataInfo info);

public class GenericLobbyFilter : ILobbyFilter
{
    public GenericLobbyDelegate OnFilter;

    public string Title;

    private bool _active = false;

    public GenericLobbyFilter(string title, GenericLobbyDelegate onFilter)
    {
        this.Title = title;
        this.OnFilter = onFilter;
    }

    public bool FilterLobby(INetworkLobby lobby, LobbyMetadataInfo info)
    {
        return OnFilter(lobby, info);
    }

    public string GetTitle()
    {
        return Title;
    }

    public bool IsActive()
    {
        return _active;
    }

    public void SetActive(bool active)
    {
        _active = active;
    }
}
