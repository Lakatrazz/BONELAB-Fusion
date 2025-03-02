using LabFusion.Data;
using LabFusion.Downloading.ModIO;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.RPC;

namespace LabFusion.Network;

public class ModInfoResponseData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(uint);

    public SerializedModIOFile modFile;

    public uint trackerId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref modFile);
        serializer.SerializeValue(ref trackerId);
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