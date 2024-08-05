using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Downloading;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.RPC;
using LabFusion.Utilities;

namespace LabFusion.Network;

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

public class PlayerRepAvatarMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepAvatar;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<PlayerRepAvatarData>();

        string barcode = data.barcode;

        // Swap the avatar for the rep
        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
        {
            player.AvatarSetter.SwapAvatar(data.stats, barcode);
        }

        // Bounce the message back
        if (NetworkInfo.IsServer)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
        }

        // Check if we need to install the avatar
        bool hasCrate = CrateFilterer.HasCrate<AvatarCrate>(new(barcode));

        if (!hasCrate)
        {
            // TODO: implement
            bool shouldDownload = true;

            // Check if we should download the mod (it's not blacklisted, mod downloading disabled, etc.)
            if (!shouldDownload)
            {
                return;
            }

            NetworkModRequester.RequestAndInstallMod(data.smallId, barcode, OnModDownloaded);

            void OnModDownloaded(DownloadCallbackInfo info)
            {
                if (info.result == ModResult.FAILED)
                {
                    FusionLogger.Warn($"Failed downloading avatar {barcode}!");
                    return;
                }

                if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
                {
                    // We just set the avatar dirty, so that if it's changed to another avatar by this point we aren't overriding it
                    player.AvatarSetter.SetAvatarDirty();
                }
            }

            return;
        }
    }
}