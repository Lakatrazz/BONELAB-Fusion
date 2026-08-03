using LabFusion.Player;
using LabFusion.SDK.Metadata;
using LabFusion.Utilities;
using LabFusion.Extensions;
using LabFusion.Network;

namespace LabFusion.SDK.Gamemodes;

public sealed class PlayerScoreKeeper : ScoreKeeper<byte>
{
    public event Action<PlayerID, int> PlayerScoreChanged;

    protected override void OnRegistered()
    {
        ScoreChanged += OnByteScoreChanged;

        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
    }

    protected override void OnUnregistered()
    {
        ScoreChanged -= OnByteScoreChanged;

        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeft;
    }

    private void OnByteScoreChanged(byte smallID, int score)
    {
        var playerID = PlayerIDManager.GetPlayerID(new ClientSmallID(smallID));

        if (playerID != null)
        {
            PlayerScoreChanged?.InvokeSafe(playerID, score, "executing PlayerScoreKeeper.OnPlayerScoreChanged");
        }
    }

    private void OnPlayerLeft(PlayerID playerID)
    {
        RemoveScoreMetadata((byte)playerID.SmallID);
    }

    public override string GetKeyWithProperty(byte property)
    {
        return KeyHelper.GetKeyFromPlayer(Key, new ClientSmallID(property));
    }

    public override byte GetPropertyWithKey(string key)
    {
        return (byte)KeyHelper.GetPlayerFromKey(key);
    }

    /// <summary>
    /// Returns all players ordered from highest score to lowest score.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<PlayerID> GetPlacedPlayers()
    {
        List<PlayerID> leaders = new(PlayerIDManager.PlayerIDs);
        leaders = leaders.OrderBy(playerID => GetScore((byte)playerID.SmallID)).ToList();
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
        leaders = leaders.OrderBy(playerID => GetScore((byte)playerID.SmallID)).ToList();

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
        if (playerID == null)
        {
            return -1;
        }

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

    public void SetScore(PlayerID playerID, int score)
    {
        if (playerID == null)
        {
            return;
        }

        SetScore((byte)playerID.SmallID, score); 
    }

    public int GetScore(PlayerID playerID) 
    { 
        if (playerID == null)
        {
            return 0;
        }

        return GetScore((byte)playerID.SmallID); 
    }

    public void AddScore(PlayerID playerID, int amount = 1) 
    { 
        if (playerID == null)
        {
            return;
        }

        AddScore((byte)playerID.SmallID, amount); 
    }

    public void SubtractScore(PlayerID playerID, int amount = 1) 
    { 
        if (playerID == null)
        {
            return;
        }

        SubtractScore((byte)playerID.SmallID, amount); 
    }
}