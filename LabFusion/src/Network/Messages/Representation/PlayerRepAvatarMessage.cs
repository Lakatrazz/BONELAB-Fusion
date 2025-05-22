using LabFusion.Bonelab;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Safety;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class PlayerRepAvatarData : INetSerializable
{
    public const int DefaultSize = sizeof(byte) + SerializedAvatarStats.Size;

    public byte smallId;
    public SerializedAvatarStats stats;
    public string barcode;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref stats);
        serializer.SerializeValue(ref barcode);
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
        if (ModBlacklist.IsBlacklisted(barcode) || GlobalModBlacklistManager.IsBarcodeBlacklisted(barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Switching player avatar from {data.barcode} to PolyBlank because it is blacklisted!");
#endif

            barcode = BonelabAvatarReferences.PolyBlankBarcode;
        }

        // Swap the avatar for the rep
        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
        {
            player.AvatarSetter.SwapAvatar(data.stats, barcode);
        }
    }
}