using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

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
    public MagmaGateEventType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class MagmaGateEventMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MagmaGateEventData>();

        var controller = MagmaGateEventHandler.GameController;

        if (!controller)
        {
            return;
        }

        GameControl_MagmaGatePatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
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
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling MagmaGateEventMessage", e);
        }

        GameControl_MagmaGatePatches.IgnorePatches = false;
    }
}
