using Epic.OnlineServices;
using System.Collections;

namespace LabFusion.Network.EpicGames;

internal abstract class EOSFriendsInterface
{
    /// <summary>
    /// The external account type this interface handles.
    /// </summary>
    internal abstract ExternalAccountType AccountType { get; }

    /// <summary>
    /// Fetches the raw external account IDs of the local user's friends on this platform.
    /// </summary>
    internal abstract IEnumerator GetFriendExternalIdsAsync(Action<IReadOnlyList<string>> onComplete);

    /// <summary>
    /// Called when the friend manager is shutting down.
    /// </summary>
    internal virtual void OnShutdown() { }
}