using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Rig;
using LabFusion.Marrow.Data;
using LabFusion.Network.Messages;
using LabFusion.Network.Serialization;
using LabFusion.Safety;
using LabFusion.SDK.Modules;
using LabFusion.Network;

namespace LabFusion.Marrow.Messages;

public class RigAvatarData : INetSerializable
{
    public NetworkEntityReference RigReference;

    public SerializedAvatarStats Stats;

    public string Barcode;

    public int? GetSize() => NetworkEntityReference.Size + SerializedAvatarStats.Size + Barcode.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RigReference);
        serializer.SerializeValue(ref Stats);
        serializer.SerializeValue(ref Barcode);
    }
}

public class RigAvatarMessage : ModuleMessageHandler
{
    protected override bool OnPreRelayMessage(ReceivedMessage received) => CommonMessageValidation.ValidateSenderOwnsEntity(received);

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RigAvatarData>();

        string barcode = data.Barcode;

        // Check for avatar blacklist
        if (ModBlacklist.IsBlacklisted(barcode) || GlobalModBlacklistManager.IsBarcodeBlacklisted(barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Switching rig avatar from {data.Barcode} to the calibration avatar because it is blacklisted!");
#endif

            barcode = MarrowGameReferences.CalibrationAvatarReference.Barcode.ID;
        }

        if (!NetworkBeingManager.TryGetNetworkRig(data.RigReference, out var networkRig))
        {
            return;
        }

        networkRig.AvatarSetter.SwapAvatar(data.Stats, barcode);
    }
}