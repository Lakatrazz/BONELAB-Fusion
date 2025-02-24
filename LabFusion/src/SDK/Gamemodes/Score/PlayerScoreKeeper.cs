using LabFusion.Player;
using LabFusion.SDK.Metadata;

namespace LabFusion.SDK.Gamemodes;

public sealed class PlayerScoreKeeper : ScoreKeeper<PlayerId>
{
    public override string GetKeyWithProperty(PlayerId property)
    {
        return KeyHelper.GetKeyFromPlayer(Key, property);
    }

    public override PlayerId GetPropertyWithKey(string key)
    {
        return KeyHelper.GetPlayerFromKey(key);
    }

    /// <summary>
    /// Returns all players ordered from highest score to lowest score.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<PlayerId> GetPlacedPlayers()
    {
        List<PlayerId> leaders = new(PlayerIdManager.PlayerIds);
        leaders = leaders.OrderBy(id => GetScore(id)).ToList();
        leaders.Reverse();

        return leaders;
    }

    /// <summary>
    /// Returns all players ordered from lowest score to the highest score.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<PlayerId> GetOrderedPlayers()
    {
        List<PlayerId> leaders = new(PlayerIdManager.PlayerIds);
        leaders = leaders.OrderBy(id => GetScore(id)).ToList();

        return leaders;
    }

    /// <summary>
    /// Returns the player at a specified place starting at 0, or null if none are found.
    /// </summary>
    /// <param name="place">The place to check, starting at index 0.</param>
    /// <returns>The player at the specified place.</returns>
    public PlayerId GetPlayerByPlace(int place)
    {
        var players = GetPlacedPlayers();

        if (players != null && players.Count > place)
        {
            return players[place];
        }

        return null;
    }

    /// <summary>
    /// Returns the placement of a player starting at index 0.
    /// </summary>
    /// <param name="player">The player to get the index of.</param>
    /// <returns></returns>
    public int GetPlace(PlayerId player)
    {
        var players = GetPlacedPlayers();

        if (players == null)
        {
            return -1;
        }

        for (var i = 0; i < players.Count; i++)
        {
            if (players[i] == player)
            {
                return i;
            }
        }

        return -1;
    }
}