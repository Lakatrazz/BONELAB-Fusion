using LabFusion.Representation;

using LabFusion.SDK.Metadata;

using Random = UnityEngine.Random;

namespace LabFusion.SDK.Gamemodes;

public class TeamManager
{
    private Gamemode _gamemode = null;
    public Gamemode Gamemode => _gamemode;

    private readonly HashSet<Team> _teams = new();
    public HashSet<Team> Teams => _teams;

    public event Action<PlayerId, Team> OnAssignedToTeam, OnRemovedFromTeam;

    private readonly Dictionary<PlayerId, MetadataVariable> _playersToTeam = new();

    public void Register(Gamemode gamemode)
    {
        _gamemode = gamemode;
        gamemode.Metadata.OnMetadataChanged += OnMetadataChanged;
        gamemode.Metadata.OnMetadataRemoved += OnMetadataRemoved;
    }

    public void Unregister()
    {
        _gamemode.Metadata.OnMetadataChanged -= OnMetadataChanged;
        _gamemode.Metadata.OnMetadataRemoved -= OnMetadataRemoved;
        _gamemode = null;
    }

    private void OnMetadataChanged(string key, string value)
    {
        // Check if this is a team key
        if (!KeyHelper.KeyMatchesVariable(key, CommonKeys.TeamKey))
        {
            return;
        }

        var player = KeyHelper.GetPlayerFromKey(key);

        var teamVariable = new MetadataVariable(key, Gamemode.Metadata);

        _playersToTeam[player] = teamVariable;

        // Invoke team change event
        var team = GetTeamByName(value);
        
        if (team != null)
        {
            OnAssignedToTeam?.Invoke(player, team);
            team.ForceAddPlayer(player);
        }
    }

    private void OnMetadataRemoved(string key, string value)
    {
        // Check if this is a team key
        if (!KeyHelper.KeyMatchesVariable(key, CommonKeys.TeamKey))
        {
            return;
        }

        var player = KeyHelper.GetPlayerFromKey(key);

        _playersToTeam.Remove(player);

        // Invoke team remove event
        var team = GetTeamByName(value);

        if (team != null)
        {
            OnRemovedFromTeam?.Invoke(player, team);
            team.ForceRemovePlayer(player);
        }
    }

    public void AddTeam(Team team)
    {
        _teams.Add(team);
    }

    public void RemoveTeam(Team team)
    {
        _teams.Remove(team);
    }

    public void ClearTeams()
    {
        foreach (var player in PlayerIdManager.PlayerIds)
        {
            TryUnassignTeam(player);
        }

        foreach (var team in _teams.ToArray())
        {
            RemoveTeam(team);
        }
    }

    public bool TryAssignTeam(PlayerId player, Team team)
    {
        var playerKey = KeyHelper.GetKeyFromPlayer(CommonKeys.TeamKey, player);
        return Gamemode.Metadata.TrySetMetadata(playerKey, team.TeamName);
    }

    public bool TryUnassignTeam(PlayerId player)
    {
        var playerKey = KeyHelper.GetKeyFromPlayer(CommonKeys.TeamKey, player);
        return Gamemode.Metadata.TryRemoveMetadata(playerKey);
    }

    public void UnassignAllPlayers()
    {
        foreach (var player in PlayerIdManager.PlayerIds)
        {
            TryUnassignTeam(player);
        }
    }

    public Team GetTeamByName(string name)
    {
        foreach (var team in Teams)
        {
            if (team.TeamName == name)
            {
                return team;
            }
        }

        return null;
    }

    public Team GetPlayerTeam(PlayerId player)
    {
        if (!_playersToTeam.TryGetValue(player, out var teamVariable))
        {
            return null;
        }

        return GetTeamByName(teamVariable.GetValue());
    }

    public Team GetLocalTeam()
    {
        return GetPlayerTeam(PlayerIdManager.LocalId);
    }
    
    public Team GetRandomTeam()
    {
        return Teams.ElementAt(Random.RandomRangeInt(0, Teams.Count));
    }

    public Team GetTeamWithFewestPlayers()
    {
        int lowestPlayers = int.MaxValue;
        Team lowestTeam = null;

        foreach (var team in Teams)
        {
            if (team.PlayerCount < lowestPlayers)
            {
                lowestPlayers = team.PlayerCount;
                lowestTeam = team;
            }
        }

        return lowestTeam;
    }
}
