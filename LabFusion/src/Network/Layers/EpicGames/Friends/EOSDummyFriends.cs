using Epic.OnlineServices;
using System.Collections;

namespace LabFusion.Network.EpicGames;

internal class EOSDummyFriends : EOSFriendsInterface
{
    internal override ExternalAccountType AccountType => ExternalAccountType.Epic;

    internal override IEnumerator GetFriendExternalIdsAsync(Action<IReadOnlyList<string>> onComplete)
    {
        onComplete?.Invoke(Array.Empty<string>());
        yield break;
    }
}