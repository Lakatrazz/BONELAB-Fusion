using LabFusion.Downloading.ModIO;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.RPC;

namespace LabFusion.Network;

public class ModInfoResponseData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(uint);

    public SerializedModIOFile ModFile;

    public uint TrackerID;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ModFile);
        serializer.SerializeValue(ref TrackerID);
    }

    public static ModInfoResponseData Create(SerializedModIOFile modFile, uint trackerID)
    {
        return new ModInfoResponseData()
        {
            ModFile = modFile,
            TrackerID = trackerID,
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
        if (received.Route.Target != PlayerIDManager.LocalSmallID)
        {
            throw new Exception($"Received a ModInfoResponse, but we were not the desired target of {received.Route.Target.Value}!");
        }

        // Run the callback
        NetworkModRequester.OnResponseReceived(data.TrackerID, new NetworkModRequester.ModCallbackInfo()
        {
            ModFile = data.ModFile.File,
            HasFile = data.ModFile.HasFile,
            Platform = data.ModFile.Platform,
        });;
    }
}