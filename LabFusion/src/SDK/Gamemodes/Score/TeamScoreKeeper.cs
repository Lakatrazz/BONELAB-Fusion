using LabFusion.SDK.Metadata;

namespace LabFusion.SDK.Gamemodes;

public sealed class TeamScoreKeeper : ScoreKeeper<Team>
{
    private readonly TeamManager _teamManager;
    public TeamManager TeamManager => _teamManager;

    public TeamScoreKeeper(TeamManager teamManager)
    {
        _teamManager = teamManager;
    }

    public override string GetKeyWithProperty(Team property)
    {
        return KeyHelper.GetKeyWithProperty(Key, property.TeamName);
    }

    public override Team GetPropertyWithKey(string key)
    {
        return TeamManager.GetTeamByName(KeyHelper.GetPropertyFromKey(key));
    }
}