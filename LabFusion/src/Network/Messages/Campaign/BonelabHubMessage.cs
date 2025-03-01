using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network;

public enum BonelabHubEventType
{
    UNKNOWN = 0,
    ELEVATOR_BREAKOUT = 1,
    SETUP_ELEVATOR = 2,
    OPEN_BW_DOOR = 3,
    BW_BOX_DESTROYED = 4,
}

public class BonelabHubEventData : IFusionSerializable
{
    public BonelabHubEventType type;

    public void Serialize(FusionWriter writer)
    {
        writer.Write((byte)type);
    }

    public void Deserialize(FusionReader reader)
    {
        type = (BonelabHubEventType)reader.ReadByte();
    }

    public static BonelabHubEventData Create(BonelabHubEventType type)
    {
        return new BonelabHubEventData()
        {
            type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class BonelabHubEventMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.BonelabHubEvent;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<BonelabHubEventData>();

        GameControl_HubPatches.IgnorePatches = true;

        var controller = HubData.GameController;

        if (controller)
        {
            switch (data.type)
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

        GameControl_HubPatches.IgnorePatches = false;
    }
}