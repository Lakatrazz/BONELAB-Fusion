using LabFusion.Network;
using LabFusion.Representation;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Senders
{
    public static class LoadSender
    {
        public static void SendLevelRequest(LevelCrate crate)
        {
            if (NetworkInfo.IsServer)
                return;

            using FusionWriter writer = FusionWriter.Create();
            var data = LevelRequestData.Create(PlayerIdManager.LocalSmallId, crate.Barcode, crate.Title);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.LevelRequest, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        public static void SendLevelLoad(string barcode, string loadBarcode, ulong userId)
        {
            if (!NetworkInfo.IsServer)
                return;

            using FusionWriter writer = FusionWriter.Create(SceneLoadData.GetSize(barcode, loadBarcode));
            var data = SceneLoadData.Create(barcode, loadBarcode);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.SceneLoad, writer);
            MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
        }

        public static void SendLoadingState(bool isLoading)
        {
            if (!NetworkInfo.HasServer || PlayerIdManager.LocalId == null)
                return;

            // Set the loading metadata
            PlayerIdManager.LocalId.TrySetMetadata(MetadataHelper.LoadingKey, isLoading.ToString());
        }

        public static void SendLevelLoad(string barcode, string loadBarcode)
        {
            if (!NetworkInfo.IsServer)
                return;

            using FusionWriter writer = FusionWriter.Create(SceneLoadData.GetSize(barcode, loadBarcode));
            var data = SceneLoadData.Create(barcode, loadBarcode);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.SceneLoad, writer);
            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
        }
    }
}
