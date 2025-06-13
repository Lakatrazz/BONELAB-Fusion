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

        var data = LevelRequestData.Create(PlayerIDManager.LocalSmallID, crate.Barcode.ID, crate.Title);

        MessageRelay.RelayNative(data, NativeMessageTag.LevelRequest, CommonMessageRoutes.ReliableToServer);
    }

    public static void SendLevelLoad(string barcode, string loadBarcode, ulong userId)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        using var writer = NetWriter.Create();

        var data = new LevelLoadData()
        {
            LevelBarcode = barcode,
            LoadingScreenBarcode = loadBarcode,
        };

        writer.SerializeValue(ref data);

        using var message = NetMessage.Create(NativeMessageTag.SceneLoad, writer, CommonMessageRoutes.None);
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

        var data = new LevelLoadData()
        {
            LevelBarcode = barcode,
            LoadingScreenBarcode = loadBarcode,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.SceneLoad, CommonMessageRoutes.ReliableToOtherClients);
    }
}
