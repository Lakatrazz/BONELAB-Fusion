using LabFusion.Data;
using LabFusion.Downloading.ModIO;
using LabFusion.Player;
using LabFusion.RPC;

namespace LabFusion.Network;

public class ModInfoResponseData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(uint);

    public SerializedModIOFile modFile;

    public uint trackerId;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(modFile);

        writer.Write(trackerId);
    }

    public void Deserialize(FusionReader reader)
    {
        modFile = reader.ReadFusionSerializable<SerializedModIOFile>();

        trackerId = reader.ReadUInt32();
    }

    public static ModInfoResponseData Create(SerializedModIOFile modFile, uint trackerId)
    {
        return new ModInfoResponseData()
        {
            modFile = modFile,
            trackerId = trackerId,
        };
    }
}

public class ModInfoResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ModInfoResponse;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        // Read request
        var data = received.ReadData<ModInfoResponseData>();

        // Make sure we're the target
        if (received.Target != PlayerIdManager.LocalSmallId)
        {
            throw new Exception($"Received a ModInfoResponse, but we were not the desired target of {received.Target.Value}!");
        }

        // Run the callback
        NetworkModRequester.OnResponseReceived(data.trackerId, new NetworkModRequester.ModCallbackInfo()
        {
            modFile = data.modFile.File,
            hasFile = data.modFile.HasFile,
        });
    }
}