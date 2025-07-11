using System.Text.Json.Serialization;
using Il2CppSystem.Collections.Specialized;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Friends;

// Just a mockup for how friending could be done, liekely to be fully replaced when merged
[Serializable]
public class FriendListing
{
    [JsonPropertyName("player")]
    public PlayerInfo Player { get; set; } = null;

    [JsonPropertyName("networklayer")]
    public string NetworkLayerTitle { get; set; } = null;
}

[Serializable]
public class FriendList
{
    [JsonPropertyName("friends")]
    public List<FriendListing> Friends { get; set; } = new();
}

public class FriendManager
{
    private static FriendList _friends = new FriendList();

    private const string FileName = "friends.json";

    public static void ReadFile()
    {
        _friends = new();

        var deserializedList = DataSaver.ReadJsonFromFile<FriendList>(FileName);

        if (deserializedList != null)
        {
            _friends = deserializedList;
        }
    }

    private static void WriteFile()
    {
        DataSaver.WriteJsonToFile(FileName, _friends);
    }

    /// <summary>
    /// Gets all friends for the current network layer.
    /// If there is no active layer, returns an empty array.
    /// </summary>
    /// <returns></returns>
    public static PlayerInfo[] GetFriends()
    {
        // Return if there isn't an active layer
        if (NetworkLayerDeterminer.LoadedLayer == null)
            return Array.Empty<PlayerInfo>();

        return _friends.Friends.Where(f => f.NetworkLayerTitle == NetworkLayerDeterminer.LoadedTitle)
                                .Select(f => f.Player)
                                .ToArray();
    }

    /// <summary>
    /// Returns true if the given string ID is a friend on the current network layer or platform. (Steam/Epic/etc.)
    /// </summary>
    /// <param name="stringID"></param>
    /// <returns></returns>
    public static bool IsFriend(string stringID)
    {
        // Return if there isn't an active layer
        if (NetworkLayerDeterminer.LoadedLayer == null)
            return false;

        return _friends.Friends.Any(f => f.Player.StringID == stringID && f.NetworkLayerTitle == NetworkLayerDeterminer.LoadedTitle)
        || NetworkHelper.IsFriend(stringID); // Check the network layer for friends as well
    }

    /// <summary>
    /// Adds a friend to the current network layer.
    /// </summary>
    /// <param name="player"></param>
    public static void AddFriend(PlayerInfo player)
    {
        // Return if there isn't an active layer
        if (NetworkLayerDeterminer.LoadedLayer == null)
            return;

        _friends.Friends.Add(new FriendListing { Player = player, NetworkLayerTitle = NetworkLayerDeterminer.LoadedTitle });

        WriteFile();
    }

    /// <summary>
    /// Removes a friend from the current network layer.
    /// </summary>
    /// <param name="player"></param>
    public static void RemoveFriend(string stringID)
    {
        _friends.Friends.RemoveAll(f => f.Player.StringID == stringID);

        WriteFile();
    }
}
