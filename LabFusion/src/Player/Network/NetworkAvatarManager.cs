using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;

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

        // Update the use time of the avatar's crate
        PalletUseHistoryManager.MarkCrateUsed(AssetWarehouseSearcher.GetCrate<AvatarCrate>(new(barcode)));
    }
}
