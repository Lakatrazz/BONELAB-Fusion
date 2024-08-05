using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Downloading.ModIO;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Extensions;
using LabFusion.Exceptions;
using LabFusion.Senders;
using LabFusion.RPC;
using LabFusion.Marrow;
using LabFusion.Downloading;

namespace LabFusion.Network
{
    public class PlayerRepAvatarData : IFusionSerializable
    {
        public const int DefaultSize = sizeof(byte) + SerializedAvatarStats.Size;

        public byte smallId;
        public SerializedAvatarStats stats;
        public string barcode;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(stats);
            writer.Write(barcode);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            stats = reader.ReadFusionSerializable<SerializedAvatarStats>();
            barcode = reader.ReadString();
        }

        public static PlayerRepAvatarData Create(byte smallId, SerializedAvatarStats stats, string barcode)
        {
            return new PlayerRepAvatarData()
            {
                smallId = smallId,
                stats = stats,
                barcode = barcode
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class PlayerRepAvatarMessage : FusionMessageHandler
    {
        public override byte Tag => NativeMessageTag.PlayerRepAvatar;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<PlayerRepAvatarData>();

            if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
            {
                if (!AvatarExists(data.barcode))
                {
                    RequestAndDownloadAvatar(data, player);
                }
                else
                {
                    player.AvatarSetter.SwapAvatar(data.stats, data.barcode);
                }
            }

            if (NetworkInfo.IsServer)
            {
                using var message = FusionMessage.Create(Tag, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
            }
        }

        private bool AvatarExists(string barcode)
        {
            // Implement logic to check if the avatar with the given barcode exists locally
            return false;
        }

        private void RequestAndDownloadAvatar(PlayerRepAvatarData data, NetworkPlayer player)
        {
            NetworkModRequester.RequestMod(new NetworkModRequester.ModRequestInfo()
            {
                target = data.smallId,
                barcode = data.barcode,
                modCallback = OnModInfoReceived,
            });

            void OnModInfoReceived(NetworkModRequester.ModCallbackInfo info)
            {
                if (!info.hasFile)
                {
                    return;
                }

                ModIODownloader.EnqueueDownload(new ModTransaction()
                {
                    modFile = info.modFile,
                    temporary = true,
                    callback = OnModDownloaded,
                });
            }

            void OnModDownloaded(DownloadCallbackInfo info)
            {
                if (info.result == ModResult.FAILED)
                {
                    FusionLogger.Warn($"Failed downloading avatar {data.barcode}!");
                    return;
                }

                player.AvatarSetter.SwapAvatar(data.stats, data.barcode);
            }
        }
    }
}
