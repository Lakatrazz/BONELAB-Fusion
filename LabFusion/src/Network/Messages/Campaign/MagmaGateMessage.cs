using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public enum MagmaGateEventType
{
    UNKNOWN = 0,
    BUTTONS_SETUP = 1,
    OBJECTIVE_COMPLETE_SETUP = 2,
    LOSE_SEQUENCE = 3,
    DOOR_DISSOLVE = 4,
}

public class MagmaGateEventData : INetSerializable
{
    public MagmaGateEventType type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref type, Precision.OneByte);
    }

    public static MagmaGateEventData Create(MagmaGateEventType type)
    {
        return new MagmaGateEventData()
        {
            type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class MagmaGateEventMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.MagmaGateEvent;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MagmaGateEventData>();

        var controller = MagmaGateData.GameController;

        if (!controller)
        {
            return;
        }

        MagmaGatePatches.IgnorePatches = true;

        switch (data.type)
        {
            default:
            case MagmaGateEventType.UNKNOWN:
                break;
            case MagmaGateEventType.BUTTONS_SETUP:
                controller.LevelSetup();
                break;
            case MagmaGateEventType.OBJECTIVE_COMPLETE_SETUP:
                controller.ObjectiveComplete();
                break;
            case MagmaGateEventType.LOSE_SEQUENCE:
                controller.LoseSequence();
                break;
            case MagmaGateEventType.DOOR_DISSOLVE:
                controller.DoorDissolve();
                break;
        }

        MagmaGatePatches.IgnorePatches = false;
    }
}
