using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.SDK.Gamemodes;

public class Team
{
    public string TeamName { get; set; }

    private string _displayName = null;
    public string DisplayName 
    {
        get => _displayName ?? TeamName;
        set => _displayName = value;
    }

    public Team(string teamName)
    {
        TeamName = teamName;
    }

    public HashSet<ClientSmallID> Players { get; } = new();

    public int PlayerCount => Players.Count;

    public bool HasPlayer(ClientSmallID smallID)
    {
        return Players.Contains(smallID);
    }

    public bool HasPlayer(PlayerID playerID)
    {
        return HasPlayer(playerID.SmallID);
    }

    public void ForceAddPlayer(ClientSmallID smallID)
    {
        Players.Add(smallID);
    }

    public void ForceAddPlayer(PlayerID playerID)
    {
        Players.Add(playerID.SmallID);
    }

    public void ForceRemovePlayer(ClientSmallID smallID)
    {
        Players.Remove(smallID);
    }

    public void ForceRemovePlayer(PlayerID playerID)
    {
        Players.Remove(playerID.SmallID);
    }
}