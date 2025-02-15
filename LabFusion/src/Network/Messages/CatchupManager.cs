using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network;

/// <summary>
/// Allows for setting up messages that will be sent to new players as they join.
/// </summary>
public static class CatchupManager
{
    public static event Action<PlayerId> OnPlayerCatchup;

    internal static void InvokePlayerCatchup(PlayerId playerId) => OnPlayerCatchup.InvokeSafe(playerId, "executing OnPlayerCatchup hook");
}
