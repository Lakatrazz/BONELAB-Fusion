using LabFusion.Player;
using LabFusion.SDK.Metadata;

namespace LabFusion.SDK.Gamemodes;

public sealed class PlayerScoreKeeper : ScoreKeeper<PlayerID>
{
    public override string GetKeyWithProperty(PlayerID property)
    {
        return KeyHelper.GetKeyFromPlayer(Key, property);
    }

    public override PlayerID GetPropertyWithKey(string key)
    {
        return KeyHelper.GetPlayerFromKey(key);
    }

    /// <summary>
    /// Returns all players ordered from highest score to lowest score.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<PlayerID> GetPlacedPlayers()
    {
        List<PlayerID> leaders = new(PlayerIDManager.PlayerIDs);
        leaders = leaders.OrderBy(id => GetScore(id)).ToList();
        leaders.Reverse();

        return leaders;
    }

    /// <summary>
    /// Returns all players ordered from lowest score to the highest score.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<PlayerID> GetOrderedPlayers()
    {
        List<PlayerID> leaders = new(PlayerIDManager.PlayerIDs);
        leaders = leaders.OrderBy(id => GetScore(id)).ToList();

        return leaders;
    }

    /// <summary>
    /// Returns the player at a specified place starting at 0, or null if none are found.
    /// </summary>
    /// <param name="place">The place to check, starting at index 0.</param>
    /// <returns>The player at the specified place.</returns>
    public PlayerID GetPlayerByPlace(int place)
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
    public int GetPlace(PlayerID player)
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