using LabFusion.Network;
using LabFusion.Player;

using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Network.Serialization;

namespace LabFusion.Senders;

public static class LoadSender
{
    public static void SendLevelRequest(LevelCrate crate)
    {
        if (NetworkInfo.IsServer)
        {
            return;
        }

        var data = LevelRequestData.Create(PlayerIdManager.LocalSmallId, crate.Barcode.ID, crate.Title);

        MessageRelay.RelayNative(data, NativeMessageTag.LevelRequest, NetworkChannel.Reliable, RelayType.ToServer);
    }

    public static void SendLevelLoad(string barcode, string loadBarcode, ulong userId)
    {
        if (!NetworkInfo.IsServer)
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
        if (!NetworkInfo.HasServer || PlayerIdManager.LocalId == null)
        {
            return;
        }

        // Set the loading metadata
        PlayerIdManager.LocalId.Metadata.TrySetMetadata(MetadataHelper.LoadingKey, isLoading.ToString());
    }

    public static void SendLevelLoad(string barcode, string loadBarcode)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        var data = SceneLoadData.Create(barcode, loadBarcode);

        MessageRelay.RelayNative(data, NativeMessageTag.SceneLoad, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}
