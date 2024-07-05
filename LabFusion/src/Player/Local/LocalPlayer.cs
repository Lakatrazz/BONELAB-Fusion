#nullable enable

using Il2CppSLZ.Interaction;

using LabFusion.Entities;
using LabFusion.Network;

namespace LabFusion.Player;

public delegate void PlayerGrabDelegate(Hand hand, Grip grip);

public static class LocalPlayer
{
    public static PlayerGrabDelegate? OnGrab { get; set; }
    public static PlayerGrabDelegate? OnRelease { get; set; }

    public static NetworkPlayer? GetNetworkPlayer()
    {
        if (!NetworkInfo.HasServer)
        {
            return null;
        }

        if (NetworkPlayerManager.TryGetPlayer(PlayerIdManager.LocalId, out var player))
        {
            return player;
        }

        return null;
    }
}