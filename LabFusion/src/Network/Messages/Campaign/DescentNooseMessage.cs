using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public enum DescentNooseType
{
    UNKNOWN = 0,
    ATTACH_NOOSE = 1,
    CUT_NOOSE = 2,
}

public class DescentNooseData : INetSerializable
{
    public byte smallId;
    public DescentNooseType type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref type, Precision.OneByte);
    }

    public static DescentNooseData Create(byte smallId, DescentNooseType type)
    {
        return new DescentNooseData()
        {
            smallId = smallId,
            type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class DescentNooseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.DescentNoose;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DescentNooseData>();

        // Send message to other clients if server
        if (received.IsServerHandled)
        {
            using var message = FusionMessage.Create(Tag, received);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            return;
        }

        NoosePatches.IgnorePatches = true;

        // Register a noose event for catchup
        _ = DescentData.CreateNooseEvent(data.smallId, data.type);

        switch (data.type)
        {
            default:
            case DescentNooseType.UNKNOWN:
                break;
            case DescentNooseType.ATTACH_NOOSE:
                if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
                {
                    // Assign the RigManager and Health to the noose
                    // We assign the rigmanager so the noose knows what neck to joint to
                    // The player health is also assigned so it doesn't damage the local player
                    DescentData.Noose.rM = player.RigRefs.RigManager;
                    DescentData.Noose.pH = player.RigRefs.Health;

                    // Now we actually attach the neck of the player
                    DescentData.Noose.AttachNeck();
                }
                break;
            case DescentNooseType.CUT_NOOSE:
                // This function is called to cut the noose as if a knife cut it
                DescentData.Noose.NooseCut();

                DescentData.CheckAchievement();
                break;
        }

        NoosePatches.IgnorePatches = false;
    }
}