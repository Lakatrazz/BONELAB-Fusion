using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Network.Serialization;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Senders;

public static class LoadSender
{
    public static void SendLevelRequest(LevelCrate crate)
    {
        if (ServerManager.IsServerRunning)
        {
            return;
        }

        var data = new LevelRequestData()
        {
            Barcode = crate.Barcode.ID,
            Title = crate.Title,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.LevelRequest, CommonMessageRoutes.ReliableToServer);
    }

    public static void SendLevelLoad(string barcode, string loadBarcode, ClientPlatformID userId)
    {
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        using var writer = NetWriter.Create();

        var data = new LevelLoadData()
        {
            LevelReference = new(barcode),
            LoadingScreenBarcode = loadBarcode,
        };

        writer.SerializeValue(ref data);

        using var message = NetMessage.CreateNative(NativeMessageTag.SceneLoad, writer, CommonMessageRoutes.None);
        ServerManager.SendToClient(message, NetworkChannel.Reliable, userId);
    }

    public static void SendLoadingState(bool isLoading)
    {
        LocalPlayer.Metadata.Loading.SetValue(isLoading);
    }

    public static void SendLevelLoad(string barcode, string loadBarcode)
    {
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        var data = new LevelLoadData()
        {
            LevelReference = new(barcode),
            LoadingScreenBarcode = loadBarcode,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.SceneLoad, CommonMessageRoutes.ReliableToOtherClients);
    }
}
