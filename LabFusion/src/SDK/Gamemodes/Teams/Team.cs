using LabFusion.Player;

namespace LabFusion.SDK.Gamemodes;

public class Team
{
    public string TeamName { get; set; }

    private string _displayName = null;
    public string DisplayName 
    {
        get
        {
            return _displayName ?? TeamName;
        }
        set
        {
            _displayName = value;
        }
    }

    private readonly HashSet<PlayerId> _players = new();
    public HashSet<PlayerId> Players => _players;

    public int PlayerCount => _players.Count;

    public void ForceAddPlayer(PlayerId player)
    {
        _players.Add(player);
    }

    public void ForceRemovePlayer(PlayerId player)
    {
        _players.Remove(player);
    }
}