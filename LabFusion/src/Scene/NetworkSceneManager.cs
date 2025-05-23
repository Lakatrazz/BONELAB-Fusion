using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Scene;

public static class NetworkSceneManager
{
    private static bool _purgatory = false;

    /// <summary>
    /// Determines if the player is in a "purgatory" state in the server.
    /// This means they are basically cut off from anything syncing such as spawnables, other players, events, etc.
    /// This is useful in cases such as a loading scene where you want the player to be able to use props but not interact with other players.
    /// </summary>
    public static bool Purgatory
    {
        get
        {
            return _purgatory;
        }
        set
        {
            if (value == _purgatory)
            {
                return;
            }

            _purgatory = value;

            OnPurgatoryChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Returns if the Local Player is the host of the active level.
    /// </summary>
    public static bool IsLevelHost => NetworkInfo.IsHost;

    /// <summary>
    /// Returns if the active level is networked. It will not be networked if there is no server or if <see cref="Purgatory"/> is enabled.
    /// </summary>
    public static bool IsLevelNetworked => NetworkInfo.HasServer && !Purgatory;

    /// <summary>
    /// Invoked when <see cref="Purgatory"/> changes.
    /// </summary>
    public static event Action<bool> OnPurgatoryChanged;

    /// <summary>
    /// Invoked when all players in the Local Player's level have finished loading. 
    /// If a player leaves while loading and all other players have finished loading, the event will still trigger.
    /// </summary>
    public static event Action OnAllPlayersLoaded;

    /// <summary>
    /// Invoked when a player loads into a level. Passes in the PlayerId and the level's barcode.
    /// </summary>
    public static event Action<PlayerId, string> OnPlayerLoadedIntoLevel;

    private static bool _allPlayersLoaded = false;

    internal static void OnInitializeMelon()
    {
        PlayerId.OnMetadataChangedEvent += OnMetadataChangedCallback;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
        MultiplayerHooking.OnMainSceneInitialized += OnMainSceneInitialized;
    }

    private static void OnMainSceneInitialized()
    {
        _allPlayersLoaded = false;

        CheckAllPlayersLoaded();
    }

    private static void OnPlayerLeft(PlayerId playerId)
    {
        CheckAllPlayersLoaded();
    }

    private static void OnMetadataChangedCallback(PlayerId playerId, string key, string value)
    {
        if (key == playerId.Metadata.Loading.Key)
        {
            var loading = playerId.Metadata.Loading.GetValue();

            if (!loading)
            {
                InvokePlayerLoaded(playerId, playerId.Metadata.LevelBarcode.GetValue());
            }
        }
    }

    private static void InvokePlayerLoaded(PlayerId playerId, string barcode)
    {
        OnPlayerLoadedIntoLevel?.InvokeSafe(playerId, barcode, "executing OnPlayerLoadedIntoLevel");

        CheckAllPlayersLoaded();
    }

    private static void CheckAllPlayersLoaded()
    {
        if (_allPlayersLoaded)
        {
            return;
        }

        var players = GetPlayersInCurrentLevel();

        foreach (var player in players)
        {
            if (player.Metadata.Loading.GetValue())
            {
                return;
            }
        }

        _allPlayersLoaded = true;

        OnAllPlayersLoaded?.InvokeSafe("executing OnAllPlayersLoaded");
    }

    /// <summary>
    /// Returns if this player is in the Local Player's current level.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool InCurrentLevel(PlayerId player)
    {
        if (player.IsMe)
        {
            return true;
        }

        if (!IsLevelNetworked)
        {
            return false;
        }

        return InLevel(player, FusionSceneManager.Barcode);
    }

    /// <summary>
    /// Returns if this player is in a specific level.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="barcode">The barcode of the level.</param>
    /// <returns></returns>
    public static bool InLevel(PlayerId player, string barcode)
    {
        return GetLevelBarcode(player) == barcode;
    }

    /// <summary>
    /// Gets the level barcode that a player is in.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns></returns>
    public static string GetLevelBarcode(PlayerId player)
    {
        return player.Metadata.LevelBarcode.GetValue();
    }

    /// <summary>
    /// Returns a list of all players in a specific level.
    /// </summary>
    /// <param name="barcode">The barcode level.</param>
    /// <returns></returns>
    public static List<PlayerId> GetPlayersInLevel(string barcode)
    {
        var players = new List<PlayerId>(PlayerIdManager.PlayerIds.Count);

        foreach (var player in PlayerIdManager.PlayerIds)
        {
            if (InLevel(player, barcode))
            {
                players.Add(player);
            }
        }

        return players;
    }

    /// <summary>
    /// Returns a list of all players in the Local Player's current level.
    /// </summary>
    /// <returns></returns>
    public static List<PlayerId> GetPlayersInCurrentLevel()
    {
        return GetPlayersInLevel(FusionSceneManager.Barcode);
    }
}
