using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow;
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

public class PlayerRepAvatarMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepAvatar;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepAvatarData>();

        string barcode = data.barcode;

        // Check for avatar blacklist
        if (ModBlacklist.IsBlacklisted(barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Switching player avatar from {data.barcode} to PolyBlank because it is blacklisted!");
#endif

            barcode = BONELABAvatarReferences.PolyBlankBarcode;
        }

        // Swap the avatar for the rep
        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
        {
            player.AvatarSetter.SwapAvatar(data.stats, barcode);
        }
    }
}