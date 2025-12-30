using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Network.Serialization;
using LabFusion.Safety;

namespace LabFusion.Network;

public class PlayerRepAvatarData : INetSerializable
{
    public SerializedAvatarStats Stats;

    public string Barcode;

    public int? GetSize() => SerializedAvatarStats.Size + Barcode.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Stats);
        serializer.SerializeValue(ref Barcode);
    }
}

public class PlayerRepAvatarMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepAvatar;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepAvatarData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        string barcode = data.Barcode;

        // Check for avatar blacklist
        if (ModBlacklist.IsBlacklisted(barcode) || GlobalModBlacklistManager.IsBarcodeBlacklisted(barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Switching player avatar from {data.barcode} to the calibration avatar because it is blacklisted!");
#endif

            barcode = MarrowGameReferences.CalibrationAvatarReference.Barcode.ID;
        }

        // Swap the avatar for the rep
        if (NetworkPlayerManager.TryGetPlayer(sender.Value, out var player))
        {
            player.AvatarSetter.SwapAvatar(data.Stats, barcode);
        }
    }
}