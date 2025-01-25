using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Player;

namespace LabFusion.Network;

public class DisconnectMessageData : IFusionSerializable
{
    public ulong longId;
    public string reason;

    public static DisconnectMessageData Create(ulong longId, string reason = "")
    {
        return new DisconnectMessageData()
        {
            longId = longId,
            reason = reason,
        };
    }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(longId);
        writer.Write(reason);
    }

    public void Deserialize(FusionReader reader)
    {
        longId = reader.ReadUInt64();
        reason = reader.ReadString();
    }
}

public class DisconnectMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.Disconnect;

    public override ExpectedType ExpectedReceiver => ExpectedType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        using var reader = FusionReader.Create(received.Bytes);
        var data = reader.ReadFusionSerializable<DisconnectMessageData>();

        // If this is our id, disconnect ourselves
        if (data.longId == PlayerIdManager.LocalLongId)
        {
            NetworkHelper.Disconnect(data.reason);
        }
        // Otherwise, disconnect the other person in the lobby
        else
        {
            InternalServerHelpers.OnUserLeave(data.longId);
        }
    }
}
