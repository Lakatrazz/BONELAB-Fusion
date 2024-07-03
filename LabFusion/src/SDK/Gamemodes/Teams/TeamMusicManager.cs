using LabFusion.Marrow;

namespace LabFusion.SDK.Gamemodes;

public class TeamMusicManager
{
    private readonly Dictionary<Team, TeamMusic> _teamToMusic = new();
    
    public AudioReference TieMusic { get; set; }

    public void ClearTeams()
    {
        _teamToMusic.Clear();
    }

    public void SetMusic(Team team, TeamMusic music)
    {
        _teamToMusic[team] = music;
    }

    public TeamMusic GetMusic(Team team)
    {
        if (_teamToMusic.TryGetValue(team, out var teamMusic))
        {
            return teamMusic;
        }

        var newMusic = new TeamMusic();
        SetMusic(team, newMusic);

        return newMusic;
    }
}
