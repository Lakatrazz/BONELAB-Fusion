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

    public Team(string teamName)
    {
        TeamName = teamName;
    }

    private readonly HashSet<PlayerID> _players = new();
    public HashSet<PlayerID> Players => _players;

    public int PlayerCount => _players.Count;

    public bool HasPlayer(PlayerID player)
    {
        return Players.Contains(player);
    }

    public void ForceAddPlayer(PlayerID player)
    {
        _players.Add(player);
    }

    public void ForceRemovePlayer(PlayerID player)
    {
        _players.Remove(player);
    }
}