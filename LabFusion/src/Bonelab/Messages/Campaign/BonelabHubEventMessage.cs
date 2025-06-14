using LabFusion.Bonelab.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Messages;

public enum BonelabHubEventType
{
    UNKNOWN = 0,
    ELEVATOR_BREAKOUT = 1,
    SETUP_ELEVATOR = 2,
    OPEN_BW_DOOR = 3,
    BW_BOX_DESTROYED = 4,
}

public class BonelabHubEventData : INetSerializable
{
    public BonelabHubEventType Type;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

[Net.DelayWhileTargetLoading]
public class BonelabHubEventMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<BonelabHubEventData>();

        var controller = BonelabHubEventHandler.GameController;

        if (controller == null)
        {
            return;
        }

        GameControl_HubPatches.IgnorePatches = true;

        try
        {
            switch (data.Type)
            {
                default:
                case BonelabHubEventType.UNKNOWN:
                    break;
                case BonelabHubEventType.ELEVATOR_BREAKOUT:
                    controller.ELEVATORBREAKOUT();
                    break;
                case BonelabHubEventType.SETUP_ELEVATOR:
                    controller.SETUPELEVATOR();
                    break;
                case BonelabHubEventType.OPEN_BW_DOOR:
                    controller.OPENBWDOOR();
                    break;
                case BonelabHubEventType.BW_BOX_DESTROYED:
                    controller.BWBOXDESTROYED();
                    break;
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling BonelabHubEventMessage", e);
        }

        GameControl_HubPatches.IgnorePatches = false;
    }
}