using LabFusion.Network;
using LabFusion.Player;

using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Network.Serialization;

namespace LabFusion.Senders;

public static class LoadSender
{
    public static void SendLevelRequest(LevelCrate crate)
    {
        if (NetworkInfo.IsHost)
        {
            return;
        }

        var data = LevelRequestData.Create(PlayerIdManager.LocalSmallId, crate.Barcode.ID, crate.Title);

        MessageRelay.RelayNative(data, NativeMessageTag.LevelRequest, NetworkChannel.Reliable, RelayType.ToServer);
    }

    public static void SendLevelLoad(string barcode, string loadBarcode, ulong userId)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        using var writer = NetWriter.Create();
        var data = SceneLoadData.Create(barcode, loadBarcode);
        writer.SerializeValue(ref data);

        using var message = FusionMessage.Create(NativeMessageTag.SceneLoad, writer);
        MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
    }

    public static void SendLoadingState(bool isLoading)
    {
        LocalPlayer.Metadata.Loading.SetValue(isLoading);
    }

    public static void SendLevelLoad(string barcode, string loadBarcode)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        var data = SceneLoadData.Create(barcode, loadBarcode);

        MessageRelay.RelayNative(data, NativeMessageTag.SceneLoad, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}
