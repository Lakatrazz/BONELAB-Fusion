using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Downloading.ModIO;
using LabFusion.Marrow;
using LabFusion.Network.Serialization;
using LabFusion.Player;

namespace LabFusion.Network;

public class ModInfoRequestData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(uint);

    public string barcode;

    public uint trackerId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref barcode);
        serializer.SerializeValue(ref trackerId);
    }

    public static ModInfoRequestData Create(string barcode, uint trackerId)
    {
        return new ModInfoRequestData()
        {
            barcode = barcode,
            trackerId = trackerId,
        };
    }
}

public class ModInfoRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ModInfoRequest;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        // Read request
        var data = received.ReadData<ModInfoRequestData>();

        // Make sure we're the target
        if (received.Target != PlayerIdManager.LocalSmallId)
        {
            throw new Exception($"Received a ModInfoRequest, but we were not the desired target of {received.Target.Value}!");
        }

        // Get the crate from the barcode
        var crate = CrateFilterer.GetCrate<Crate>(new(data.barcode));

        if (crate == null)
        {
            return;
        }

        var pallet = crate.Pallet;

        // Make sure the pallet isn't part of a marrow game and is a mod
        if (pallet.IsInMarrowGame())
        {
            return;
        }

        // Get the mod info
        var manifest = CrateFilterer.GetManifest(pallet);

        if (manifest == null)
        {
            return;
        }

        var modListing = manifest.ModListing;

        var modTarget = ModIOManager.GetTargetFromListing(modListing);

        if (modTarget == null)
        {
            return;
        }

        // Write and send response
        var modFile = new SerializedModIOFile(new ModIOFile((int)modTarget.ModId, (int)modTarget.ModfileId));

        var writtenData = ModInfoResponseData.Create(modFile, data.trackerId);

        MessageRelay.RelayNative(writtenData, NativeMessageTag.ModInfoResponse, NetworkChannel.Reliable, RelayType.ToTarget, received.Sender.Value);
    }
}