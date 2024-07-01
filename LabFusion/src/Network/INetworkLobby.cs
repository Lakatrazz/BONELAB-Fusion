using LabFusion.Extensions;

namespace LabFusion.Network;

/// <summary>
/// A set of variables for Network Lobbies that should never change.
/// </summary>
public static class LobbyConstants
{
    private const string _internalPrefix = "BONELAB_FUSION_";
    public const string HasServerOpenKey = _internalPrefix + "HasServerOpen";

    public const string KeyCollectionKey = _internalPrefix + "KeyCollection";
}

public interface INetworkLobby
{
    /// <summary>
    /// Writes a list of all keys to the metadata.
    /// </summary>
    void WriteKeyCollection();

    /// <summary>
    /// Sets data associated with the lobby.
    /// </summary>
    /// <param name="key">The key the data is stored at.</param>
    /// <param name="value">The data that is stored.</param>
    void SetMetadata(string key, string value);

    /// <summary>
    /// Tries to get data associated with the lobby.
    /// </summary>
    /// <param name="key">The key the data is stored at.</param>
    /// <param name="value">The data to be stored.</param>
    /// <returns>If the data exists.</returns>
    bool TryGetMetadata(string key, out string value);

    /// <summary>
    /// Gets data associated with the lobby.
    /// </summary>
    /// <param name="key">The key the data is stored at.</param>
    /// <returns>The data.</returns>
    string GetMetadata(string key);

    /// <summary>
    /// Creates a delegate that, when invoked, makes the local server join a lobby.
    /// </summary>
    /// <param name="lobbyId">The lobby to join.</param>
    /// <returns>The join delegate.</returns>
    Action CreateJoinDelegate(ulong lobbyId);
}

public abstract class NetworkLobby : INetworkLobby
{
    private readonly List<string> _keyCollection = new();

    public void WriteKeyCollection()
    {
        SetMetadata(LobbyConstants.KeyCollectionKey, _keyCollection.Contract());
    }

    /// <summary>
    /// Saves the key to the key collection. Should be called on every SetMetadata.
    /// </summary>
    /// <param name="key"></param>
    protected void SaveKey(string key)
    {
        // Don't save the key collection
        if (key == LobbyConstants.KeyCollectionKey)
            return;

        // If it already exists don't add it
        if (!_keyCollection.Contains(key))
            _keyCollection.Add(key);
    }

    public abstract string GetMetadata(string key);
    public abstract void SetMetadata(string key, string value);
    public abstract bool TryGetMetadata(string key, out string value);
    public abstract Action CreateJoinDelegate(ulong lobbyId);
}