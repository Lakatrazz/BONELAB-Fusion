using LabFusion.Entities;
using LabFusion.Player;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

public class TeamLogoManager
{
    private readonly Dictionary<Team, Texture> _teamToLogo = new();

    private TeamManager _teamManager = null;
    public TeamManager TeamManager => _teamManager;

    private bool _showTeammateLogos = true;

    /// <summary>
    /// Should the logos of teammates (same team) be shown? Defaults to true.
    /// </summary>
    public bool ShowTeammateLogos
    {
        get
        {
            return _showTeammateLogos;
        }
        set
        {
            _showTeammateLogos = value;

            SetDirty();
        }
    }

    private bool _showOpponentLogos = false;

    /// <summary>
    /// Should the logos of opponents (different team) be shown? Defaults to false.
    /// </summary>
    public bool ShowOpponentLogos
    {
        get
        {
            return _showOpponentLogos;
        }
        set
        {
            _showOpponentLogos = value;

            SetDirty();
        }
    }

    /// <summary>
    /// Registers the TeamLogoManager to a TeamManager. This is required for logos to be created properly.
    /// </summary>
    /// <param name="teamManager"></param>
    public void Register(TeamManager teamManager)
    {
        _teamManager = teamManager;

        teamManager.OnAssignedToTeam += OnAssignedToTeam; 
        teamManager.OnRemovedFromTeam += OnRemovedFromTeam;
    }

    /// <summary>
    /// Unregisters the TeamLogoManager. Make sure to call this when the TeamLogoManager will no longer be used.
    /// </summary>
    public void Unregister()
    {
        _teamManager.OnAssignedToTeam -= OnAssignedToTeam;
        _teamManager.OnRemovedFromTeam -= OnRemovedFromTeam;

        _teamManager = null;
    }

    /// <summary>
    /// Sets the TeamLogoManager dirty so that all logos are updated.
    /// </summary>
    public void SetDirty()
    {
        foreach (var player in PlayerIDManager.PlayerIDs)
        {
            if (!NetworkPlayerManager.TryGetPlayer(player, out var networkPlayer))
            {
                continue;
            }

            var team = TeamManager.GetPlayerTeam(player);

            if (team == null)
            {
                networkPlayer.Icon.Texture = null;
                networkPlayer.Icon.Visible = false;
                continue;
            }

            var logo = GetLogo(team);

            if (logo == null)
            {
                networkPlayer.Icon.Texture = null;
                networkPlayer.Icon.Visible = false;
                continue;
            }

            bool teammate = TeamManager.GetLocalTeam() == team;
            bool visible = (teammate && ShowTeammateLogos) || (!teammate && ShowOpponentLogos);

            networkPlayer.Icon.Texture = logo;
            networkPlayer.Icon.Visible = visible;
        }
    }

    private void OnAssignedToTeam(PlayerID player, Team team)
    {
        SetDirty();
    }

    private void OnRemovedFromTeam(PlayerID player, Team team)
    {
        SetDirty();
    }

    public void ClearTeams()
    {
        _teamToLogo.Clear();

        SetDirty();
    }

    public void SetLogo(Team team, Texture logo)
    {
        _teamToLogo[team] = logo;

        SetDirty();
    }

    public Texture GetLogo(Team team)
    {
        if (_teamToLogo.TryGetValue(team, out var logo))
        {
            return logo;
        }

        return null;
    }
}