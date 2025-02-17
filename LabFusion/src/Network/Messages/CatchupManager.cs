using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network;

/// <summary>
/// Allows for setting up messages that will be sent to new players as they join.
/// </summary>
public static class CatchupManager
{
    /// <summary>
    /// Callback invoked when a new player joins the server and needs to be caught up on past server messages.
    /// </summary>
    public static event Action<PlayerId> OnPlayerServerCatchup;

    internal static void InvokePlayerServerCatchup(PlayerId playerId) => OnPlayerServerCatchup.InvokeSafe(playerId, "executing OnPlayerCatchup hook");
}
