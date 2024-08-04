using LabFusion.Data;
using LabFusion.Downloading.ModIO;
using LabFusion.Player;
using LabFusion.RPC;

namespace LabFusion.Network;

public class ModInfoResponseData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(uint);

    public byte target;

    public SerializedModIOFile modFile;

    public uint trackerId;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(target);

        writer.Write(modFile);

        writer.Write(trackerId);
    }

    public void Deserialize(FusionReader reader)
    {
        target = reader.ReadByte();

        modFile = reader.ReadFusionSerializable<SerializedModIOFile>();

        trackerId = reader.ReadUInt32();
    }

    public static ModInfoResponseData Create(byte target, SerializedModIOFile modFile, uint trackerId)
    {
        return new ModInfoResponseData()
        {
            target = target,
            modFile = modFile,
            trackerId = trackerId,
        };
    }
}

public class ModInfoResponseMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.ModInfoResponse;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // Read request
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<ModInfoResponseData>();

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
            throw new Exception($"Received a ModInfoResponse, but we were not the desired target of {data.target}!");
        }

        // Run the callback
        NetworkModRequester.OnResponseReceived(data.trackerId, new NetworkModRequester.ModCallbackInfo()
        {
            modFile = data.modFile.File,
            hasFile = data.modFile.HasFile,
        });
    }
}