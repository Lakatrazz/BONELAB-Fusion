using Epic.OnlineServices;
using Steamworks;
using System.Collections;

namespace LabFusion.Network.EpicGames;

internal class EOSSteamFriends : EOSFriendsInterface
{
    internal override ExternalAccountType AccountType => ExternalAccountType.Steam;

    internal override IEnumerator GetFriendExternalIdsAsync(Action<IReadOnlyList<string>> onComplete)
    {
        if (!SteamClient.IsValid)
        {
            onComplete?.Invoke(Array.Empty<string>());
            yield break;
        }

        var friendIds = new List<string>();

        foreach (var friend in SteamFriends.GetFriends())
        {
            friendIds.Add(friend.Id.Value.ToString());
        }

        onComplete?.Invoke(friendIds);
    }
}