using LabFusion.Player;
using LabFusion.SDK.Metadata;
using LabFusion.Utilities;

namespace LabFusion.SDK.Gamemodes;

public sealed class PlayerScoreKeeper : ScoreKeeper<byte>
{
    public event Action<PlayerID, int> OnPlayerScoreChanged;

    protected override void OnRegistered()
    {
        OnScoreChanged += OnByteScoreChanged;

        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
    }

    protected override void OnUnregistered()
    {
        OnScoreChanged -= OnByteScoreChanged;

        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeft;
    }

    private void OnByteScoreChanged(byte smallID, int score)
    {
        var playerID = PlayerIDManager.GetPlayerID(smallID);

        if (playerID != null)
        {
            OnPlayerScoreChanged?.InvokeSafe(playerID, score, "executing PlayerScoreKeeper.OnPlayerScoreChanged");
        }
    }

    private void OnPlayerLeft(PlayerID playerID)
    {
        RemoveScoreMetadata(playerID.SmallID);
    }

    public override string GetKeyWithProperty(byte property)
    {
        return KeyHelper.GetKeyFromPlayer(Key, property);
    }

    public override byte GetPropertyWithKey(string key)
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
        leaders = leaders.OrderBy(playerID => GetScore(playerID.SmallID)).ToList();
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
        leaders = leaders.OrderBy(playerID => GetScore(playerID.SmallID)).ToList();

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
    /// <param name="playerID">The player to get the index of.</param>
    /// <returns></returns>
    public int GetPlace(PlayerID playerID)
    {
        var players = GetPlacedPlayers();

        if (players == null)
        {
            return -1;
        }

        for (var i = 0; i < players.Count; i++)
        {
            if (players[i] == playerID)
            {
                return i;
            }
        }

        return -1;
    }

    public void SetScore(PlayerID playerID, int score) => SetScore(playerID.SmallID, score);

    public int GetScore(PlayerID playerID) => GetScore(playerID.SmallID);

    public void AddScore(PlayerID playerID, int amount = 1) => AddScore(playerID.SmallID, amount);

    public void SubtractScore(PlayerID playerID, int amount = 1) => SubtractScore(playerID.SmallID, amount);
}