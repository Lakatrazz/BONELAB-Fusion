using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Downloading.ModIO;
using LabFusion.Marrow;
using LabFusion.Player;

namespace LabFusion.Network;

public class ModInfoRequestData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(uint);

    public byte requester;
    public byte target;

    public string barcode;

    public uint trackerId;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(requester);
        writer.Write(target);

        writer.Write(barcode);

        writer.Write(trackerId);
    }

    public void Deserialize(FusionReader reader)
    {
        requester = reader.ReadByte();
        target = reader.ReadByte();

        barcode = reader.ReadString();

        trackerId = reader.ReadUInt32();
    }

    public static ModInfoRequestData Create(byte requester, byte target, string barcode, uint trackerId)
    {
        return new ModInfoRequestData()
        {
            requester = requester,
            target = target,
            barcode = barcode,
            trackerId = trackerId,
        };
    }
}

public class ModInfoRequestMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.ModInfoRequest;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // Read request
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<ModInfoRequestData>();

        // If we're the server, send to the desired recipient
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.SendFromServer(data.target, NetworkChannel.Reliable, message);
            return;
        }

        // Make sure we're the target
        if (data.target != PlayerIdManager.LocalSmallId)
        {
            throw new Exception($"Received a ModInfoRequest, but we were not the desired target of {data.target}!");
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

        using var writer = FusionWriter.Create(ModInfoResponseData.Size);
        var writtenData = ModInfoResponseData.Create(data.requester, modFile);
        writer.Write(writtenData);
        
        using var response = FusionMessage.Create(NativeMessageTag.ModInfoResponse, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, response);
    }
}