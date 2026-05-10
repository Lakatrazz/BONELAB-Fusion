using LabFusion.Entities;
using LabFusion.Extensions;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Player;

public delegate void NetworkPlayerAvatarDelegate(NetworkPlayer networkPlayer, Avatar avatar, string barcode);

public static class NetworkAvatarManager
{
    /// <summary>
    /// Invoked when the avatar of a NetworkPlayer changes. This is also invoked for the Local Player's NetworkPlayer.
    /// </summary>
    public static event NetworkPlayerAvatarDelegate OnNetworkPlayerAvatarChanged;

    internal static void InvokeAvatarChanged(NetworkPlayer networkPlayer, Avatar avatar, string barcode)
    {
        networkPlayer.OnAvatarBarcodeChanged(barcode);

        OnNetworkPlayerAvatarChanged?.InvokeSafe(networkPlayer, avatar, barcode, "executing NetworkAvatarManager.OnNetworkPlayerAvatarChanged");
    }
}
