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

    private readonly HashSet<byte> _players = new();
    public HashSet<byte> Players => _players;

    public int PlayerCount => _players.Count;

    public bool HasPlayer(byte smallID)
    {
        return Players.Contains(smallID);
    }

    public bool HasPlayer(PlayerID playerID)
    {
        return HasPlayer(playerID.SmallID);
    }

    public void ForceAddPlayer(byte smallID)
    {
        _players.Add(smallID);
    }

    public void ForceAddPlayer(PlayerID playerID)
    {
        _players.Add(playerID.SmallID);
    }

    public void ForceRemovePlayer(byte smallID)
    {
        _players.Remove(smallID);
    }

    public void ForceRemovePlayer(PlayerID playerID)
    {
        _players.Remove(playerID.SmallID);
    }
}